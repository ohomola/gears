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
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Extensions;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library.UI
{
    [HelpDescription("isselected <i>\t-\t checks if a checkbox or similar input is selected")]
    public class IsSelected : Keyword, IAssertion
    {
        private  WebElementInstruction spec;
        public virtual List<ITagSelector> TagNames { get; set; }

        public virtual string SubjectName { get; set; }

        public virtual string Locale { get; set; }

        public virtual SearchDirection Direction { get; set; }

        public virtual int Order { get; set; }

        public virtual string What
        {
            set
            {
                MapSyntaxToSemantics(new WebElementInstruction(value));
            }
        }


        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Checks if a checkbox or similar input is selected

#### Scenario usages
| Discriminator | What | Expect |
| ------------- | ---- | ----|
| IsSelected     | Accept Terms and Conditions | true |

#### Console usages
    IsEnabled Accept Terms and Conditions

> Note: console usages always Expect true result (you cannot specify Expect parameter when calling the keyword from console)

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        public IsSelected()
        {
        }

        public IsSelected(string what)
        {
            MapSyntaxToSemantics(new WebElementInstruction(what));
        }

        private void MapSyntaxToSemantics(WebElementInstruction instruction)
        {
            SubjectName = instruction.SubjectName ?? SubjectName;
            Locale = instruction.Locale ?? Locale;
            Direction = instruction.Direction ?? Direction;
            Order = instruction.Order ?? Order;
            TagNames = instruction.TagNames ?? TagNames;
            spec = instruction;
        }

        [Wire]
        public IBrowserOverlay BrowserOverlay { get; set; }

        public override string Instruction
        {
            set
            {
                Expect = true;
                What = value;
            }
        }

        public override object DoRun()
        {
            var searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);
            var lookupResult = searchStrategy.DirectLookup(TagNames, SubjectName, Locale, Direction, Order, false);

            if (Interpreter?.IsAnalysis == true)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{lookupResult.MainResult}\nAll results:\n\t{string.Join("\n\t", lookupResult.AllValidResults)}");
            }

            var chromeHandle = Selenium.BrowserHandle;
            var browserBox = new UserBindings.RECT();
            UserBindings.GetWindowRect(chromeHandle, ref browserBox);

            var e = lookupResult.AllValidResults.ElementAt(Order);

            Selenium.PutElementOnScreen(e.WebElement);

            var refreshedPosition = e.WebElement.AsBufferedElement().Rectangle;

            var centerX = refreshedPosition.X + refreshedPosition.Width/2;
            var centerY = refreshedPosition.Y + refreshedPosition.Height/2;

            var p = new Point(centerX, centerY);
            Selenium.ConvertFromPageToWindow(ref p);

            if (p.X < 0 || p.X > browserBox.Right || p.Y < 0 || p.Y> browserBox.Bottom)
            {
                throw new NotFoundException("Element not found");
            }
            else
            {
                if (Interpreter?.IsDebugMode == true)
                {
                    BrowserOverlay
                        .HighlightElements((Order + 1).ToString(), (Expect.ToString().ToLower().Equals(true.ToString().ToLower()) ? Color.GreenYellow : Color.Red), lookupResult.MainResult)
                        .ShowFor(750, "Highlighted element will be Clicked");
                }
                return lookupResult.AllValidResults.ElementAt(Order).WebElement.Selected;
            }

            throw new NotFoundException("Element not found");

            throw new NotImplementedException($"Checking visibility of nth ({Order}) elements is not implemented.");
        }

        

        //private bool CheckTextOnly(string what)
        //{
        //    var oldClipboard = Clipboard.GetDataObject();

        //    var chromeHandle = Selenium.BrowserHandle();
        //    UserBindings.SetForegroundWindow(chromeHandle);
        //    var pointOnScreen = new Point(100,10);
        //    UserBindings.ClientToScreen(chromeHandle, ref pointOnScreen);
        //    UserInteropAdapter.ClickOnPoint(Selenium.BrowserHandle(), pointOnScreen);

        //    UserInteropAdapter.PressWithControl(chromeHandle, 0x41);
        //    UserInteropAdapter.PressWithControl(chromeHandle, 0x43);

        //    UserInteropAdapter.ClickOnPoint(Selenium.BrowserHandle(), pointOnScreen);

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
