using System.Drawing;

namespace Gears.Interpreter.App.UI.Overlay
{
    public class Artifact
    {
        public string Text { get; set; }
        public Rectangle Rectangle { get; set; }
        public Color BackgroundColor { get; }
        public Color LineColor { get; set; }

        public Artifact(string text, Rectangle rectangle, Color backgroundColor, Color lineColor)
        {
            Text = text;
            Rectangle = rectangle;
            BackgroundColor = backgroundColor;
            LineColor = lineColor;
        }

        public void RenderTo(Graphics painter)
        {
            painter.FillRectangle(new SolidBrush(LineColor), Rectangle);
            painter.DrawString(Text, new Font(FontFamily.GenericSansSerif, 10),
                new SolidBrush(Color.FromArgb(1, 1, 1)), Rectangle.X, Rectangle.Y);
        }
    }
}