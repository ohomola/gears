using System.Collections.Generic;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications
{
    public class ScenarioFinishedEventArgs
    {
        public List<Keyword> Keywords { get; set; }

        public ScenarioFinishedEventArgs(List<Keyword> keywords)
        {
            Keywords = keywords;
        }
    }
}