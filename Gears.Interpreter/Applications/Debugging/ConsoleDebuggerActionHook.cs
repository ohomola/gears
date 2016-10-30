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
using System.Text.RegularExpressions;

namespace Gears.Interpreter.Applications.Debugging
{
    public class ConsoleDebuggerActionHook
    {
        public string Description { get; set; }
        public Action<string> Action { get; set; }
        private readonly string _commandRegEx;

        public ConsoleDebuggerActionHook(string commandRegEx, string description, Action<string> action)
        {
            Description = description;
            Action = action;
            _commandRegEx = commandRegEx;
        }

        public bool Matches(string userInput)
        {
            return new Regex("^"+ _commandRegEx+ "$").IsMatch(userInput);
        }
    }
}