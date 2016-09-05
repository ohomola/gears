using System.IO;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public static class WebDriverExtensions
    {
        public static object RunLibraryScript(this IWebDriver webDriver, string scriptCode)
        {
            var scriptFile = FileFinder.Find("Gears.Library.js");
            var script = File.ReadAllText(scriptFile);
            script += scriptCode;
            return ((IJavaScriptExecutor)webDriver).ExecuteScript(script);
        }

    }
}
