using System;

namespace Gears.Interpreter.Library.Lookup
{
    public class SeleniumException : ApplicationException
    {
        public SeleniumException(Exception innerException) : base("Selenium WebDriver has encountered error.", innerException)
        {
        }
    }
}