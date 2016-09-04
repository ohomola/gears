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
using System.ComponentModel;
using System.Linq;

namespace Gears.Interpreter.Core.Extensions
{
    public static class StringExtensions
    {
        public static T AsEnum<T>(this string s)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), s);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Invalid argument for " + typeof(T).Name + " enum:" + e.Message);
            }
        }

        public static string[] Split(this string t, string separator)
        {
            return t.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries);
        }


        /// <summary>
        /// This is a 'flow-ish' extension for string format. No big reason to have it, it's just a pain to use the native static method.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static string FormatWith(this string s, params string[] arguments)
        {
            return string.Format(s, arguments);
        }

    }
}