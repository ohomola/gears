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
using System.Drawing;
using System.Threading;
using Castle.Core.Internal;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [UserDescription("hastext <inst> \t-\t checks if text of an element equals to expected value")]
    public class HasText : Keyword, IHasTechnique, IInstructed, IAssertion
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
Checks a text input element (or dropdown) located by a visible text on the screen. The input parameter is a query instruction passed as a string to parameter **What**. 

>The HasText understands simple instructions differently than Click , which means you do not need to specify 'input next to login' each time you are looking for input labelled 'login'. Instead you can ask just for 'login' - HasText assumes you're looking for something that has a label next to it somewhere.

**Text** parameter is the text expected.

#### Scenario usages
| Discriminator | What  | Text  | Expect |
| ------------- | ----- | ----- | ------ |
| HasText       | login | user1 | TRUE   |
| HasText       | input right from login | password | TRUE |

#### Console usages
    hastext login myname

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        #endregion

        public HasText()
        {
        }

        public HasText(string what)
        {
            MapSyntaxToSemantics(new Instruction(what));
        }

        public HasText(string what, string text) : this(what)
        {
            Text = text;
        }

        public override IKeyword FromString(string textInstruction)
        {
            return new HasText(ExtractSingleParameterFromTextInstruction(textInstruction)) {Expect = true};
        }

        public override object DoRun()
        {
            var searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);

            var lookupResult = searchStrategy.DirectLookupWithNeighbours(LabelText, Direction, Order);

            if (Interpreter?.IsAnalysis == true)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, "DirectLookup (exact matches): " + _instruction?.ToAnalysisString());
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{lookupResult.Result}\nAll results:\n\t{string.Join("\n\t", lookupResult.OtherValidResults)}");
            }

            if (ExactMatch == false && lookupResult.Success == false)
            {
                lookupResult = searchStrategy.DirectLookupWithNeighbours(LabelText, Direction, Order, false);

                if (Interpreter?.IsAnalysis == true)
                {
                    Console.Out.WriteColoredLine(ConsoleColor.Magenta, "DirectLookup (all matches): " + _instruction?.ToAnalysisString());
                    Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{lookupResult.Result}\nAll results:\n\t{string.Join("\n\t", lookupResult.OtherValidResults)}");
                }
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
                case Technique.MouseAndKeyboard:

                    Selenium.BringToFront();
                    if (Interpreter?.IsDebugMode == true)
                    {
                        Highlighter.HighlightElements(750, Selenium, new[] { lookupResult.Result }, Color.Aqua, Color.Red, -1, Color.Aqua);
                    }

                    Actual = lookupResult.Result.WebElement.Text;
                    return Actual?.ToLower() == Text.ToLower();

            }

            return null;
        }

        public string Actual { get; set; }

        public override string ToString()
        {
            return $"HasText {Order}. '{LabelText}' with '{Text}': " +
                (Actual.IsNullOrEmpty()?"":$"Expected {((bool)Expect ? "" : "not to be")}:'{Text}'\nActual:'{Actual}'\n");
        }
    }
}
