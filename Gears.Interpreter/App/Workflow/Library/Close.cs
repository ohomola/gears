using System.Collections.Generic;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    public class Close : Keyword
    {
        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Closes currently open Scenario file an empties the Keyword Execution Plan.
#### Console usage
    close";
        }

        public override object DoRun()
        {
            Interpreter.Plan = new List<IKeyword>();
            Interpreter.Iterator.Index = 0;

            return new SuccessAnswer("Scenario closed.");
        }
    }
}