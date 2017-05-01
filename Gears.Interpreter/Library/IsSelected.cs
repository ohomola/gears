﻿#region LICENSE
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
using System.Windows.Forms;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Library.Workflow;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    [UserDescription("isselected <i>\t-\t checks if a checkbox or similar input is selected")]
    public class IsSelected : Keyword, IAssertion
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
        }


        public override IKeyword FromString(string textInstruction)
        {
            return new IsSelected() {What = ExtractSingleParameterFromTextInstruction(textInstruction), Expect = true};
        }

        public override object DoRun()
        {
            var searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);
            var lookupResult = searchStrategy.DirectLookup(TagNames, SubjectName, Locale, Direction, Order, false);

            if (Interpreter?.IsAnalysis == true)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{lookupResult.Result}\nAll results:\n\t{string.Join("\n\t", lookupResult.OtherValidResults)}");
            }

            var chromeHandle = Selenium.GetChromeHandle();
            var browserBox = new UserBindings.RECT();
            UserBindings.GetWindowRect(chromeHandle, ref browserBox);

            var e = lookupResult.OtherValidResults.ElementAt(Order);

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
                    Highlighter.HighlightElements(750, Selenium, lookupResult.OtherValidResults, (Expect.ToString().ToLower().Equals(true.ToString().ToLower()) ? Color.GreenYellow : Color.Red), Color.Yellow, -1, Color.Black);
                }
                return lookupResult.OtherValidResults.ElementAt(Order).WebElement.Selected;
            }

            throw new NotFoundException("Element not found");

            throw new NotImplementedException($"Checking visibility of nth ({Order}) elements is not implemented.");
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