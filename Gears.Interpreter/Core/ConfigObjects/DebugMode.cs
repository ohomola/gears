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
    public class SkipAssertions : IHaveDocumentation, IConfig
    {
        public string CreateDocumentationMarkDown()
        {
            return $@"## {this.GetType().Name}

Configuration Type used to turn off assertions executions.

> Note: This is a configuration, not an active keyword. It changes behaviour of your scenario by merely existing in your data context. Use 'Use' keyword to add it to your context and 'Off' keyword to take it away.


#### Scenario usages
| Discriminator | What |
| ------------- | ---- |
| Use           | SkipAssertions |
| IsVisible         | Somthing which is broken and we know it |
| Off           | SkipAssertions|

#### Console usages
    use SkipAssertions
    run
    SkipAssertions off
";
        }

        public string CreateDocumentationTypeName()
        {
            return nameof(SkipAssertions);
        }

        public override string ToString()
        {
            return "SkipAssertions (any assertions keyword is automatically skipped)";
        }
    }

    public interface IConfig
    {
    }

    public class DebugMode : IAutoRegistered
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

        public void Register(IInterpreter applicationLoop)
        {
        }
    }
}