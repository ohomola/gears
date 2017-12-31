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
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.Library
{
    [HelpDescription("remember <var> <val> \t saves/updates a variable with a specified value.")]
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


        #region Documentation

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Saves a specified value to a variable for later use. Your currently memorized variables are displayed in the console status.
#### Scenario usages
| Discriminator | What               | Variable     |
| ------------- | -----              | -----        |
| Remember      | user1              | myLogin      |
| Fill          | login              | [myLogin]    |
| Remember      | {{Generate.Word(3)}} | randomWord   |
| Fill          | password           | [randomWord] |

#### Console usages
     remember myUser John
";
        }

        #endregion


        public override string Instruction
        {
            set
            {
                var args = value.Split(' ');

                args = new[]
                {
                    args.First(),
                    string.Join(" ", args.Skip(1))
                };

                if (args.Length < 2)
                {
                    throw new ArgumentException("Must provide two parameters.");
                }
                Variable = args[0];
                What = args[1];
            }
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

            if (Variable.Contains("[") || Variable.Contains("]"))
            {
                throw new ArgumentException("Variable name must not contain [] brackets. Please use brackets to reference an existing variable later, not while decalaring it.");
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
}