namespace Gears.Interpreter.Core.ConfigObjects
{
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