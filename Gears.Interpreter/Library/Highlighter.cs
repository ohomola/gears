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
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Debugging.Overlay;

namespace Gears.Interpreter.Library
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
            using (var overlay = new Overlay())
            {
                var handle = seleniumAdapter.GetChromeHandle();
                overlay.Init();
                int i = 0;
                foreach (var element in elements)
                {
                    i++;

                    var p = new Point(element.Rectangle.Left, element.Rectangle.Top);
                    seleniumAdapter.BrowserToClient(ref p);

                    overlay.DrawStuff(handle, i, p.X, p.Y,
                        overlay.Graphics, element.Rectangle.Width, element.Rectangle.Height, 
                        i==(1+selectionIndex)?selectionColor:innerColor, 
                        outerColor);
                }

                Console.Out.WriteColoredLine(ConsoleColor.White,
                    $"{elements.Count()} elements highlighted on screen. Press enter to continue (highlighting will disappear).");
                Console.ReadLine();
            }
        }

        public static void HighlightPoints(ISeleniumAdapter selenium, params Point[] points)
        {
            using (var overlay = new Overlay())
            {
                var handle = selenium.GetChromeHandle();
                overlay.Init();


                for (int index = 0; index < points.Length; index++)
                {
                    var point = points[index];
                    overlay.DrawStuff(handle, index, point.X - 5, point.Y - 5, overlay.Graphics, 10, 10, Color.FromArgb(255, 0, 255, 255), Color.FromArgb(255, 255, 0, 255));
                }
            }
        }
    }
}
