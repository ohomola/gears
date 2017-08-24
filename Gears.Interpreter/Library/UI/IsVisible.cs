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
using System.Linq;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Extensions;

namespace Gears.Interpreter.Library.UI
{
    [UserDescription("isvisible <i>\t-\t checks if an element is visible on screen")]
    public class IsVisible : Keyword, IAssertion
    {
        private  Instruction spec;
        public virtual List<ITagSelector> TagNames { get; set; }

        public virtual string SubjectName { get; set; }

        public virtual string Locale { get; set; }

        public virtual SearchDirection Direction { get; set; }

        public virtual int Order { get; set; }



        public virtual string What
        {
            set
            {
                MapSyntaxToSemantics(new Instruction(value));
            }
        }


        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Checks the presence of a web element or text in the browser window. The input parameter is a query instruction passed as a string to parameter 'What'. See [Web element instructions](#web-element-instructions) for more info.

#### Scenario usages
| Discriminator | What | Expect |
| ------------- | ---- | ----|
| IsVisible     | Save | true |
| IsVisible     | 1st button 'save customer' below 'New Customer'| false |
| IsVisible     | 4th button from right| true |

#### Console usages
    IsVisible save
    IsVisible 4th button from right

> Note: console usages always Expect true result (you cannot specify Expect parameter when calling the keyword from console)

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        public IsVisible()
        {
        }

        public IsVisible(string what)
        {
            MapSyntaxToSemantics(new Instruction(what));
        }

        private void MapSyntaxToSemantics(Instruction instruction)
        {
            SubjectName = instruction.SubjectName;
            Locale = instruction.Locale;
            Direction = instruction.Direction;
            Order = instruction.Order;
            TagNames = instruction.TagNames;
            spec = instruction;
            ExactMatch = instruction.Accuracy != Accuracy.Partial;
        }

        public bool ExactMatch { get; set; }


        public override IKeyword FromString(string textInstruction)
        {
            return new IsVisible() {What = ExtractSingleParameterFromTextInstruction(textInstruction), Expect = true};
        }

        public override object DoRun()
        {
            var lookupResult = new DirectLookupStrategy(Selenium,TagNames, SubjectName, Locale, Direction, Order, false, ExactMatch).LookUp();

            if (Interpreter?.IsAnalysis == true)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{lookupResult.MainResult}\nAll results:\n\t{string.Join("\n\t", lookupResult.AllValidResults)}");
            }

            var visibleArea = Selenium.GetChromeBox();
            var visibles = lookupResult.AllValidResults.Where(x => IsInsideBoundingBox(x, visibleArea)).ToList();

            if (Interpreter?.IsDebugMode == true && lookupResult.AllValidResults.Any())
            {
                var passedExpectations = Expect.ToString().ToLower().Equals(true.ToString().ToLower());
                Highlighter.HighlightElements(1250, Selenium, lookupResult.AllValidResults,
                (passedExpectations? Color.Aqua: Color.Chocolate), Color.FromArgb(4,4,4), Order, (passedExpectations ? Color.FromArgb(0,255,0) : Color.Red));
            }

            return visibles.Count > Order;


            //for (int index = 0; index < lookupResult.AllValidResults.Count(); index++)
            //{
            //    var e = lookupResult.AllValidResults.ElementAt(index);

            //    var isInsideBoundingBox = IsInsideBoundingBox(e, Selenium.GetChromeBox());

            //    if (isInsideBoundingBox)
            //    {
            //        if (Interpreter?.IsDebugMode == true)
            //        {
            //            Highlighter.HighlightElements(1250, Selenium, lookupResult.AllValidResults,
            //            (Expect.ToString().ToLower().Equals(true.ToString().ToLower())
            //                ? Color.GreenYellow
            //                : Color.Red), Color.Yellow, index, Color.Black);
            //        }
            //        return true;
            //    }
            //}


            //return false;

            //throw new NotImplementedException($"Checking visibility of nth ({Order}) elements is not implemented.");
        }

        private bool IsInsideBoundingBox(IBufferedElement e, UserBindings.RECT browserBox)
        {
            Selenium.PutElementOnScreen(e.WebElement);

            var refreshedPosition = e.WebElement.AsBufferedElement().Rectangle;

            var centerX = refreshedPosition.X + refreshedPosition.Width/2;
            var centerY = refreshedPosition.Y + refreshedPosition.Height/2;

            var p = new Point(centerX, centerY);
            Selenium.ConvertFromPageToWindow(ref p);

            var isOutsideBoundingBox = p.X < 0 || p.X > browserBox.Right || p.Y < 0 || p.Y > browserBox.Bottom;
            return !isOutsideBoundingBox;
        }


        //private bool CheckTextOnly(string what)
        //{
        //    var oldClipboard = Clipboard.GetDataObject();

        //    var chromeHandle = Selenium.GetChromeHandle();
        //    UserBindings.SetForegroundWindow(chromeHandle);
        //    var pointOnScreen = new Point(100,10);
        //    UserBindings.ClientToScreen(chromeHandle, ref pointOnScreen);
        //    UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), pointOnScreen);

        //    UserInteropAdapter.PressWithControl(chromeHandle, 0x41);
        //    UserInteropAdapter.PressWithControl(chromeHandle, 0x43);

        //    UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), pointOnScreen);

        //    var text = Clipboard.GetText(TextDataFormat.UnicodeText);
        //    if (oldClipboard != null)
        //    {
        //        Clipboard.SetDataObject(oldClipboard);
        //    }
        //    UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());

        //    if (text.ToLower().Contains(what.ToLower()))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public override string ToString()
        {
            return $"Is {spec} visible? Expect {Expect}";
        }
    }
}
