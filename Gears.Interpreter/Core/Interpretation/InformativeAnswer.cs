using System.Collections.Generic;

namespace Gears.Interpreter.Core.Interpretation
{
    public class InformativeAnswer : IInformativeAnswer
    {
        public object Body { get; }
        public List<IAnswer> Children { get; set; } = new List<IAnswer>();
        public string Text => Body?.ToString();

        public InformativeAnswer(object response)
        {
            Body = response;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}";
        }

        public InformativeAnswer With(IInformativeAnswer childAnswer)
        {
            Children.Add(childAnswer);

            return this;
        }
    }
}