using System;
using System.Linq;
using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    [UserDescription("goto (N)\t-\t goes to Nth keyword")]
    public class GoTo : Keyword
    {
        public int Index { get; set; }

        public override object DoRun()
        {
            var moved = Math.Max(0, Math.Min(Interpreter.Plan.Count(), Index));
            Interpreter.Iterator.Index = moved;
            return new SuccessAnswer($"Moved to {Interpreter.Iterator.Index+1}.");
        }

        public override IKeyword FromString(string textInstruction)
        {
            var goTo = new GoTo();

            var param = ExtractSingleParameterFromTextInstruction(textInstruction);

            if (!string.IsNullOrEmpty(param))
            {
                goTo.Index = int.Parse(param)-1;
            }
            else
            {
                throw new ArgumentException("Must specify index number");
            }

            return goTo;
        }
    }
}