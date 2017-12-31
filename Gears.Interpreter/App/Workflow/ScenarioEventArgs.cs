using System.Collections.Generic;
using Gears.Interpreter.Core;

namespace Gears.Interpreter.App.Workflow
{
    public class ScenarioEventArgs
    {
        public List<IKeyword> Keywords { get; set; }
        public string Name { get; set; }

        public ScenarioEventArgs(List<IKeyword> keywords, string name)
        {
            Keywords = keywords;
            Name = name;
        }
    }
}