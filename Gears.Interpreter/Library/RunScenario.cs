using System;
using System.Collections.Generic;
using System.Linq;

namespace Gears.Interpreter.Library
{
    public class RunScenario : Keyword
    {
        public RunScenario(params Keyword[] keywords)
        {
            Keywords = keywords.ToList();
        }

        public RunScenario(string keywords)
        {
            throw new NotImplementedException();
        }

        public List<Keyword> Keywords { get; set; }

        public override object Run()
        {
            return null;
        }
    }
}