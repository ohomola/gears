using System.Collections.Generic;

namespace Gears.Interpreter.Core.Interpretation
{
    public interface IFollowupQuestion : IAnswer
    {
        IEnumerable<IKeyword> Options { get; set; }
    }
}