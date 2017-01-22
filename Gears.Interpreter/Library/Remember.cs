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

using System.Linq;
using Gears.Interpreter.Data;

namespace Gears.Interpreter.Library
{
    public class Remember : Keyword
    {
        public virtual string Variable { get; set; }
        public virtual string What { get; set; }

        public Remember()
        {
        }

        public Remember(string variable, string what)
        {
            Variable = variable;
            What = what;
        }

        public override object DoRun()
        {
            var existingMemory = Data.GetAll<RememberedText>().FirstOrDefault(x => x.Variable.ToLower() == Variable.ToLower());
            if (existingMemory != null)
            {
                existingMemory.What = What;
            }
            else
            {
                Data.DataAccesses.OfType<SharedObjectDataAccess>().First().Add(new RememberedText(Variable, What));
            }

            return null;
        }

        public override string ToString()
        {
            return $"Remember {What} as variable called '{Variable}'";
        }
    }

    public class RememberedText
    {
        public virtual string Variable { get; set; }
        public virtual string What { get; set; }

        public RememberedText(string variable, string what)
        {
            Variable = variable;
            What = what;
        }

        public RememberedText()
        {
        }

        public override string ToString()
        {
            return $"Memory text [{Variable}] = '{What}'";
        }
    }
}