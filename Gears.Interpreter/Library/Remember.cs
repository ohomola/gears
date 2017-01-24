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
using Gears.Interpreter.Applications;
using Gears.Interpreter.Data;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [UserDescription("remember <var> <val> \t saves/updates a variable with a specified value.")]
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

        public override IKeyword FromString(string textInstruction)
        {
            var args = ExtractTwoParametersFromTextInstruction(textInstruction);
            return new Remember(args[0], args[1]);
        }

        public override object DoRun()
        {
            if (string.IsNullOrEmpty(Variable))
            {
                throw new ArgumentException("Variable name cannot be empty");
            }

            if (string.IsNullOrEmpty(What))
            {
                throw new ArgumentException("Value cannot be empty");
            }

            var existingMemory = Data.GetAll<RememberedText>().FirstOrDefault(x => x.Variable.ToLower() == Variable.ToLower());
            if (existingMemory != null)
            {
                existingMemory.What = What;
                return new SuccessAnswer($"Updated '{existingMemory.Variable}' to new value \'{existingMemory.What}\' to memory.");
            }
            else
            {
                var rememberedText = new RememberedText(Variable, What);
                Data.DataAccesses.OfType<SharedObjectDataAccess>().First().Add(rememberedText);

                return new SuccessAnswer($"Saved \'{rememberedText.What}\' as variable '{rememberedText.Variable}'.");
            }

            
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