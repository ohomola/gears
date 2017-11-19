using System.Collections.Generic;

namespace Gears.Interpreter.Core.Adapters.UI
{
    public enum SeleniumAdapterBrowserType
    {
        Chrome = 0,
        InternetExplorer = 2,
        Firefox = 1,
    }

    public static class SeleniumAdapterBrowserTypeMapper
    {
        public static Dictionary<SeleniumAdapterBrowserType, string> MainWindowTitleFor = new Dictionary<SeleniumAdapterBrowserType, string>()
        {
            { SeleniumAdapterBrowserType.Chrome, "google chrome" },
            { SeleniumAdapterBrowserType.Firefox, "firefox" },
            { SeleniumAdapterBrowserType.InternetExplorer, "Internet Explorer" },
        };
    }
}