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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Castle.Components.DictionaryAdapter.Xml;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public class IsVisible : Keyword
    {
        private Instruction spec;
        public List<string> TagNames { get; set; }

        public string SubjectName { get; set; }

        public string Locale { get; set; }

        public SearchDirection Direction { get; set; }

        public int Order { get; set; }

        public IsVisible(string what)
        {
            spec = new Instruction(what);
            SubjectName = spec.SubjectName;
            Locale = spec.Locale;
            Direction = spec.Direction;
            Order = spec.Order;
            TagNames = spec.TagNames;
        }

        public override object Run()
        {
            var searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);
            var lookupResult = searchStrategy.DirectLookup(TagNames, SubjectName, Locale, Direction, Order, false);


            if (Order == 0)
            {
                var chromeHandle = Selenium.GetChromeHandle();
                var browserBox = new UserBindings.RECT();
                UserBindings.GetWindowRect(chromeHandle, ref browserBox);

                for (int index = 0; index < lookupResult.OtherValidResults.Count; index++)
                {
                    var e = lookupResult.OtherValidResults[index];

                    var centerX = e.Rectangle.X + e.Rectangle.Width/2;
                    var centerY = e.Rectangle.Y + e.Rectangle.Height/2;

                    var p = new Point(centerX, centerY);
                    Selenium.BrowserToClient(ref p);

                    if (p.X < 0 || p.X > browserBox.Right || p.Y < 0 || p.Y> browserBox.Bottom)
                    {
                        continue;
                    }
                    else
                    {
                        Show.HighlightElements(Selenium, lookupResult.OtherValidResults, index);
                        return true;
                    }
                }
                Show.HighlightElements(Selenium, lookupResult.OtherValidResults);
                return false;
            }

            throw new NotImplementedException();
        }

        

        private bool CheckTextOnly(string what)
        {
            var oldClipboard = Clipboard.GetDataObject();

            var chromeHandle = Selenium.GetChromeHandle();
            UserBindings.SetForegroundWindow(chromeHandle);
            var pointOnScreen = new Point(100,10);
            UserBindings.ClientToScreen(chromeHandle, ref pointOnScreen);
            UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), pointOnScreen);

            UserInteropAdapter.PressWithControl(chromeHandle, 0x41);
            UserInteropAdapter.PressWithControl(chromeHandle, 0x43);

            UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), pointOnScreen);

            var text = Clipboard.GetText(TextDataFormat.UnicodeText);
            if (oldClipboard != null)
            {
                Clipboard.SetDataObject(oldClipboard);
            }
            UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());

            if (text.ToLower().Contains(what.ToLower()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return $"Is {spec} visible?";
        }
    }
}
