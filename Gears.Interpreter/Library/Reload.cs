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

    [NotLogged]
    [UserDescription("forget \t\t-\t erases all remembered text values from memory")]
    public class Forget : Keyword
    {
        public override object DoRun()
        {
            var items = Interpreter.Data.GetAll<RememberedText>();
            Interpreter.Data.RemoveAll<RememberedText>();
            
            return new SuccessAnswer($"Forgot {items.Count()} items.");
        }
    }
}