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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace Gears.Interpreter.Library
{
    public class Show : Keyword
    {
        public string Where { get; set; }

       
        public Show(string @where)
        {
            Where = @where;
        }

        public override object Run()
        {
            var elements = Selenium.WebDriver.GetAllByTagNameAndLocation(new TagQuery(Where));
            if (elements == null)
            {
                return null;
            }

            HighlightElements(elements, Selenium);

            return null;
        }

        public static void HighlightElements(IEnumerable<IWebElement> elements, ISeleniumAdapter seleniumAdapter)
        {
            var YOffset =
                (int)
                Math.Abs(
                    (long) seleniumAdapter.WebDriver.RunLibraryScript("return window.innerHeight - window.outerHeight"));
            var XOffset =
                (int)
                Math.Abs((long) seleniumAdapter.WebDriver.RunLibraryScript("return window.innerWidth - window.outerWidth"));

            var scrollOffset =
                (int)
                Math.Abs(
                    (long)
                    seleniumAdapter.WebDriver.RunLibraryScript(
                        "return window.pageYOffset || document.documentElement.scrollTop"));

            using (var overlay = new Overlay())
            {
                var handle = seleniumAdapter.GetChromeHandle();
                overlay.Init();
                int i = 0;
                foreach (var element in elements)
                {
                    i++;
                    overlay.DrawStuff(handle, i, element.Location.X + XOffset, element.Location.Y + YOffset - scrollOffset,
                        overlay.Graphics);
                }

                Console.Out.WriteColoredLine(ConsoleColor.White,
                    $"{elements.Count()} elements highlighted on screen. Press enter to continue (highlighting will disappear).");
                Console.ReadLine();
            }
        }


        public override string ToString()
        {
            return $"Show {Where}";
        }
    }
}
