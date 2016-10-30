using System.Drawing;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
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
}