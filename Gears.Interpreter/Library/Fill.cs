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
using System.Xml.Serialization;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Gears.Interpreter.Library
{
    public class Clear : Keyword
    {
        public string What { get; set; }
        public string Where { get; set; }
        public string Text { get; set; }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

        public Clear(string what)
        {
            What = what;
        }

        public override object Run()
        {
            try
            {
                var element = Selenium.WebDriver.FindInput(What, Where);
                element.SendKeys(Keys.LeftControl + "a");
                element.SendKeys(Keys.Delete);
                return element;
            }
            catch (Exception)
            {
                throw new ApplicationException($"Element {What} was not found");
            }
        }



        public override string ToString()
        {
            return $"Fill {Where} '{What}'  with '{Text}'";
        }
    }
    public class Fill : Keyword
    {
        public string What { get; set; }
        public string Where { get; set; }
        public string Text { get; set; }
        public bool Javascript { get; set; }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

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
                    var element = Selenium.WebDriver.FindInput(What, Where);
                    element.SendKeys(Text);
                    return element;
                }
                catch (Exception)
                {
                    throw new ApplicationException($"Element {What} was not found");
                }
            }
            else{
                var element = Selenium.WebDriver.FindInput( What, Where);

                var handle = Selenium.GetChromeHandle();

                var screenLocation = Selenium.PutElementOnScreen(element);

                UserInteropAdapter.ClickOnPoint(handle, screenLocation);
                Thread.Sleep(50);
                UserInteropAdapter.SendText(handle, Text, screenLocation);
                Thread.Sleep(50);
                UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());

                return element;
            }
        }

        

        public override string ToString()
        {
            return $"Fill {Where} '{What}'  with '{Text}'";
        }
    }
}
