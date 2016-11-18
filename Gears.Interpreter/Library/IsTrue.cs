namespace Gears.Interpreter.Library
{
    public class IsTrue : Keyword
    {
        private readonly bool _predicate;

        public IsTrue(bool predicate)
        {
            this._predicate = predicate;
        }

        public string Message { get; set; }

        public override object Run()
        {
            return _predicate;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}