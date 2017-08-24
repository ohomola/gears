namespace Gears.Interpreter.Core.Interpretation
{
    public class WarningAnswer : InformativeAnswer, INegativeAnswer
    {
        public WarningAnswer(object response) : base(response)
        {
        }
    }
}