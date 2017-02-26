using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library.Workflow
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