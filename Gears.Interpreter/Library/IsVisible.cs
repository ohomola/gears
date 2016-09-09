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
using System.Collections;
using System.IO;
using System.Linq;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public class IsVisible : Keyword
    {
        public string What { get; set; }

        public IsVisible(string what)
        {
            What = what;
        }

        public override object Run()
        {
            var result = (Selenium.WebDriver).RunLibraryScript($"return tagMatches(getExactMatches(\"{What}\"));");

            if (result != null)
            {
                var elements = (IEnumerable) result;

                return elements.Cast<object>().Any();
            }
            return false;
        }

        public override string ToString()
        {
            return $"Is text '{What}' visible?";
        }
    }
}
