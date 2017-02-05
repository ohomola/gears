using System.Collections.Generic;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications
{
    public class ScenarioFinishedEventArgs
    {
        public List<IKeyword> Keywords { get; set; }
        public string Name { get; set; }

        public ScenarioFinishedEventArgs(List<IKeyword> keywords, string name)
        {
            Keywords = keywords;
            Name = name;
        }
    }
}