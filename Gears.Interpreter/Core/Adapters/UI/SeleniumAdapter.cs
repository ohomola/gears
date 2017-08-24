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
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.Adapters.UI.JavaScripts;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Data.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Gears.Interpreter.Core.Adapters.UI
{
    public interface ISeleniumAdapter: IDisposable
    {
        IWebDriver WebDriver { get; }
        UserBindings.RECT BrowserWindowScreenRectangle { get; }
        IFluentElementQuery Query { get; }
        IntPtr GetChromeHandle();
        UserBindings.RECT GetChromeBox();

        Point PutElementOnScreen(IWebElement element);
        void ConvertFromPageToWindow(ref Point p);
        void ConvertFromWindowToScreen(ref Point point);
        void ConvertFromScreenToGraphics(ref Point point);


        void ConvertFromWindowToPage(ref Point p);
        void ConvertFromGraphicsToScreen(ref Point point);
        void ConvertFromScreenToWindow(ref Point point);
        int ContentOffsetX();
        int ContentOffsetY();
        void BringToFront();
        bool TerminateProcess(string name);
    }

    public class SeleniumAdapter : ISeleniumAdapter, IDisposable
    {
        private IntPtr _handle;
        private IWebDriver _webDriver;

        protected Lazy<int> LazyContentOffsetX { get; set; }
        protected Lazy<int> LazyContentOffsetY { get; set; }

        public SeleniumAdapter()
        {
            LazyContentOffsetX = new Lazy<int>(() => (int)
                Math.Abs((long)WebDriver.RunLibraryScript("return window.innerWidth - window.outerWidth")) - 15);

            LazyContentOffsetY =
                new Lazy<int>(
                    () =>
                        (int)
                        Math.Abs((long) WebDriver.RunLibraryScript("return window.innerHeight - window.outerHeight")) -
                        7);
        }

        public IWebDriver WebDriver
        {
            get
            {
                LazyInitialize();
                return _webDriver;
            }
        }

        public UserBindings.RECT BrowserWindowScreenRectangle {
            get
            {
                var handle = this.GetChromeHandle();
                var rect = new UserBindings.RECT();
                UserBindings.GetWindowRect(handle, ref rect);
                return rect;
            }
        }

        private void LazyInitialize()
        {
            if (_webDriver == null)
            {
                var path = FileFinder.Find("chromedriver.exe");
                var existingChromes = AllChromeHandles();
                
                var options = new ChromeOptions();
                options.AddArguments("enable-automation");
                options.AddArguments("--disable-infobars");
                _webDriver = new ChromeDriver(Path.GetDirectoryName(path), options);
                _handle = FindChromeHandle(existingChromes);
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

            TerminateProcess("chromedriver");
        }

        public bool TerminateProcess(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (!clsProcess.ProcessName.StartsWith(name))
                {
                    continue;
                }

                try
                {
                    Console.Out.Write($"Terminating {name}");
                    clsProcess.CloseMainWindow();
                    clsProcess.WaitForExit();
                }
                catch
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public IFluentElementQuery Query => new FluentElementQuery(this);

        public IntPtr GetChromeHandle()
        {
            LazyInitialize();
            
            return _handle;
        }

        public UserBindings.RECT GetChromeBox()
        {
            var browserBox = new UserBindings.RECT();
            UserBindings.GetWindowRect(GetChromeHandle(), ref browserBox);
            return browserBox;
        }

        private IntPtr FindChromeHandle(IEnumerable<Process> oldChromes)
        {
            var allChromeHandles = AllChromeHandles();

            var newChromees = allChromeHandles.Where(each=>!oldChromes.Select(x=>x.Id).Contains(each.Id));
            var process = newChromees.Single();
            if (process == null)
            {
                throw new ApplicationException("Chrome window was not found");
            }

            return process.MainWindowHandle;
        }

        private static IEnumerable<Process> AllChromeHandles()
        {
            return Process.GetProcesses().Where(x => x.MainWindowTitle.ToLower().Contains("google chrome")).ToList();
        }


        public void ConvertFromPageToWindow(ref Point p)
        {
            int scrollOffset;
            try
            {
                var scriptResult = WebDriver.RunLibraryScript("return window.pageYOffset || document.documentElement.scrollTop");
                scrollOffset = (int)
                        Math.Abs(
                            (long)
                            scriptResult);
            }
            catch (Exception)
            {
                scrollOffset = 0;
            }

            p.X += ContentOffsetX();
            p.Y += ContentOffsetY() - scrollOffset;
        }

        public void ConvertFromWindowToScreen(ref Point point)
        {
            UserBindings.ClientToScreen(this.GetChromeHandle(), ref point);
        }

        public void ConvertFromScreenToGraphics(ref Point point)
        {
            UserInteropAdapter.ScreenToGraphics(ref point);
        }

        public void ConvertFromGraphicsToScreen(ref Point point)
        {
            UserInteropAdapter.GraphicsToScreen(ref point);
        }

        public void ConvertFromScreenToWindow(ref Point point)
        {
            UserBindings.ScreenToClient(this.GetChromeHandle(), ref point);
        }

        public void ConvertFromWindowToPage(ref Point p)
        {
            var scrollOffset =
                (int)
                Math.Abs(
                    (long)
                    WebDriver.RunLibraryScript("return window.pageYOffset || document.documentElement.scrollTop"));

            p.X -= ContentOffsetX();
            p.Y -= ContentOffsetY() - scrollOffset;
        }

        
        public Point PutElementOnScreen(IWebElement element)
        {
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            //((IJavaScriptExecutor)WebDriver).ExecuteScript("scrollBy(0,-200);");

            var location = GetLocation(element);

            var size = element.Size;

            var browserBox = new UserBindings.RECT();
            UserBindings.GetWindowRect(GetChromeHandle(), ref browserBox);

            ScrollToCenterOfScreen(location, browserBox);

            location = GetLocation(element);

            var browserBarHeight = ContentOffsetY();
            var bb = ContentOffsetX();
            location.Y += (int)Math.Abs(browserBarHeight);
            location.Y += Math.Min(size.Height/2,8);
            location.X += Math.Min(size.Width/2, 8);
            return location;
        }

        public int ContentOffsetX()
        {
            return LazyContentOffsetX.Value;
        }

        public int ContentOffsetY()
        {
            return LazyContentOffsetY.Value;
        }

        public void BringToFront()
        {
            UserBindings.SetForegroundWindow(_handle);
            ((IJavaScriptExecutor)WebDriver).ExecuteScript("window.focus();");
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