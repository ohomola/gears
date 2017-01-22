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
using System.Threading;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Registrations;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Tests.Pages;
using Microsoft.Office.Interop.Excel;

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

            try
            {
                Bootstrapper.Register(args);

                var interpreter = Bootstrapper.ResolveInterpreter();

                Render(interpreter.Please("start"));

                Render(interpreter.Please("help"));

                while (interpreter.IsAlive)
                {
                    Render(interpreter.Please("status"));

                    var answer = interpreter.Please(ReadLineFromConsole());

                    Render(answer);
                }

                return OkStatusCode;
            }
            catch (Exception e)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Red, "Error running application: " + e.GetAllMessages());
                ReadLineFromConsole();
                return CriticalErrorStatusCode;
            }
            finally
            {
                Thread.Sleep(1000);

                Bootstrapper.Release();
            }
        }

        private static string ReadLineFromConsole()
        {
            UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());

            return Console.ReadLine();
        }

        private static void Render(IAnswer answer)
        {
            var outputs = ConsoleOutputMapper.MapToOutput(answer);

            foreach (var consoleOutput in outputs)
            {
                Console.Out.WriteColored(consoleOutput.Color, consoleOutput.Text);
            }

            Console.Out.Write("\n");
        }
    }

    public class ConsoleOutput
    {
        public ConsoleColor Color;
        public string Text;

        public ConsoleOutput(ConsoleColor color, string text)
        {
            Color = color;
            Text = text;
        }

        public override string ToString()
        {
            return $"[{Color}] '{Text}'";
        }
    }

    internal class TestCaseDefinition
    {
        public string JUnitReportPath { get; set; }
    }
}
