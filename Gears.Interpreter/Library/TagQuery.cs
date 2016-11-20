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

using System.Text.RegularExpressions;
using Gears.Interpreter.Core.Extensions;

namespace Gears.Interpreter.Library
{
    public class TagQuery
    {
        private enum TagType
        {
            Link,
            TextField,
            Button
        }

        private readonly string _query;

        private TagType _tagType;

        public TagQuery(string query)
        {
            _query = query;
            OneBasedOrder = ParseOrder(query);
            IsFromRight = query.ToLower().Contains("right");
            TagNames = new[] { "button" };
            _tagType = TagType.Button;
            if (query.ToLower().Contains("input"))
            {
                TagNames = new[] { "input", "textArea" };
                _tagType = TagType.TextField;
            }
            if (query.ToLower().Contains("link"))
            {
                _tagType = TagType.Link;
                TagNames = new[] { "a" };
            }
        }

        private int ParseOrder(string @where)
        {
            @where = @where.ToLower();
            if (@where.Contains("first")) return 1;
            if (@where.Contains("second")) return 2;
            if (@where.Contains("third")) return 3;
            if (@where.Contains("fourth")) return 4;
            if (@where.Contains("fifth")) return 5;

            var regex = new Regex("\\D*(\\d+)\\D*");
            var result = regex.Match(@where);
            if (result.Success)
            {
                var number =int.Parse(result.Groups[1].Value);
                return number;
            }
            return 1;
        }

        public int OneBasedOrder { get; set; }
        public bool IsFromRight { get; set; }
        public string[] TagNames { get; set; }

        public override string ToString()
        {
            return $"{OneBasedOrder.ToOrdinalString()} {_tagType} from the {(IsFromRight ? "right" : "left")}";
        }
    }
}