using System;

namespace Gears.Interpreter.Core
{
    public class NotLoggedAttribute : Attribute
    {
    }

    public class UserDescriptionAttribute : Attribute
    {
        public UserDescriptionAttribute(string description)
        {
            Description = description + "\n";
        }

        public string Description { get; set; }
    }
}