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

namespace Gears.Interpreter.Core.Data.Core
{
    public class CorruptObject
    {
        public Exception Exception { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return $"Corrupt data. Problem description: '{Exception?.Message ?? Description}'";
        }
    }
}
