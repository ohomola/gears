using System;
using System.IO;
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
    }
}
