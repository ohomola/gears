#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears, a software automation and assistance framework.

Gears is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Gears is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core.Extensions;

namespace Gears.Interpreter.Core.Adapters.UI.Interoperability
{
    public static class Highlighter
    {
        public static void HighlightElements(ISeleniumAdapter seleniumAdapter, params IBufferedElement[] elements)
        {
            HighlightElements(seleniumAdapter, elements.ToList());
        }

        public static void HighlightElements(ISeleniumAdapter seleniumAdapter, IEnumerable<IBufferedElement> elements)
        {
            HighlightElements(seleniumAdapter,elements, Color.FromArgb(255, 0, 255, 255), Color.FromArgb(255, 255, 0, 255), 0, Color.FromArgb(255, 0, 255, 255));
        }

        public static void HighlightElements(ISeleniumAdapter seleniumAdapter, IEnumerable<IBufferedElement> elements, int selectionIndex)
        {
            HighlightElements(seleniumAdapter, elements, Color.FromArgb(255, 0, 255, 255), Color.FromArgb(255, 255, 0, 255), selectionIndex, Color.FromArgb(255, 0, 255, 0));
        }

        public static void HighlightElements(ISeleniumAdapter seleniumAdapter, IEnumerable<IBufferedElement> elements, Color innerColor, Color outerColor, int selectionIndex, Color selectionColor)
        {
            HighlightElements(()=>Console.ReadLine(), seleniumAdapter,elements, innerColor,outerColor,selectionIndex,selectionColor);
        }

        public static void HighlightElements(int wait, ISeleniumAdapter seleniumAdapter, IEnumerable<IBufferedElement> elements, Color innerColor, Color outerColor, int selectionIndex, Color selectionColor)
        {
            HighlightElements(() => Thread.Sleep(wait), seleniumAdapter, elements, innerColor, outerColor, selectionIndex, selectionColor);
        }

        public static void HighlightElements(Action action, ISeleniumAdapter seleniumAdapter, IEnumerable<IBufferedElement> elements, Color innerColor, Color outerColor, int selectionIndex, Color selectionColor ,int xOffset = 0, int yOffset =0)
        {
            using (var overlay = new Overlay())
            {
                if (selectionIndex != -1)
                {
                    seleniumAdapter.PutElementOnScreen(elements.ElementAt(selectionIndex).WebElement);
                }
                var handle = seleniumAdapter.BrowserHandle;
                overlay.Init();
                int i = 0;
                foreach (var element in elements)
                {
                    i++;

                    var p = new Point(element.Rectangle.Left, element.Rectangle.Top);
                    seleniumAdapter.ConvertFromPageToWindow(ref p);

                    overlay.DrawStuff(handle, i, p.X, p.Y,
                        overlay.Graphics, element.Rectangle.Width, element.Rectangle.Height,
                        i == (1 + selectionIndex) ? selectionColor : innerColor,
                        outerColor);

                    if (xOffset != 0 || yOffset != 0)
                    {
                        overlay.Graphics.DrawLine(Pens.Red, 
                            p.X+ element.Rectangle.Width/2,
                            p.Y + element.Rectangle.Height/2,
                            p.X + element.Rectangle.Width / 2 + xOffset,
                            p.Y + element.Rectangle.Height / 2 + yOffset);
                    }
                }

                Console.Out.WriteColoredLine(ConsoleColor.White,
                    $"{elements.Count()} elements highlighted on screen. Press enter to continue (highlighting will disappear).");
                

                action.Invoke();
            }
        }

        public static void HighlightPoints(int wait, ISeleniumAdapter selenium, params Point[] points)
        {
            using (var overlay = new Overlay())
            {
                var handle = selenium.BrowserHandle;
                overlay.Init();


                for (int index = 0; index < points.Length; index++)
                {
                    var point = points[index];
                    overlay.DrawStuff(handle, index, point.X - 15, point.Y - 15, overlay.Graphics, 30, 30, Color.FromArgb(255, 0, 255, 255), Color.FromArgb(255, 255, 0, 255));
                }

                Thread.Sleep(wait);
            }
        }

        public static void PingScreen(int x, int y)
        {
            using (var hud = Hud.CreateFor(750))
            {
                var point = hud.Ping(x, y);
            }
        }
    }
}
