#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears, a software automation and assistance framework.

Gears is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Gears is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.Extensions;

namespace Gears.Interpreter.Library
{
    public interface IHasTechnique
    {
        Technique Technique { get; set; }
    }

    public class Click : Keyword, IHasTechnique, IInstructed
    {
        #region Semantics

        public int Order { get; set; }
        public SubjectType SearchedType { get; set; }
        public List<string> SearchedTagNames { get; set; }
        public string VisibleTextOfTheButton { get; set; }
        public SearchDirection Direction { get; set; }
        public string NeighbourToLookFrom { get; set; }
        //TODO parse from instruction
        public bool LookForOrthogonalNeighboursOnly { get; set; }
        //TODO parse from instruction
        public Technique Technique { get; set; }
        
        public void MapSyntaxToSemantics(Instruction instruction)
        {
            Order = instruction.Order;
            SearchedTagNames = instruction.TagNames;
            SearchedType = instruction.SubjectType;
            VisibleTextOfTheButton = instruction.SubjectName;
            Direction = instruction.Direction;
            NeighbourToLookFrom = instruction.Locale;
        }

        #endregion

        public Click(string what)
        {
            MapSyntaxToSemantics(new Instruction(what));
        }

        public override object Run()
        {
            var query = new LocationHeuristictSearchStrategy(Selenium);

            var result = query.DirectLookup(SearchedTagNames, VisibleTextOfTheButton, NeighbourToLookFrom, Direction, Order, LookForOrthogonalNeighboursOnly);

            if (result.Success == false)
            {
                throw new LookupFailureException(result, $"Cannot find element {(Order > 0 ? (Order + 1).ToString() : "")}({result.OtherValidResults.Count()} results found)");
            }

            switch (Technique)
            {
                case Technique.HighlightOnly:
                    Show.HighlightElements(Selenium, result.OtherValidResults, Order);
                    break;
                case Technique.Javascript:
                    Selenium.WebDriver.Click(result.Result.WebElement);
                    break;
                case Technique.MouseAndKeyboard:
                    var screenLocation = Selenium.PutElementOnScreen(result.Result.WebElement);
                    UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), screenLocation);
                    Thread.Sleep(50);
                    UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());
                    break;
            }

            return result.Result;
        }
        
        public override string ToString()
        {
            return $"Click {(Order+1).ToOrdinalString()} {(SearchedType == default(SubjectType) ? "" : SearchedType.ToString())} {VisibleTextOfTheButton} {NeighbourToLookFrom} {(Direction==default(SearchDirection)?"":Direction.ToString())}";
        }

        
        #region Backward compatibility

        [Obsolete("Backward compatibility")]
        public bool Javascript
        {
            get { return Technique == Technique.Javascript; }
            set { Technique = value == true ? Technique.Javascript : Technique.MouseAndKeyboard; }
        }

        [Obsolete("Backward compatibility")]
        public Click(string what, string where) : this(what)
        {
            where = @where.ToLower().Trim();
            switch (@where)
            {
                case ("right"):
                    Direction = SearchDirection.LeftFromRightEdge;
                    break;
                case ("top"):
                case ("up"):
                    Direction = SearchDirection.DownFromTopEdge;
                    break;
                case ("down"):
                case ("bottom"):
                    Direction = SearchDirection.UpFromBottomEdge;
                    break;
                default:
                    Direction = SearchDirection.RightFromLeftEdge;
                    break;
            }
        } 
        #endregion
    }

    public enum Technique
    {
        MouseAndKeyboard = 0,
        Javascript,
        HighlightOnly
    }


    
}
