#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears.

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
using System.Linq;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public class Fill : Keyword
    {
        private readonly string _label;
        private readonly string _text;

        public Fill(string label, string text)
        {
            _label = label;
            _text = text;
        }

        public override object Run()
        {
            try
            {
                var scriptFile = FileFinder.Find("Gears.Library.js");
                var script = File.ReadAllText(scriptFile);
                script += $"tagMatches(getMatches(\"{_label}\"));";

                ((IJavaScriptExecutor)Selenium).ExecuteScript(script);

                var elems = Selenium.WebDriver.FindElements(
                    By.CssSelector("[Exact_Match_by_Text],[Exact_Match_by_Attribute]"));

                elems.First().SendKeys(_text);
            }
            catch (Exception)
            {
                throw new ApplicationException($"Element {_label} was not found");
            }

            return null;
        }

        public override string ToString()
        {
            return $"Fill {_label} with {_text}";
        }
    }
}
