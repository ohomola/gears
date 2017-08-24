using System.Collections.Generic;
using Gears.Interpreter.App;
using Gears.Interpreter.Core.Data;

namespace Gears.Interpreter.Core.Interpretation
{
    public class StatusAnswer : IAnswer
    {
        public StatusAnswer(IEnumerable<IKeyword> keywords, int index, IDataContext data, IInterpreter interpreter)
        {
            Keywords = keywords;
            Index = index;
            Data = data;
            Interpreter = interpreter;
        }

        public IEnumerable<IKeyword> Keywords { get; set; }
        public int Index { get; set; }
        public IDataContext Data { get; set; }
        public IInterpreter Interpreter { get; set; }
        public object Body { get; }
        public List<IAnswer> Children { get; }
        public string Text { get; }
    }
}