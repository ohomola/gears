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

namespace Gears.Interpreter.Library
{
    public class DirectionIndex
    {
        public DirectionIndex(string @where)
        {
            Order = ParseOrder(@where);
            IsFromRight = @where.ToLower().Contains("right");
        }

        private bool ParseYDirection(string @where)
        {
            return false;
        }

        private int ParseOrder(string @where)
        {
            return 0;
        }

        public int Order { get; set; }
        public bool IsFromRight { get; set; }
     
    }
}