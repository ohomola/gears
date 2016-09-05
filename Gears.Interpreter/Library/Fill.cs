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
using System.IO;
using System.Linq;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Gears.Interpreter.Library
{
    public class Fill : Keyword
    {
        public string Label { get; set; }
        public string Text { get; set; }

        public Fill(string label, string text)
        {
            Label = label;
            Text = text;
        }

        public override object Run()
        {
            try
            {
                var siblingMatches= Selenium.WebDriver.RunLibraryScript($"return tagMatches(getSiblingExactMatches(\"{Label}\"));");
                
                var elements = (siblingMatches as IEnumerable<IWebElement>);

                elements.First(x=>x.Displayed && x.Enabled && x.TagName == "input").SendKeys(Text);
            }
            catch (Exception)
            {
                throw new ApplicationException($"Element {Label} was not found");
            }

            return null;
        }

        
        public override string ToString()
        {
            return $"Fill {Label} with {Text}";
        }
    }
}
