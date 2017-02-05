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
        public override string ToString()
        {
            return $"<{WebElement.TagName}> {(string.IsNullOrEmpty(WebElement.Text)?"": $" with text {WebElement.Text}")} Position {WebElement.Location} Size {WebElement.Size}";
        }

        protected bool Equals(BufferedElement other)
        {
            return Rectangle.Equals(other.Rectangle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BufferedElement) obj);
        }

        public override int GetHashCode()
        {
            return Rectangle.GetHashCode();
        }
    }
}