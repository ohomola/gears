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

using Gears.Interpreter.App;
using Gears.Interpreter.App.Configuration;

namespace Gears.Interpreter.Core.ConfigObjects
{
    public class DebugMode : IAutoRegistered, IHaveDocumentation
    {
        public DebugMode()
        {
            IsActive = true;
        }

        public DebugMode(bool isActive)
        {
            IsActive = isActive;
        }

        public bool IsActive { get; set; }

        public void Register(IInterpreter interpreter)
        {
        }

        public string CreateDocumentationMarkDown()
        {
            return $@"
## DebugMode

Configuration switch used to make Interpreter work in debug mode, halting before each step waiting for user action.
> Note: To add this to any scenario, you can also use a command-line argument when executing Gears Interpreter -{nameof(DebugMode)}
";
        }

        public string CreateDocumentationTypeName()
        {
            return "DebugMode";
        }
    }
}