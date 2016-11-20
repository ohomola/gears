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
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Serialization.Mapping;
using Gears.Interpreter.Data.Serialization.Mapping.LazyResolving;

namespace Gears.Interpreter.Core.Extensions
{
    public static class IntegerExtension
    {
        public static string ToOrdinalString(this int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }

        }
    }

    public static class ObjectExtensions
    {
        public static T AsLazyEvaluated<T>(this T obj) where T : class
        {
            if (ServiceLocator.IsInitialised())
            {
                return ServiceLocator.Instance.Resolve<ILazyExpressionResolver>().ToProxy(obj.GetType(), obj) as T;
            }

            throw new InvalidOperationException("Cannot create lazy evaluated object without registered Lazy Value Resolver");
        }
    }

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