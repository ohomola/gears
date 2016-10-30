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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Gears.Interpreter.Library;
using OpenQA.Selenium;

namespace Gears.Interpreter.Adapters
{
    public interface ISeleniumAdapter: IDisposable
    {
        IWebDriver WebDriver { get; set; }
        IntPtr GetChromeHandle();
        Point PutElementOnScreen(IWebElement element);
        void BrowserToClient(ref Point p);
    }

    public class SeleniumAdapter : ISeleniumAdapter, IDisposable
    {
        private IntPtr _handle;
        public IWebDriver WebDriver { get; set; }

        public SeleniumAdapter(IWebDriver webDriver)
        {
            WebDriver = webDriver;
        }

        public void Dispose()
        {
            try
            {
                WebDriver.Close();
                WebDriver.Quit();
            }
            catch (Exception)
            {
            }
        }

        public IntPtr GetChromeHandle()
        {
            if (_handle == default(IntPtr))
            {
                var processes = Process.GetProcesses().Where(x => x.MainWindowTitle.ToLower().Contains("google chrome"));
                if (processes.Count() > 1)
                {
                    throw new ApplicationException("Please close other Chrome windows.");
                }
                var process = processes.FirstOrDefault();
                if (process == null)
                {
                    throw new ApplicationException("Chrome window was not found");
                }
                _handle = process.MainWindowHandle;
            }

            return _handle;
        }

        public void BrowserToClient(ref Point p)
        {
            var YOffset =
                (int)
                Math.Abs(
                    (long)WebDriver.RunLibraryScript("return window.innerHeight - window.outerHeight"));
            var XOffset =
                (int)
                Math.Abs((long)WebDriver.RunLibraryScript("return window.innerWidth - window.outerWidth"));

            var scrollOffset =
                (int)
                Math.Abs(
                    (long)
                    WebDriver.RunLibraryScript("return window.pageYOffset || document.documentElement.scrollTop"));

            p.X += XOffset;
            p.Y += YOffset - scrollOffset;
        }

        public Point PutElementOnScreen(IWebElement element)
        {
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("scrollBy(0,-200);");

            var rect =
                (Dictionary<string, object>)
                ((IJavaScriptExecutor)WebDriver).ExecuteScript(
                    "return arguments[0].getBoundingClientRect();", element);

            var xx = (rect["left"]);
            var yy = (rect["top"]);

            var location = new Point(Convert.ToInt32(xx), System.Convert.ToInt32(yy));

            var aa = WebDriver.RunLibraryScript("return window.innerHeight - window.outerHeight");
            var bb = WebDriver.RunLibraryScript("return window.innerWidth - window.outerWidth");
            location.Y += (int)Math.Abs((long)aa);
            location.Y += 5;
            location.X += 5;
            return location;
        }
    }
}