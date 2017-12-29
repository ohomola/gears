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
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.Library.UI
{
    [HelpDescription("fill <inst> \t-\t fills element via instruction")]
    public class Fill : Keyword, IHasTechnique, IInstructed
    {

        public virtual string Text { get; set; }
        public virtual string LabelText { get; set; }
        public virtual int Order { get; set; }
        public virtual SearchDirection Direction { get; set; }
        public virtual Technique Technique { get; set; }
        public virtual bool ExactMatch { get; set; }

        // RICH SYNTACTIC PROPERTY
        public virtual string What
        {
            set => MapRichSyntaxToSemantics(new Instruction(value));
        }

        public void MapRichSyntaxToSemantics(Instruction instruction)
        {
            if (!string.IsNullOrEmpty(instruction.Locale))
            {
                LabelText = instruction.Locale;
            }
            else
            {
                LabelText = instruction.SubjectName ?? LabelText;
            }

            Direction = instruction.Direction ?? Direction;
            Text = instruction.With ?? Text;
            Order = instruction.Order ?? Order;

            ExactMatch = instruction.Accuracy != CompareAccuracy.Partial;
        }

        public Fill()
        {
        }

        public Fill(string what)
        {
            What = what;
        }

        public Fill(string what, string text) : this(what)
        {
            Text = text;
        }

        public override string ToString()
        {
            return $"Fill {Order + 1}. '{LabelText}' with '{Text}'";
        }

        public override void FromString(string textInstruction)
        {
            What = textInstruction;
        }

        public override object DoRun()
        {
            var lookupResult = new TextFieldLookupStrategy(Selenium, ExactMatch, Order, Direction, LabelText).LookUp();

            switch (Technique)
            {
                case Technique.Show:

                    Highlighter.HighlightElements(Selenium, lookupResult.AllValidResults);
                    return new InformativeAnswer("Highlighting complete.");

                case Technique.Javascript:

                    lookupResult.MainResult.WebElement.SendKeys(Text);
                    break;

                case Technique.MouseAndKeyboard:

                    var handle = Selenium.BrowserHandle;

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

                default:
                    throw new NotSupportedException($"Keyword {GetType().Name} cannot be run using {Technique}");
            }

            return new SuccessAnswer("Fill successful.");
        }

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
    }
}
