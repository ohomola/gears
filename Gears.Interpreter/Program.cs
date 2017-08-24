﻿#region LICENSE
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
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.Core.Interpretation;

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
            Console.Out.WriteLine("----------------------------------------");
            Console.ResetColor();

            try
            {
                Bootstrapper.RegisterArguments(args);

                var interpreter = Bootstrapper.ResolveInterpreter();

                ConsoleView.Render(interpreter.Please("start"));

                while (interpreter.IsAlive)
                {
                    ConsoleView.Render(interpreter.Please("status"));
                    
                    var answer = interpreter.Please(interpreter.Continue());

                    if (answer is IFollowupQuestion)
                    {
                        var possibleReplies = ((IFollowupQuestion) answer).Options;

                        interpreter.Language.AddFollowupOptions(possibleReplies);
                    }
                    else
                    {
                        interpreter.Language.ResetFollowupOptions();
                    }

                    ConsoleView.Render(answer);
                }

                return OkStatusCode;
            }
            catch (Exception e)
            {
                ConsoleView.Render(ConsoleColor.Red, "Error running application: " + e);

                return CriticalErrorStatusCode;
            }
            finally
            {
                Bootstrapper.Release();
            }
        }
    }
}
