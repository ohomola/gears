using System.Linq;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [HelpDescription("back (N)\t-\t back one or N (if specified) steps")]
    public class Back : Keyword
    {
        public int Count { get; set; } = 1;

        public override object DoRun()
        {
            if (Interpreter.Iterator.Index == 0 && Interpreter.Plan.OfType<StepOut>().Any())
            {
                Interpreter.Plan.OfType<StepOut>().First().Execute();
                return new InformativeAnswer("Returning back to master scenario");
            }

            var moved = Interpreter.Iterator.MoveBack(Count);

            return new SuccessAnswer($"Backed {moved} step{(moved == 1 ? "" : "s")}.");
        }

        public override string Instruction
        {
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Count = int.Parse(value);
                }
            }
        }

        #region Documentation
        public override string CreateDocumentationMarkDown()
        {
            return
                $@"{base.CreateDocumentationMarkDown()}
Moves selected keyword one step back. You can also add a number parameter to specify the numer of steps.
#### Console usages
    back
    back 4";
        }
        #endregion
    }
}