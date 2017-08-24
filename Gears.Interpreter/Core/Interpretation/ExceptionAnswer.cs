namespace Gears.Interpreter.Core.Interpretation
{
    public class ExceptionAnswer : InformativeAnswer, INegativeAnswer
    {
        public ExceptionAnswer(object response) : base(response)
        {
        }
    }
}