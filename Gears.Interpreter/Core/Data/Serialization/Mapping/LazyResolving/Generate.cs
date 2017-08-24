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

namespace Gears.Interpreter.Core.Data.Serialization.Mapping.LazyResolving
{
    public class Generate
    {
        public static string Word(int length = 3, string alphabet = "abcdefghijklmnopqrst")
        {
            var random = new System.Random();

            string result = null;

            for (int i = 0; i < length; i++)
            {
                result = result + alphabet[random.Next(alphabet.Length)];
            }

            return result;
        }

        public static int Number(int maxInclusive)
        {
            return Number(0, maxInclusive);
        }

        public static int Number(int minInclusive, int maxInclusive)
        {
            return new Random().Next(maxInclusive - minInclusive)+minInclusive;
        }
    }
}