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
using System.IO;
using System.Linq;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public static class WebDriverExtensions
    {
        public static object RunLibraryScript(this IWebDriver webDriver, string scriptCode)
        {
            var script = File.ReadAllText(FileFinder.Find("Gears.Library.js"));
            
            script += scriptCode;
            return ((IJavaScriptExecutor)webDriver).ExecuteScript(script);
        }

        public static void ClickByVisibleText(this IWebDriver webDriver, string what, string @where)
        {
            try
            {
                webDriver.RunLibraryScript($"clickFirstMatch([firstByLocation(\"{where}\", getExactMatches(\"{what}\"))]);");
            }
            catch (Exception)
            {
                throw new ApplicationException($"Element '{what}' was not found");
            }
        }

        public static void ClickByTagNameAndLocation(this IWebDriver webDriver, ButtonQuery locationDescription)
        {
            try
            {
                webDriver.RunLibraryScript($"clickNthMatch(sortByLocation(" +
                                           $"{locationDescription.IsFromRight.ToString().ToLower()}, " +
                                           $"getElementsByTagName(\"{locationDescription.TagName}\"))," +
                                           $"{locationDescription.Order}" +
                                           $");");
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Element '{locationDescription}' was not found");
            }
        }

        public static object GetByTagNameAndLocation(this IWebDriver webDriver, ButtonQuery locationDescription)
        {
            try
            {
                var result = webDriver.RunLibraryScript($"return sortByLocation(" +
                                           $"{locationDescription.IsFromRight.ToString().ToLower()}, " +
                                           $"getElementsByTagName(\"{locationDescription.TagName}\"));");

                return result;
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Element '{locationDescription}' was not found");
            }
        }

        
    }
}
