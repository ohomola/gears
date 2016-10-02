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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace Gears.Interpreter.Library
{
    public class Show : Keyword
    {
        public string Where { get; set; }

       
        public Show(string @where)
        {
            Where = @where;
        }

        public override object Run()
        {
            var elements = Selenium.WebDriver.GetAllByTagNameAndLocation(new ButtonQuery(Where));
                
                var YOffset =
                    (int)
                    Math.Abs(
                        (long) Selenium.WebDriver.RunLibraryScript("return window.innerHeight - window.outerHeight"));
                var XOffset =
                    (int)
                    Math.Abs((long) Selenium.WebDriver.RunLibraryScript("return window.innerWidth - window.outerWidth"));

                var scrollOffset =
                    (int)
                    Math.Abs(
                        (long)
                        Selenium.WebDriver.RunLibraryScript(
                            "return window.pageYOffset || document.documentElement.scrollTop"));
                //var top  = window.pageYOffset || document.documentElement.scrollTop,

                if (elements == null)
                {
                    return null;
                }

            using (var overlay = new Overlay())
            {
                overlay.Init();
                int i = 0;
                foreach (var element in elements)
                {
                    i++;
                    DrawStuff(i, element.Location.X + XOffset, element.Location.Y + YOffset - scrollOffset,
                        overlay.Graphics);
                }

                Console.Out.WriteColoredLine(ConsoleColor.White,
                    $"{elements.Count} elements highlighted on screen. Press enter to continue (highlighting will disappear).");
                Console.ReadLine();
            }

            return null;
        }

        private void DrawStuff(int number, int clientX, int clientY, Graphics overlayGraphics)
        {
            var point = new Point(clientX, clientY);
            var handle = Selenium.GetChromeHandle();
            ClientToScreen(handle, ref point);
            UserInteropAdapter.ScreenToGraphics(ref point);
            
            overlayGraphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 255, 255)), point.X, point.Y, 20, 20);
            overlayGraphics.DrawString(number.ToString(), new Font(FontFamily.GenericSansSerif, 10), new SolidBrush(Color.DarkMagenta),point.X, point.Y);
            overlayGraphics.DrawRectangle(new Pen(Color.FromArgb(255, 255,0,255)), point.X, point.Y, 21,21 );
        }

        public override string ToString()
        {
            return $"Show {Where}";
        }

        [DllImport("user32", ExactSpelling = true, SetLastError = true)]
        internal static extern bool ClientToScreen(IntPtr hWnd, [In, Out] ref Point lpPoint);

        [DllImport("user32", ExactSpelling = true, SetLastError = true)]
        internal static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref System.Drawing.Point pt, [MarshalAs(UnmanagedType.U4)] int cPoints);

        [DllImport("user32", ExactSpelling = true, SetLastError = true)]
        internal static extern bool GetWindowRect(IntPtr hWndTo,[In, Out] ref RECT rect);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        
            public int Top;        
            public int Right;      
            public int Bottom;
        }

        

        
    }
}
