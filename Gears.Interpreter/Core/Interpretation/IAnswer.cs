using System.Collections.Generic;

namespace Gears.Interpreter.Core.Interpretation
{
    public interface IAnswer
    {
        object Body { get; }
        List<IAnswer> Children { get; }
        string Text { get; }
    }
}