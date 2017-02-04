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
using System.IO;

namespace Gears.Interpreter.Applications.Debugging
{
    public static class ConsoleExtensions
    {
        public static void WriteColored(this TextWriter writer, ConsoleColor color, string text)
        {
            var oldColor = Console.ForegroundColor;

            Console.ForegroundColor = color;

            writer.Write(text);

            Console.ForegroundColor = oldColor;
        }

        public static void BeginRewritableLine(this TextWriter writer)
        {
            RewritableAreaCaretStart = Console.CursorLeft;
        }

        public static int RewritableAreaCaretStart { get; set; }

        public static void Wipe(this TextWriter writer)
        {
            Console.Out.Write(string.Empty.PadLeft(Math.Max(0,Console.CursorLeft - RewritableAreaCaretStart), '\b'));
        }

        public static void WriteColoredLine(this TextWriter writer, ConsoleColor color, string text)
        {
            WriteColored(writer, color, text + "\n\r");
        }
    }
}