using System;
using System.Drawing;
using Gears.Interpreter.Core.Adapters.UI;
using Point = System.Drawing.Point;

namespace Gears.Interpreter.App.UI.Overlay
{
    public interface IHud : IDisposable
    {
        Point ReadClick();
        object Ping(int x, int y);
        void Highlight(string text, IBufferedElement bufferedElement, Color color);
    }

    public class Hud : IDisposable, IHud
    {
        private bool _isClickable;
        private readonly ISeleniumAdapter _selenium;

        protected Hud(ISeleniumAdapter selenium)
        {
            _isClickable = true;
            _selenium = selenium;
        }

        private Hud()
        {
        }

        public static IHud CreateFor(int timeout)
        {
            var worker = new ClickableHudWorker(() => new ClickableHudForm());

            worker.RunFor(timeout);

            var hud = new Hud();

            hud.Worker = worker;

            return hud;
        }

        private ClickableHudWorker Worker { get; set; }

        public static IHud CreateClickable(ISeleniumAdapter selenium)
        {
            return new Hud(selenium);
        }

        public void Dispose()
        {
        }

        public Point ReadClick()
        {
            var worker = new ClickableHudWorker(() => new ClickableHudForm());

            return worker.RunUntilClicked(_selenium);
        }

        public object Ping(int x, int y)
        {
            var worker = new ClickableHudWorker(() => new ClickableHudForm());

            return worker.RunPingAnimation(x,y);
        }

        public void Highlight(string text, IBufferedElement bufferedElement, Color color)
        {
            Worker.Highlight(text, bufferedElement.Rectangle, color);
        }
    }
}
