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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public static class WebDriverExtensions
    {
        public static object RunLibraryScript(this IWebDriver webDriver, string scriptCode, params object[] elements)
        {
            var script = File.ReadAllText(FileFinder.Find("Gears.Library.js"));
            
            script += scriptCode;
            return ((IJavaScriptExecutor)webDriver).ExecuteScript(script, elements);
        }

        //public static void ClickByVisibleText(this IWebDriver webDriver, string what, string @where)
        //{
        //    try
        //    {
        //        webDriver.RunLibraryScript($"clickFirstMatch([firstByLocation(\"{where}\", getExactMatches(\"{what}\"))]);");
        //    }
        //    catch (Exception)
        //    {
        //        throw new ApplicationException($"Element '{what}' was not found");
        //    }
        //}

        public static IWebElement GetElementByVisibleText(this IWebDriver webDriver, string what, string @where)
        {
            try
            {
                return (IWebElement) webDriver.RunLibraryScript($"return firstByLocation(\"{@where}\", getExactMatches(\"{what}\"));");
            }
            catch (Exception)
            {
                throw new ApplicationException($"Element '{what}' was not found");
            }
        }


        //public static void ClickByTagNameAndLocation(this IWebDriver webDriver, ButtonQuery locationDescription)
        //{
        //    try
        //    {
        //        webDriver.RunLibraryScript($"clickNthMatch(sortByLocation(" +
        //                                   $"{locationDescription.IsFromRight.ToString().ToLower()}, " +
        //                                   $"getElementsByTagName(\"{locationDescription.TagName}\"))," +
        //                                   $"{locationDescription.OneBasedOrder}" +
        //                                   $");");
        //    }
        //    catch (Exception e)
        //    {
        //        throw new ApplicationException($"Element '{locationDescription}' was not found");
        //    }
        //}

        public static void Click(this IWebDriver webDriver, IWebElement element)
        {
            webDriver.RunLibraryScript($"click(arguments[0]);", element);
        }

        public static IWebElement GetByTagNameAndLocation(this IWebDriver webDriver, ButtonQuery locationDescription)
        {
            var elements = GetAllByTagNameAndLocation(webDriver, locationDescription);
            return elements.ElementAt(locationDescription.OneBasedOrder-1);
        }

        public static ReadOnlyCollection<IWebElement> GetAllByTagNameAndLocation(this IWebDriver webDriver, ButtonQuery locationDescription)
        {
            try
            {
                var result = webDriver.RunLibraryScript($"return sortByLocation(" +
                                           $"{locationDescription.IsFromRight.ToString().ToLower()}, " +
                                           $"getElementsByTagName(\"{locationDescription.TagName}\"));");

                return (ReadOnlyCollection<IWebElement>) result;
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Element '{locationDescription}' was not found");
            }
        }   


        public static IWebElement FindInput(this IWebDriver webDriver, string what, string whereLocator)
        {
            var results = webDriver.RunLibraryScript($"return findInput(\"{what}\",\"{whereLocator}\")");
            var element = (results as IWebElement);
            if (element == null)
            {
                throw new ApplicationException("Element was not found");
            }
            return element;
        }


    }
}
