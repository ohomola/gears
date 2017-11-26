using System;

namespace Gears.Interpreter.Core.Adapters.UI.Lookup
{
    public class SeleniumException : ApplicationException
    {
        public SeleniumException(Exception innerException) : base("Selenium WebDriver has encountered error: \n"+ innerException.Message, innerException)
        {
        }
    }
}