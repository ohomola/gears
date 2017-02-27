using System;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Data;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [NotLogged]
    [UserDescription("reload \t\t-\t re-reads all input files")]
    public class Reload : Keyword
    {

        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Reloads the scenario. Use this along with a shared xls document to build automated tests on-the-fly.
#### Console usage
    reload";
        }

        public override object DoRun()
        {
            foreach (var dataAccess1 in Interpreter.Data.DataAccesses.OfType<FileObjectAccess>())
            {
                dataAccess1.ForceReload();
            }

            Interpreter.Plan = Interpreter.Data.GetAll<Keyword>().ToList();

            if (Interpreter.Iterator.Index >= Interpreter.Plan.ToList().Count)
            {
                Interpreter.Iterator.Index = 0;
            }

            return new SuccessAnswer("Reload completed.");
        }
    }
}