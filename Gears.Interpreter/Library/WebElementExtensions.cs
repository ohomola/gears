using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public static class WebElementExtensions
    {
        public static IBufferedElement AsBufferedElement(this IWebElement webElement)
        {
            return new BufferedElement(webElement);
        }

        public static bool IsClickable(this IWebElement arg)
        {
            return arg.TagName.ToLower() == "button" || arg.GetAttribute("type") == "button" ||
                   arg.TagName.ToLower() == "a";
        }
    }
}