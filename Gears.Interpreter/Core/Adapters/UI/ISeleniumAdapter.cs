using System;
using System.Drawing;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using OpenQA.Selenium;

namespace Gears.Interpreter.Core.Adapters.UI
{
    public interface ISeleniumAdapter: IDisposable
    {
        IWebDriver WebDriver { get; }
        UserBindings.RECT BrowserWindowScreenRectangle { get; }
        IFluentElementQuery Query { get; }
        IntPtr BrowserHandle { get; }
        UserBindings.RECT GetBrowserBox();

        Point PutElementOnScreen(IWebElement element);
        void ConvertFromPageToWindow(ref Point p);
        void ConvertFromWindowToScreen(ref Point point);
        void ConvertFromScreenToGraphics(ref Point point);


        void ConvertFromWindowToPage(ref Point p);
        void ConvertFromGraphicsToScreen(ref Point point);
        void ConvertFromScreenToWindow(ref Point point);
        int ContentOffsetX();
        int ContentOffsetY();
        void BringToFront();
        bool TerminateProcess(string name);
        void SetBrowserType(SeleniumAdapterBrowserType type);
    }
}