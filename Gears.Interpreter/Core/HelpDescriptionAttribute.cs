using System;

namespace Gears.Interpreter.Core
{
    public class HelpDescriptionAttribute : Attribute
    {
        public HelpDescriptionAttribute(string description)
        {
            Description = description + "\n";
        }

        public string Description { get; set; }
    }
}