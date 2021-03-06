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
using System.Xml.Serialization;
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Extensions;

namespace Gears.Interpreter.Library.Assistance
{
    public class Comment : Keyword
    {
        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Writes text to console. Use this to comment scenario.
#### Scenario usage
| Discriminator | Text |
| ------------- | ---- |
| Comment | Hello world |

#### Console usage
    comment Hello world";
        }

        public virtual string Text { get; set; }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

        public Comment(string text)
        {
            Text = text;
        }

        public Comment()
        {
        }

        public override string Instruction
        {
            set { Text = value; }
        }

        public override object DoRun()
        {
            Console.Out.WriteColoredLine(ConsoleColor.DarkGreen,"\""+Text+ "\"\n");

            return Text;
        }

        public override string ToString()
        {
            return $"Comment: {Text}";
        }
    }
}
