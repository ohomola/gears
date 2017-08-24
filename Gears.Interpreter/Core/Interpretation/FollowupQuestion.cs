using System.Collections.Generic;

namespace Gears.Interpreter.Core.Interpretation
{
    public abstract class FollowupQuestion : IFollowupQuestion
    {
        public abstract object Body { get; }
        public List<IAnswer> Children { get; set; } = new List<IAnswer>();
        public string Text => Body?.ToString();
        public IEnumerable<IKeyword> Options { get; set; }
    }
}