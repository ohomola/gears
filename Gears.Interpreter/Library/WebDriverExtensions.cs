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
using System.Reflection;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public static class WebElementExtensions
    {
        public static IBufferedElement AsBufferedElement(this IWebElement webElement)
        {
            return new BufferedElement(webElement);
        }
    }

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


        //public static void ClickByTagNameAndLocation(this IWebDriver webDriver, TagQuery locationDescription)
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

        public static IWebElement GetByTagNameAndLocation(this IWebDriver webDriver, TagQuery locationDescription)
        {
            var elements = GetAllByTagNameAndLocation(webDriver, locationDescription);
            return elements.ElementAt(locationDescription.OneBasedOrder-1);
        }

        public static ReadOnlyCollection<IWebElement> GetAllByTagNameAndLocation(this IWebDriver webDriver, TagQuery locationDescription)
        {
            try
            {
                //var result = webDriver.RunLibraryScript($"return sortByLocation(" +
                //                           $"{locationDescription.IsFromRight.ToString().ToLower()}, " +
                //                           $"getElementsByTagName(\"{locationDescription.TagName}\"));");

                var result = webDriver.RunLibraryScript($"return sortByLocation(" +
                                           $"{locationDescription.IsFromRight.ToString().ToLower()}, " +
                                           $"getElementsByTagName(arguments[0]));", ((object)locationDescription.TagNames));

                return (ReadOnlyCollection<IWebElement>) result;
            }
            catch (Exception e)
            {
                throw new ApplicationException($"No Element was found by looking for '{locationDescription}'.");
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

        [JavascriptFunctionWrapper]
        public static ReadOnlyCollection<IWebElement> GetAllElements(this IWebDriver webDriver)
        {
            var result = webDriver.RunLibraryScript($"return {MethodBase.GetCurrentMethod().Name}()");

            return ToCollection(result);
        }

        [JavascriptFunctionWrapper]
        public static ReadOnlyCollection<IWebElement> GetElementsByTagNames(this IWebDriver webDriver, IEnumerable<string> tagNames)
        {
            var result = webDriver.RunLibraryScript($"return {MethodBase.GetCurrentMethod().Name}(arguments[0])", tagNames);
            
            return ToCollection(result);
        }

        [JavascriptFunctionWrapper]
        public static ReadOnlyCollection<IWebElement> FilterElementsByText(this IWebDriver webDriver, string text, IEnumerable<IWebElement> elements, bool matchWhenTextIsInChild)
        {
            var result = webDriver.RunLibraryScript($"return {MethodBase.GetCurrentMethod().Name}(\"{text}\",arguments[0],arguments[1])", elements, matchWhenTextIsInChild);

            return ToCollection(result);
        }
        
        [JavascriptFunctionWrapper]
        public static ReadOnlyCollection<IWebElement> FilterOrthogonalElements(this IWebDriver webDriver, ReadOnlyCollection<IWebElement> elements, IWebElement element)
        {
            var result = webDriver.RunLibraryScript($"return {MethodBase.GetCurrentMethod().Name}(arguments[0], arguments[1])", elements, element);

            return ToCollection(result);
        }

        [JavascriptFunctionWrapper]
        public static ReadOnlyCollection<IWebElement> FilterDomNeighbours(this IWebDriver webDriver, ReadOnlyCollection<IWebElement> elements, IWebElement element)
        {
            var result = webDriver.RunLibraryScript($"return {MethodBase.GetCurrentMethod().Name}(arguments[0], arguments[1])", elements, element);

            return ToCollection(result);
        }

        [JavascriptFunctionWrapper]
        public static IEnumerable<IBufferedElement> SelectWithLocation(this IWebDriver webDriver, ReadOnlyCollection<IWebElement> elements)
        {
            var result = webDriver.RunLibraryScript($"return {MethodBase.GetCurrentMethod().Name}(arguments[0])",
                elements);

            var returnValue = new List<IBufferedElement>();
            foreach (var pair in result as ReadOnlyCollection<object>)
            {
                var dictionary = ((ReadOnlyCollection<object>)pair)[1] as Dictionary<string, object>;
                var left = ConvertToIntFromTypeUnsafe(dictionary["left"]);
                var top = ConvertToIntFromTypeUnsafe(dictionary["top"]);
                var height = ConvertToIntFromTypeUnsafe(dictionary["height"]);
                var width = ConvertToIntFromTypeUnsafe(dictionary["width"]);

                var bufferedElement = new BufferedElement
                {
                    WebElement = ((ReadOnlyCollection<object>) pair)[0] as IWebElement,
                    Rectangle = new Rectangle(left, top, width, height)
                };

                returnValue.Add(bufferedElement);
            }

            return returnValue;
        }

        private static ReadOnlyCollection<IWebElement> ToCollection(object result)
        {
            if (result is ReadOnlyCollection<IWebElement>)
            {
                return (ReadOnlyCollection<IWebElement>)result;
            }
            return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
        }

        private static int ConvertToIntFromTypeUnsafe( object @object)
        {
            if (@object == null)
            {
                throw new ArgumentException();
            }
            if (@object is double)
            {
                return (int)Math.Round((double)@object);
            }
            else if (@object is int)
            {
                return (int)@object;
            }
            else if (@object is long)
            {
                var val = (long) @object;
                return unchecked((int)val);
            }
            throw new InvalidCastException($"Cannot convert {@object} of type {@object.GetType()} to int");
        }
    }

    public interface IBufferedElement
    {
        IWebElement WebElement { get; set; }
        Rectangle Rectangle { get; set; }
    }

    public class BufferedElement : IBufferedElement
    {
        public BufferedElement(IWebElement webElement)
        {
            WebElement = webElement;
            Rectangle = new Rectangle(webElement.Location.X, webElement.Location.Y, webElement.Size.Width, webElement.Size.Height);
        }

        public BufferedElement()
        {
        }

        public IWebElement WebElement { get; set; }

        public Rectangle Rectangle { get; set; }
    }

    /// <summary>
    /// Marker attribute to identify methods wrapping javascript library functions.
    /// It does not ave any other function except convenience
    /// </summary>
    public class JavascriptFunctionWrapperAttribute : Attribute
    {
    }
}
