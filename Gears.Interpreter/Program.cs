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
using System.Windows.Forms;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter
{
    public class Program
    {
        public const int CriticalErrorStatusCode = -2;
        public const int ScenarioFailureStatusCode = -1;
        public const int OkStatusCode = 0;

        private static int Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.WriteLine("----------------------------------------");
            Console.Out.WriteLine("Gears Scenario Debugger");
            Console.Out.WriteLine("Copyright 2016 Ondrej Homola");
            Console.Out.WriteLine("----------------------------------------");
            Console.ResetColor();

            var applicationLoop = new ApplicationLoop(args);

            try
            {
                return applicationLoop.Run();
            }
            catch (Exception e)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Red, "Error running application: " + e.GetAllMessages());
                return CriticalErrorStatusCode;
            }
        }

        
    }

    internal class TestCaseDefinition
    {
        public string JUnitReportPath { get; set; }
    }
}
