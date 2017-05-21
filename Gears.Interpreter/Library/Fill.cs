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
using System.Drawing;
using System.Threading;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core;
using Gears.Interpreter.Library.Lookup;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [UserDescription("fill <inst> \t-\t fills element via instruction")]
    public class Fill : Keyword, IHasTechnique, IInstructed
    {
        private Instruction _instruction;

        #region Semantics
        public virtual int Order { get; set; }

        public virtual string Text { get; set; }

        public virtual string LabelText { get; set; }

        public virtual SearchDirection Direction { get; set; }

        public virtual Technique Technique { get; set; }

        public bool ExactMatch { get; set; }

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
            _instruction = instruction;
            ExactMatch = instruction.Accuracy != Accuracy.Partial;
        }

        #endregion

        #region Documentation

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Fills a text input element (or dropdown) located by a visible text on the screen. The input parameter is a query instruction passed as a string to parameter **What**. 

>The Fill understands simple instructions differently than Click , which means you do not need to specify 'input next to login' each time you are looking for input labelled 'login'. Instead you can ask just for 'login' - Fill assumes you're looking for something that has a label next to it somewhere.

**Text** parameter is the text to be filled into the element.

#### Scenario usages
| Discriminator | What  | Text  |
| ------------- | ----- | ----- |
| Fill          | login | user1 |
| Fill          | input right from login | password |

#### Console usages
    fill login with user1
    fill input right from login with password
    show fill login with user1

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
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
            var lookupResult = new TextFieldLookupStrategy(Selenium, ExactMatch, Order, Direction, LabelText).LookUp();

            switch (Technique)
            {
                case Technique.HighlightOnly:
                    Highlighter.HighlightElements(Selenium, lookupResult.AllValidResults);
                    return new InformativeAnswer("Highlighting complete.");
                case Technique.Javascript:
                    lookupResult.MainResult.WebElement.SendKeys(Text);
                    break;
                case Technique.MouseAndKeyboard:
                    var handle = Selenium.GetChromeHandle();

                    var screenLocation = Selenium.PutElementOnScreen(lookupResult.MainResult.WebElement);

                    Selenium.BringToFront();
                    if (Interpreter?.IsAnalysis == true)
                    {
                        Highlighter.HighlightElements(750, Selenium, new[] { lookupResult.MainResult }, Color.Aqua, Color.Red, -1, Color.Aqua);
                    }

                    UserInteropAdapter.ClickOnPoint(handle, screenLocation);
                    Thread.Sleep(50);
                    UserInteropAdapter.SendText(handle, Text, screenLocation);
                    Thread.Sleep(50);
                    break;
            }

            return new SuccessAnswer("Fill successful.");
        }

        private LookupResult Lookup()
        {
            var searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);

            var lookupResult = searchStrategy.DirectLookupWithNeighbours(LabelText, Direction, Order);

            if (Interpreter?.IsAnalysis == true)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Magenta,
                    "DirectLookup (exact matches): " + _instruction?.ToAnalysisString());
                Console.Out.WriteColoredLine(ConsoleColor.Magenta,
                    $"Main Result: \n\t{lookupResult.MainResult}\nAll results:\n\t{string.Join("\n\t", lookupResult.AllValidResults)}");
            }

            if (ExactMatch == false && lookupResult.Success == false)
            {
                lookupResult = searchStrategy.DirectLookupWithNeighbours(LabelText, Direction, Order, false);

                if (Interpreter?.IsAnalysis == true)
                {
                    Console.Out.WriteColoredLine(ConsoleColor.Magenta,
                        "DirectLookup (all matches): " + _instruction?.ToAnalysisString());
                    Console.Out.WriteColoredLine(ConsoleColor.Magenta,
                        $"Main Result: \n\t{lookupResult.MainResult}\nAll results:\n\t{string.Join("\n\t", lookupResult.AllValidResults)}");
                }
            }

            if (lookupResult.Success == false)
            {
                throw new LookupFailureException(lookupResult, "Input not found");
            }
            return lookupResult;
        }

        public override string ToString()
        {
            return $"Fill {Order+1}. '{LabelText}' with '{Text}'";
        }

        #region Backward compatibility

        //[Obsolete("Backward compatibility")]
        //public Fill(string what, string where, string text) : this(what, text)
        //{
        //    where = @where.ToLower().Trim();
        //    switch (@where)
        //    {
        //        case ("right"):
        //            Direction = SearchDirection.LeftFromRightEdge;
        //            break;
        //        case ("top"):
        //        case ("up"):
        //            Direction = SearchDirection.DownFromTopEdge;
        //            break;
        //        case ("down"):
        //        case ("bottom"):
        //            Direction = SearchDirection.UpFromBottomEdge;
        //            break;
        //        default:
        //            Direction = SearchDirection.RightFromLeftEdge;
        //            break;
        //    }
        //}

        //[Obsolete("Backward compatibility")]
        //public virtual bool Javascript
        //{
        //    get { return Technique == Technique.Javascript; }
        //    set { Technique = value == true ? Technique.Javascript : Technique.MouseAndKeyboard; }
        //}

        #endregion
    }
}
