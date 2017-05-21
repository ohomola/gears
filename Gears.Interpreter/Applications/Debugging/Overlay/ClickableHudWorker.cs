using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications.Debugging.Overlay
{
    internal class ClickableHudWorker
    {
        private readonly Func<ClickableHudForm> _hudFormFactory;
        private ClickableHudForm _form;
        private Bitmap _backBuffer;
        public BackgroundWorker BackgroundWorker { get; set; }

        public ClickableHudWorker(Func<ClickableHudForm> hudFormFactory)
        {
            _hudFormFactory = hudFormFactory;
        }

        public void RunFor(int timeout)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += RunWindow;
            bw.RunWorkerAsync(_hudFormFactory);

            var renderer = new BackgroundWorker();
            renderer.DoWork += RenderArtifacts;
            
            while (_form == null)
            {
                Thread.Sleep(50);
            }

            var graphics = _form.CreateGraphics();

            graphics.CompositingMode = CompositingMode.SourceOver;

            _backBuffer = new Bitmap(_form.Width, _form.Height);
            var backBufferPainter = Graphics.FromImage(_backBuffer);

            renderer.RunWorkerAsync(new RenderereParams() { Graphics = backBufferPainter, timeout = timeout });

            try
            {
                DateTime timeoutTime = DateTime.Now.AddMilliseconds(timeout);
                while (DateTime.Now < timeoutTime)
                {
                    foreach (var artifact in Artifacts)
                    {
                        artifact.RenderTo(backBufferPainter);
                    }

                    graphics.DrawImage(_backBuffer, 0, 0);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Yellow, $"Error rendering overlay: {e.ToString()}");
            }
        }

        class RenderereParams
        {
            public Graphics Graphics { get; set; }            
            public int timeout { get; set; }
        }

        private void RenderArtifacts(object sender, DoWorkEventArgs e)
        {
            var renderereParams = e.Argument as RenderereParams;
            var graphics = renderereParams.Graphics;
            DateTime timeoutTime = DateTime.Now.AddMilliseconds(renderereParams.timeout);
            while (DateTime.Now < timeoutTime)
            {
                foreach (var artifact in Artifacts)
                {
                    artifact.RenderTo(graphics);
                }

                Thread.Sleep(20);

                graphics.DrawImage(_backBuffer, 0, 0);
            }
        }

        public Point RunUntilClicked(ISeleniumAdapter selenium)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += RunWindowToReadMousePosition;
            bw.RunWorkerAsync(_hudFormFactory);

            while (_form == null)
            {
                Thread.Sleep(50);
            }

            var graphics = _form.CreateGraphics();

            graphics.CompositingMode = CompositingMode.SourceOver;

            _backBuffer = new Bitmap(_form.Width, _form.Height);
            var backBufferPainter = Graphics.FromImage(_backBuffer);

            try
            {
                while (_form.ClickPosition == default(Point))
                {
                    Render(backBufferPainter, selenium);

                    graphics.DrawImage(_backBuffer, 0, 0);
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Yellow, $"Error rendering overlay: {e.ToString()}");
            }

            return _form.ClickPosition;
        }

        private void Render(Graphics backBuffer, ISeleniumAdapter selenium)
        {
            var windowRect = selenium.BrowserWindowScreenRectangle;
            var topLeft = new Point(windowRect.Left, windowRect.Top);
            var bottomRight = new Point(windowRect.Right, windowRect.Bottom);

            UserInteropAdapter.ScreenToGraphics(ref topLeft);
            UserInteropAdapter.ScreenToGraphics(ref bottomRight);

            backBuffer.Clear(Color.Black);

            float height = Math.Abs(bottomRight.Y - topLeft.Y);
            float width = Math.Abs(bottomRight.X - topLeft.X);
            backBuffer.FillRectangle(new SolidBrush(Color.FromArgb(255, 100, 255, 255)), topLeft.X+8, topLeft.Y+22, width-16, height-30);
                
            var contentTopLeft = new Point(windowRect.Left, windowRect.Top);
            contentTopLeft.X += selenium.ContentOffsetX();
            contentTopLeft.Y += selenium.ContentOffsetY();
            backBuffer.FillRectangle(new SolidBrush(Color.FromArgb(255, 100, 255, 150)), contentTopLeft.X+8, contentTopLeft.Y, width- selenium.ContentOffsetX()-16, height- selenium.ContentOffsetY()-18);

            backBuffer.DrawString("Click an element", new Font(FontFamily.GenericSansSerif, 14), new SolidBrush(Color.FromArgb(255, 10, 10, 10)), topLeft.X+340, topLeft.Y+22);

            if (ElementLocation != default(Point))
            {
                backBuffer.DrawRectangle(new Pen(Color.FromArgb(255, 0, 50, 150), 2),
                    ElementLocation.X, ElementLocation.Y, ElementSize.Width, ElementSize.Height);
                backBuffer.FillRectangle(new SolidBrush(Color.FromArgb(255, 100, 255, 255)), 
                    ElementLocation.X, ElementLocation.Y, ElementSize.Width, ElementSize.Height);
            }
        }

        private void RunWindowToReadMousePosition(object sender, DoWorkEventArgs e)
        {
            try
            {
                var factory = (e.Argument as Func<ClickableHudForm>);

                _form = factory.Invoke();
                Console.Out.WriteColored(ConsoleColor.White, $"Click element with Mouse:\n");
                var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

                _form.MouseMove+= delegate(object o, MouseEventArgs args)
                {
                    this.HoverX = ((MouseEventArgs) args).X;
                    this.HoverY = ((MouseEventArgs)args).Y;

                    //
                    var windowRect = selenium.BrowserWindowScreenRectangle;
                    var topLeft = new Point(windowRect.Left, windowRect.Top);
                    var bottomRight = new Point(windowRect.Right, windowRect.Bottom);

                    UserInteropAdapter.ScreenToGraphics(ref topLeft);
                    UserInteropAdapter.ScreenToGraphics(ref bottomRight);
                    float height = Math.Abs(bottomRight.Y - topLeft.Y);
                    float width = Math.Abs(bottomRight.X - topLeft.X);
                    var contentTopLeft = new Point(windowRect.Left, windowRect.Top);
                    contentTopLeft.X += selenium.ContentOffsetX();
                    contentTopLeft.Y += selenium.ContentOffsetY();
                    //

                    var point = new Point(this.HoverX, this.HoverY);
                    selenium.ConvertFromScreenToWindow(ref point);
                    selenium.ConvertFromWindowToPage(ref point);

                    var elements = selenium.WebDriver.GetElementByCoordinates(point.X, point.Y);

                    var element = elements.LastOrDefault(x => x.IsClickable());
                    
                    Console.Out.Wipe();
                    Console.Out.BeginRewritableLine();
                    Console.Out.WriteColored(ConsoleColor.Yellow, $"X {HoverX:D4} Y {HoverY:D4} ");
                    Console.Out.WriteColored(ConsoleColor.Green, $"{(element == null ? "                        " : $" -- Button {element.Text} --")}");

                    
                    //Console.Out.WriteColored(ConsoleColor.Magenta, $"Top Left X {topLeft.X:D4} Y {topLeft.Y:D4} ");
                    //Console.Out.WriteColored(ConsoleColor.Magenta, $"Bottom Right X {bottomRight.X:D4} Y {bottomRight.Y:D4} ");
                    Console.Out.WriteColored(ConsoleColor.Magenta, $"width {width} height {height} ");


                    if (element != null)
                    {
                        var p = new Point(element.Location.X, element.Location.Y);

                        selenium.ConvertFromPageToWindow(ref p);
                        selenium.ConvertFromWindowToScreen(ref p);
                        selenium.ConvertFromScreenToGraphics(ref p);

                        ElementLocation = p;
                        ElementSize = element.Size;
                    }
                    else
                    {
                        ElementLocation = default(Point);
                    }
                };

                Application.Run(_form);

                _form.Dispose();
            }
            catch (Exception ex)
            {
                Console.Out.WriteColoredLine(ConsoleColor.DarkRed, "UI render issue "+ ex.Message);
            }

            //Application.Exit();
        }

        private void RunWindow(object sender, DoWorkEventArgs e)
        {
            try
            {
                var factory = (e.Argument as Func<ClickableHudForm>);

                _form = factory.Invoke();
                Console.Out.WriteColored(ConsoleColor.White, $"Displaying overlay...\n");

                Application.Run(_form);

                _form.Dispose();
            }
            catch (Exception ex)
            {
                Console.Out.WriteColoredLine(ConsoleColor.DarkRed, "UI render issue " + ex.Message);
            }

            //Application.Exit();
        }

        public Size ElementSize { get; set; }

        public Point ElementLocation { get; set; }

        public int HoverY { get; set; }

        public int HoverX { get; set; }

        public object RunPingAnimation(int x, int y)
        {
            return null;
        }


        public void Highlight(string text, Rectangle bufferedElementRectangle, Color color)
        {
            Artifacts.Add(new Artifact(text, bufferedElementRectangle, color));
        }

        public List<Artifact> Artifacts { get; set; } = new List<Artifact>()
        {
            new Artifact("Overlay", new Rectangle(100,100, 500, 200), Color.CornflowerBlue)
        };
    }

    internal class Artifact
    {
        public string Text { get; set; }
        public Rectangle Rectangle { get; set; }
        public Color Color { get; set; }

        public Artifact(string text, Rectangle rectangle, Color color)
        {
            Text = text;
            Rectangle = rectangle;
            Color = color;
        }

        public void RenderTo(Graphics painter)
        {
            painter.FillRectangle(new SolidBrush(Color), Rectangle);
            painter.DrawString(Text, new Font(FontFamily.GenericSansSerif, 10),
                    new SolidBrush(Color.FromArgb(1, 1, 1)), Rectangle.X, Rectangle.Y);
        }
    }
}