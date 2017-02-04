using System;
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
    internal class HudWorker
    {
        private readonly Func<ClickableHudForm> _hudFormFactory;
        private ClickableHudForm _form;
        public BackgroundWorker BackgroundWorker { get; set; }

        public HudWorker(Func<ClickableHudForm> hudFormFactory)
        {
            _hudFormFactory = hudFormFactory;
        }

        public Point RunUntilClicked(ISeleniumAdapter selenium)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += DoWork;
            bw.RunWorkerAsync(_hudFormFactory);

            while (_form == null)
            {
                Thread.Sleep(50);
            }


            var graphics = _form.CreateGraphics();

            graphics.CompositingMode = CompositingMode.SourceOver;

            
            while (_form.ClickPosition == default(Point))
            {
                Thread.Sleep(5);
                Render(_form, graphics, selenium);
            }


            return _form.ClickPosition;
        }

        private void Render(ClickableHudForm form, Graphics graphics, ISeleniumAdapter selenium)
        {
            try
            {
                var handle = selenium.GetChromeHandle();
                var p = new Point(0, 0);

                var windowRect = selenium.BrowserWindowScreenRectangle;
                var topLeft = new Point(windowRect.Left, windowRect.Top);
                var bottomRight = new Point(windowRect.Right, windowRect.Bottom);

                UserInteropAdapter.ScreenToGraphics(ref topLeft);
                UserInteropAdapter.ScreenToGraphics(ref bottomRight);


                Bitmap bitmap = new Bitmap(form.Width, form.Height);
                Graphics g = Graphics.FromImage(bitmap);

                g.Clear(Color.Black);
                

                float height = Math.Abs(bottomRight.Y - topLeft.Y);
                float width = Math.Abs(bottomRight.X - topLeft.X);
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 100, 255, 255)), topLeft.X+8, topLeft.Y+22, width-16, height-30);

                
                var contentTopLeft = new Point(windowRect.Left, windowRect.Top);
                contentTopLeft.X += selenium.ContentOffsetX();
                contentTopLeft.Y += selenium.ContentOffsetY();
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 100, 255, 150)), contentTopLeft.X+8, contentTopLeft.Y, width- selenium.ContentOffsetX()-16, height- selenium.ContentOffsetY()-18);

                //g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 128)), topLeft.X+280, topLeft.Y, 320, 50);
                g.DrawString("Click an element", new Font(FontFamily.GenericSansSerif, 14), new SolidBrush(Color.FromArgb(255, 10, 10, 10)), topLeft.X+340, topLeft.Y+22);

                if (ElementLocation != default(Point))
                {
                    g.DrawRectangle(new Pen(Color.FromArgb(255, 0, 50, 150), 2),
                        ElementLocation.X, ElementLocation.Y, ElementSize.Width, ElementSize.Height);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(255, 100, 255, 255)), 
                        ElementLocation.X, ElementLocation.Y, ElementSize.Width, ElementSize.Height);
                }

                graphics.DrawImage(bitmap,0,0);

            }
            catch (Exception)
            {
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
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
                    
                    var point = new Point(this.HoverX, this.HoverY);
                    selenium.ConvertFromScreenToWindow(ref point);
                    selenium.ConvertFromWindowToPage(ref point);

                    var elements = selenium.WebDriver.GetElementByCoordinates(point.X, point.Y);

                    var element = elements.LastOrDefault(x => x.IsClickable());
                    
                    Console.Out.Wipe();
                    Console.Out.BeginRewritableLine();
                    Console.Out.WriteColored(ConsoleColor.Yellow, $"X {HoverX:D4} Y {HoverY:D4} ");
                    Console.Out.WriteColored(ConsoleColor.Green, $"{(element == null ? "                        " : $" -- Button {element.Text} --")}");

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
            catch (Exception)
            {
            }

            //Application.Exit();
        }

        public Size ElementSize { get; set; }

        public Point ElementLocation { get; set; }

        public int HoverY { get; set; }

        public int HoverX { get; set; }
    }
}