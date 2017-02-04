using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gears.Interpreter.Adapters;
using Point = System.Drawing.Point;

namespace Gears.Interpreter.Applications.Debugging.Overlay
{
    public interface IHud : IDisposable
    {
        Point ReadClick();
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

        public static IHud CreateNonClickable()
        {
            return new Hud();
        }

        public static IHud CreateClickable(ISeleniumAdapter selenium)
        {
            return new Hud(selenium);
        }

        public void Dispose()
        {
        }

        public Point ReadClick()
        {
            var worker = new HudWorker(() => new ClickableHudForm());

            return worker.RunUntilClicked(_selenium);
        }
    }
}
