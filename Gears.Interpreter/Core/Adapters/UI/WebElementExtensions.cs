using OpenQA.Selenium;

namespace Gears.Interpreter.Core.Adapters.UI
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
                   arg.TagName.ToLower() == "a" || arg.GetCssValue("cursor") == "pointer";
        }

        public static bool IsFillable(this IWebElement arg)
        {
            return arg.TagName.ToLower() == "input" ||
                   arg.TagName.ToLower() == "textArea" || arg.GetCssValue("cursor") == "input";
        }
    }
}