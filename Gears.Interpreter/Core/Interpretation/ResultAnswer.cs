namespace Gears.Interpreter.Core.Interpretation
{
    public class ResultAnswer : InformativeAnswer
    {
        public int Code { get; set; }

        public ResultAnswer(int code) : base(code)
        {
            Code = code;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Code)}: {Code}";
        }
    }
}