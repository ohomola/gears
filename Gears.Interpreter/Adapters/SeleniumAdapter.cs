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
using System.IO;
using System.Linq;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Gears.Interpreter.Adapters
{
    public interface ISeleniumAdapter: IDisposable
    {
        IWebDriver WebDriver { get; }
        IntPtr GetChromeHandle();
        Point PutElementOnScreen(IWebElement element);
        void BrowserToClient(ref Point p);
    }

    public class SeleniumAdapter : ISeleniumAdapter, IDisposable
    {
        private IntPtr _handle;
        private IWebDriver _webDriver;

        public IWebDriver WebDriver
        {
            get
            {
                if (_webDriver == null)
                {
                    var path = FileFinder.Find("chromedriver.exe");
                    _webDriver = new ChromeDriver(Path.GetDirectoryName(path), new ChromeOptions());
                }
                return _webDriver;
            }
        }


        public void Dispose()
        {
            try
            {
                if (_webDriver != null)
                {
                    _webDriver.Close();
                    _webDriver.Quit();
                }
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
            //((IJavaScriptExecutor)WebDriver).ExecuteScript("scrollBy(0,-200);");

            var location = GetLocation(element);

            var browserBox = new UserBindings.RECT();
            UserBindings.GetWindowRect(GetChromeHandle(), ref browserBox);

            ScrollToCenterOfScreen(location, browserBox);

            location = GetLocation(element);

            var browserBarHeight = WebDriver.RunLibraryScript("return window.innerHeight - window.outerHeight");
            var bb = WebDriver.RunLibraryScript("return window.innerWidth - window.outerWidth");
            location.Y += (int)Math.Abs((long)browserBarHeight);
            location.Y += 5;
            location.X += 5;
            return location;
        }

        private Point GetLocation(IWebElement element)
        {
            var rect =
                (Dictionary<string, object>)
                ((IJavaScriptExecutor) WebDriver).ExecuteScript(
                    "return arguments[0].getBoundingClientRect();", element);

            var xx = (rect["left"]);
            var yy = (rect["top"]);

            var location = new Point(Convert.ToInt32(xx), System.Convert.ToInt32(yy));
            return location;
        }

        private void ScrollToCenterOfScreen(Point location, UserBindings.RECT browserBox)
        {
            var diff = location.Y - browserBox.Top;
            if (diff < 200)
            {
                var scrollBy = 200-diff;
                ((IJavaScriptExecutor)WebDriver).ExecuteScript($"scrollBy(0,-{scrollBy});");
                location.Y += scrollBy;
            }

            diff = browserBox.Bottom - location.Y;
            if (diff < 200)
            {
                var scrollBy = -200+diff;
                ((IJavaScriptExecutor)WebDriver).ExecuteScript($"scrollBy(0,{scrollBy});");
                location.Y += scrollBy;
            }

        }
    }
}