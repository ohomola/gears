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
using System.Runtime.InteropServices;
using System.Threading;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Gears.Interpreter.Library
{
    public class Fill : Keyword
    {
        public string What { get; set; }
        public string Where { get; set; }
        public string Text { get; set; }
        public bool Javascript { get; set; }

        public Fill(string what, string text)
        {
            What = what;
            Text = text;
        }

        public override object Run()
        {
            if(Javascript)
            {
                try
                {
                    var results = Selenium.WebDriver.RunLibraryScript($"return findInput(\"{What}\",\"{Where}\")");
                    var element = (results as IWebElement);
                    if (element == null)
                    {
                        throw new ApplicationException("Element was not found");
                    }
                    element.SendKeys(Text);
                    return element;
                }
                catch (Exception)
                {
                    throw new ApplicationException($"Element {What} was not found");
                }
            }
            else{
                var results = Selenium.WebDriver.RunLibraryScript($"return findInput(\"{What}\",\"{Where}\")");
                var element = (results as IWebElement);
                if (element == null)
                {
                    throw new ApplicationException("Element was not found");
                }

                ((IJavaScriptExecutor) Selenium.WebDriver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                ((IJavaScriptExecutor) Selenium.WebDriver).ExecuteScript("scrollBy(0,-200);");

                var rect =
                    (Dictionary<string, object>)
                    ((IJavaScriptExecutor) Selenium.WebDriver).ExecuteScript(
                        "return arguments[0].getBoundingClientRect();", element);

                var xx = (rect["left"]);
                var yy = (rect["top"]);
                var location = new Point(Convert.ToInt32(xx), System.Convert.ToInt32(yy));
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
                var handle = process.MainWindowHandle;

                var aa = Selenium.WebDriver.RunLibraryScript("return window.innerHeight - window.outerHeight");
                location.Y += (int) Math.Abs((long) aa);
                location.Y += 5;
                location.X += 5;

                UserControl.ClickOnPoint(handle, location);
                Thread.Sleep(50);
                UserControl.SendText(handle, Text, location);
                Thread.Sleep(50);
                UserControl.SetForegroundWindow(UserControl.GetConsoleWindow());

                return element;
            }
        }



        public override string ToString()
        {
            return $"Fill {Where} '{What}'  with '{Text}'";
        }
    }
}
