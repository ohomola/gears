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
using System.Linq;
using System.Text.RegularExpressions;
using Gears.Interpreter.Core.Extensions;

namespace Gears.Interpreter.Library
{
    public class ButtonQuery
    {
        public ButtonQuery(string query)
        {
            Order = ParseOrder(query);
            IsFromRight = query.ToLower().Contains("right");

            TagName = "button";
            if (query.ToLower().Contains("input")) TagName = "input";
            if (query.ToLower().Contains("link")) TagName = "a";
        }

        private int ParseOrder(string @where)
        {
            @where = @where.ToLower();
            if (@where.Contains("first")) return 0;
            if (@where.Contains("second")) return 1;
            if (@where.Contains("third")) return 2;
            if (@where.Contains("fourth")) return 3;
            if (@where.Contains("fifth")) return 4;

            var regex = new Regex(".*(\\d+).*");
            var result = regex.Match(@where);
            if (result.Success)
            {
                var number =int.Parse(result.Groups[1].Value)-1;
                return number;
            }
            return 0;
        }

        public int Order { get; set; }
        public bool IsFromRight { get; set; }
        public string TagName { get; set; }

        public override string ToString()
        {
            return $"{Order.ToOrdinalString()} button from the {(IsFromRight ? "right" : "left")}";
        }
    }
}