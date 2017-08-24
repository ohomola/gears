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
using System.Text;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;
using Microsoft.Win32.SafeHandles;

namespace Gears.Interpreter.Core.Adapters.UI.Interoperability
{
    public static class KernelInteropAdapter
    {
        public static void ConfigureConsoleWindow()
        {
            KernelBindings.AllocConsole();
            var stdHandle = KernelBindings.GetStdHandle(KernelBindings.STD_OUTPUT_HANDLE);
            var safeFileHandle = new SafeFileHandle(stdHandle, true);
            var fileStream = new FileStream(safeFileHandle, System.IO.FileAccess.Write);
            var encoding = Encoding.GetEncoding(KernelBindings.MY_CODE_PAGE);
            var standardOutput = new StreamWriter(fileStream, encoding) {AutoFlush = true};

            Console.SetOut(standardOutput);

            Console.SetBufferSize(Console.LargestWindowWidth, KernelBindings.ConsoleBufferSize);

            Console.SetWindowSize(Math.Min(90, Console.LargestWindowWidth), Console.LargestWindowHeight - 8);
        }
    }
}
