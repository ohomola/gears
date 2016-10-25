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
using System.IO;
using System.Threading;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace Gears.Interpreter.Library
{
    public class Click : Keyword
    {
        public string What { get; set; }
        public string Where { get; set; }

        public bool Javascript { get; set; }

        public Click(string what)
        {
            What = what;
            Javascript = false;
        }

        public override object Run()
        {
            try
            {
                var elem = Selenium.WebDriver.GetElementByVisibleText(What, Where);

                if (elem == null)
                {
                    elem = Selenium.WebDriver.GetByTagNameAndLocation(new TagQuery(What));
                }

                if (Javascript)
                {
                    Selenium.WebDriver.Click(elem);
                }
                else
                {
                    var screenLocation = Selenium.PutElementOnScreen(elem);
                    UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), screenLocation);
                    Thread.Sleep(50);
                    UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());
                }
            }
            catch (Exception)
            {
                throw new ApplicationException($"Element was not found");
            }
            return null;
        }

        public override string ToString()
        {
            return $"Click {Where} '{What}'";
        }
    }

    //public class Click : Keyword
    //{
    //    public string What { get; set; }
    //    public string Where { get; set; }

    //    public bool Javascript { get; set; }

    //    public Click(string what)
    //    {
    //        What = what;
    //        Javascript = false;
    //    }

    //    public override object Run()
    //    {
    //        try
    //        {
    //            var elem = Selenium.WebDriver.GetElementByVisibleText(What, Where);

    //            if (elem == null)
    //            {
    //                elem = Selenium.WebDriver.GetByTagNameAndLocation(new TagQuery(What));
    //            }

    //            if (Javascript)
    //            {
    //                Selenium.WebDriver.Click(elem);
    //            }
    //            else
    //            {
    //                var screenLocation = Selenium.PutElementOnScreen(elem);
    //                UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), screenLocation);
    //                Thread.Sleep(50);
    //                UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());
    //            }
    //        }
    //        catch (Exception)
    //        {
    //            throw new ApplicationException($"Element was not found");
    //        }
    //        return null;
    //    }

    //    public override string ToString()
    //    {
    //        return $"Click {Where} '{What}'";
    //    }
    //}
}
