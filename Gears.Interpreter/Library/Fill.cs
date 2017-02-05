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
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [UserDescription("fill <inst> \t-\t fills element via instruction")]
    public class Fill : Keyword, IHasTechnique, IInstructed
    {
        #region Semantics
        public virtual int Order { get; set; }

        public virtual string Text { get; set; }

        public virtual string LabelText { get; set; }

        public virtual SearchDirection Direction { get; set; }

        public virtual Technique Technique { get; set; }

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

        public Fill()
        {
        }

        public Fill(string what)
        {
            MapSyntaxToSemantics(new Instruction(what));
        }

        public Fill(string what, string text) : this(what)
        {
            Text = text;
        }

        public override IKeyword FromString(string textInstruction)
        {
            return new Fill(ExtractSingleParameterFromTextInstruction(textInstruction));
        }

        public override object DoRun()
        {
            var searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);

            var lookupResult = searchStrategy.DirectLookupWithNeighbours(LabelText, Direction, Order);

            if (Interpreter?.IsAnalysis == true)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{lookupResult.Result}\nAll results:\n\t{string.Join("\n\t", lookupResult.OtherValidResults)}");
            }

            if (lookupResult.Success == false)
            {
                throw new LookupFailureException(lookupResult, "Input not found");
            }

            switch (Technique)
            {
                case Technique.HighlightOnly:
                    Highlighter.HighlightElements(Selenium, lookupResult.OtherValidResults);
                    return new InformativeAnswer("Highlighting complete.");
                case Technique.Javascript:
                    lookupResult.Result.WebElement.SendKeys(Text);
                    break;
                case Technique.MouseAndKeyboard:
                    var handle = Selenium.GetChromeHandle();

                    var screenLocation = Selenium.PutElementOnScreen(lookupResult.Result.WebElement);

                    Selenium.BringToFront();
                    UserInteropAdapter.ClickOnPoint(handle, screenLocation);
                    Thread.Sleep(50);
                    UserInteropAdapter.SendText(handle, Text, screenLocation);
                    Thread.Sleep(50);
                    break;
            }

            return new SuccessAnswer("Fill successful.");
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
        public virtual bool Javascript
        {
            get { return Technique == Technique.Javascript; }
            set { Technique = value == true ? Technique.Javascript : Technique.MouseAndKeyboard; }
        }

        #endregion
    }
}
