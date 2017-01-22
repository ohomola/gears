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
        public override object DoRun()
        {
            Interpreter.Plan = new List<IKeyword>();

            return new SuccessAnswer("Scenario closed.");
        }
    }
}