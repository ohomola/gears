using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.JavaScripts;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.App.UI.Overlay
{
    public interface IAnnotationOverlay
    {
        void RunSync(string title, int timeout, bool clickThrough);
        Task<Point> RunAsync(string title, int timeout, bool clickThrough);
        void Highlight(string text, Rectangle bufferedElementRectangle, Color backgroundColor, Color lineColor);
        
        Point ClickResult { get; set; }
        IList<Artifact> Artifacts { get; set; }
        bool IsShowing { get; }
        HudForm Form { get; }
        void Stop();
    }

    public class AnnotationOverlay : IAnnotationOverlay, IDisposable
    {
        public Color BackgroundColor { get; set; } = Color.FromArgb(255, 180, 180, 180);
        public Color HeaderColor { get; set; } = Color.FromArgb(255, 150, 255, 255);

        public IList<Artifact> Artifacts { get; set; } = new List<Artifact>();
        public bool IsShowing { get; private set; }

        private readonly ISeleniumAdapter _seleniumAdapter;

        public HudForm Form { get; private set; }
        private int _timeToLive;
        private bool _isOwnerThreadRequestedToStop;
        private string Title;

        public Size ElementSize { get; set; }
        public Point ClickResult { get; set; }
        public Point ElementLocation { get; set; }

        public bool ClickThrough { get; set; }
        public int HoverY { get; set; }

        public int HoverX { get; set; }

        public AnnotationOverlay(ISeleniumAdapter seleniumAdapter)
        {
            _seleniumAdapter = seleniumAdapter;
        }

        public Task<Point> RunAsync(string title, int timeout, bool clickThrough)
        {
            ClickThrough = clickThrough;
            _timeToLive = timeout;
            Title = title;

            return Task.Run(()=>Run());
        }

        public void RunSync(string title, int timeout, bool clickThrough)
        {
            ClickThrough = clickThrough;
            _timeToLive = timeout;
            Title = title;

            Run();
        }

        public void Stop()
        {
            _isOwnerThreadRequestedToStop = true;
        }

        private Point Run()
        {
            WaitUntilPreviousExecutionsAreCompleted();

            var ownerThread = StartFormOwnerThread();

            WaitUntilFormIsCreatedByOwnerThread();

            IsShowing = true;

            try
            {
                var graphics = Form.CreateGraphics();

                graphics.CompositingMode = CompositingMode.SourceOver;

                var backBuffer = new Bitmap(Form.Width, Form.Height);

                var overlayCloseTime = DateTime.Now.AddMilliseconds(_timeToLive);

                while (Form.ClickPosition == default(Point) 
                    && overlayCloseTime > DateTime.Now
                    && _isOwnerThreadRequestedToStop == false)
                {
                    Render(Graphics.FromImage(backBuffer), _seleniumAdapter);

                    graphics.DrawImage(backBuffer, 0, 0);
                }
                
            }
            catch (Exception e)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Yellow, $"Error rendering overlay: {e.ToString()}");
            }

            Form.Invoke((MethodInvoker)delegate
            {

                Form.Close();
                Form.Dispose();
                _isOwnerThreadRequestedToStop = false;
            });

            WaitToPreventBrowserEventsTriggerAtTheResultTime();
            
            ClickResult = Form.ClickPosition;

            
            Form = null;

            ownerThread.Join();

            

            IsShowing = false;

            return ClickResult;
        }

        private void WaitUntilFormIsCreatedByOwnerThread()
        {
            while (Form == null)
            {
                Thread.Sleep(50);
            }
        }

        private void WaitUntilPreviousExecutionsAreCompleted()
        {
            if (IsShowing && !_isOwnerThreadRequestedToStop)
            {
                throw new InvalidOperationException("Two Overlays cannot run at once.");
            }

            while (IsShowing)
            {
                Thread.Sleep(50);
            }
        }

        private Thread StartFormOwnerThread()
        {
            var ownerThread = new Thread(FormThreadRun);

            ownerThread.Start();

            return ownerThread;
        }


        public void Highlight(string text, Rectangle bufferedElementRectangle, Color backgroundColor, Color lineColor)
        {
            Artifacts.Add(new Artifact(text, bufferedElementRectangle, backgroundColor, lineColor));
        }

        /// <summary>
        /// Suspected to cause frequent Stale Element exceptions due to elements disappearing after releasing the Overlay
        /// (for instance when hover event triggers in browser)
        /// </summary>
        private static void WaitToPreventBrowserEventsTriggerAtTheResultTime()
        {
            Thread.Sleep(250);
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

            var contentTopLeft = new Point(windowRect.Left, windowRect.Top);
            contentTopLeft.X += selenium.ContentOffsetX();
            contentTopLeft.Y += selenium.ContentOffsetY();
            if (!ClickThrough)
            {
                backBuffer.FillRectangle(new SolidBrush(BackgroundColor), contentTopLeft.X + 8, contentTopLeft.Y, width - selenium.ContentOffsetX() - 16, height - selenium.ContentOffsetY() - 18);
            }

            backBuffer.FillRectangle(new SolidBrush(HeaderColor), topLeft.X+8, topLeft.Y+22, width-16, 24);
            backBuffer.DrawRectangle(new Pen(HeaderColor, 10), topLeft.X + 8, topLeft.Y, width - 16, height-8);
            
            backBuffer.DrawString(Title, new Font(FontFamily.GenericSansSerif, 14), new SolidBrush(Color.FromArgb(255, 10, 10, 10)), topLeft.X+340, topLeft.Y+22);

            if (ElementLocation != default(Point))
            {
                backBuffer.DrawRectangle(new Pen(Color.FromArgb(255, 0, 50, 150), 4),
                    ElementLocation.X-2, ElementLocation.Y-2, ElementSize.Width+4, ElementSize.Height+4);
                backBuffer.FillRectangle(new SolidBrush(Color.FromArgb(255, 100, 255, 255)), 
                    ElementLocation.X-2, ElementLocation.Y-2, ElementSize.Width+4, ElementSize.Height+4);
            }

            foreach (var artifact in Artifacts)
            {
                var rectangle = new Rectangle(artifact.Rectangle.X, artifact.Rectangle.Y, artifact.Rectangle.Width, artifact.Rectangle.Height);

                _seleniumAdapter.ConvertFromPageToWindow(ref rectangle);
                _seleniumAdapter.ConvertFromWindowToScreen(ref rectangle);
                _seleniumAdapter.ConvertFromScreenToGraphics(ref rectangle);

                backBuffer.DrawRectangle(new Pen(artifact.LineColor, 6),
                    rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                backBuffer.FillRectangle(new SolidBrush(artifact.BackgroundColor),
                    rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

                backBuffer.DrawString(artifact.Text, new Font(FontFamily.GenericSansSerif, 12), new SolidBrush(Color.FromArgb(255, 10, 10, 10)), rectangle.X, rectangle.Y);
            }
        }

        /// <summary>
        /// Form owner
        /// </summary>
        private void FormThreadRun()
        {
            try
            {
                Form = new HudForm(ClickThrough);
                var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

                if (!ClickThrough)
                {
                    Form.MouseMove += HandleMouseMove(selenium);
                }

                Application.Run(Form);
            }
            catch (Exception ex)
            {
                Console.Out.WriteColoredLine(ConsoleColor.DarkRed, "UI render issue " + ex.Message);
            }
        }

        private MouseEventHandler HandleMouseMove(ISeleniumAdapter selenium)
        {
            return delegate(object o, MouseEventArgs args)
            {
                this.HoverX = ((MouseEventArgs) args).X;
                this.HoverY = ((MouseEventArgs) args).Y;

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

                //Console.Out.Wipe();
                //Console.Out.BeginRewritableLine();
                //Console.Out.WriteColored(ConsoleColor.Yellow, $"X {HoverX:D4} Y {HoverY:D4} ");
                //Console.Out.WriteColored(ConsoleColor.Green, $"{(element == null ? "                        " : $" -- Button {element.Text} --")}");


                //Console.Out.WriteColored(ConsoleColor.Magenta, $"width {width} height {height} ");


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
        }

        public void Dispose()
        {
            _seleniumAdapter?.Dispose();
            Form?.Dispose();
        }
    }
}