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
using System.Threading;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;

namespace Gears.Interpreter.Library
{
    public class Fill : Keyword, IHasTechnique, IInstructed
    {
        #region Semantics
        public int Order { get; set; }

        public string Text { get; set; }

        public string LabelText { get; set; }

        public SearchDirection Direction { get; set; }

        public Technique Technique { get; set; }

        public void MapSyntaxToSemantics(Instruction instruction)
        {
            if (string.IsNullOrEmpty(instruction.Locale))
            {
                LabelText = instruction.SubjectName;
            }
            else
            {
                LabelText = instruction.Locale;
            }

            Direction = instruction.Direction;
            Text = instruction.With;
            Order = instruction.Order;
        }  
        #endregion

        public Fill(string what)
        {
            MapSyntaxToSemantics(new Instruction(what));
        }

        public Fill(string what, string text) : this(what)
        {
            Text = text;
        }

        public override object Run()
        {
            var searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);

            var lookupResult = searchStrategy.DirectLookupWithNeighbours(LabelText, Direction, Order);

            if (lookupResult.Success == false)
            {
                throw new LookupFailureException(lookupResult, "Input not found");
            }

            switch (Technique)
            {
                case Technique.HighlightOnly:
                    Show.HighlightElements(Selenium, lookupResult.OtherValidResults);
                    break;
                case Technique.Javascript:
                    lookupResult.Result.WebElement.SendKeys(Text);
                    break;
                case Technique.MouseAndKeyboard:
                    var handle = Selenium.GetChromeHandle();

                    var screenLocation = Selenium.PutElementOnScreen(lookupResult.Result.WebElement);

                    UserInteropAdapter.ClickOnPoint(handle, screenLocation);
                    Thread.Sleep(50);
                    UserInteropAdapter.SendText(handle, Text, screenLocation);
                    Thread.Sleep(50);
                    UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());
                    break;
            }

            return lookupResult.Result;
        }

        public override string ToString()
        {
            return $"Fill '{LabelText}' with '{Text}'";
        }

        #region Backward compatibility

        [Obsolete("Backward compatibility")]
        public Fill(string what, string where, string text) : this(what, text)
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

        [Obsolete("Backward compatibility")]
        public bool Javascript
        {
            get { return Technique == Technique.Javascript; }
            set { Technique = value == true ? Technique.Javascript : Technique.MouseAndKeyboard; }
        }

        #endregion
    }
}
