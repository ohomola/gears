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
using System.Diagnostics;
using System.Linq;
using OpenQA.Selenium;

namespace Gears.Interpreter.Adapters
{
    public interface ISeleniumAdapter: IDisposable
    {
        IWebDriver WebDriver { get; set; }
        IntPtr GetChromeHandle();
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
    }
}