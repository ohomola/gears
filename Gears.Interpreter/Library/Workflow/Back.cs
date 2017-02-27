using Gears.Interpreter.Applications;
using Gears.Interpreter.Library.Documentations;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    [UserDescription("back (N)\t-\t back one or N (if specified) steps")]
    public class Back : Keyword
    {
        public int Count { get; set; }

        public override string CreateDocumentationMarkDown()
        {
            return
                $@"{base.CreateDocumentationMarkDown()}
Moves selected keyword one step back. You can also add a number parameter to specify the numer of steps.
#### Console usages
    back
    back 4";
        }

        public override object DoRun()
        {
            var moved = Interpreter.Iterator.MoveBack(Count);

            return new SuccessAnswer($"Backed {moved} step{(moved == 1 ? "" : "s")}.");
        }

        public override IKeyword FromString(string textInstruction)
        {
            var back = new Back();

            var param = ExtractSingleParameterFromTextInstruction(textInstruction);

            if (!string.IsNullOrEmpty(param))
            {
                back.Count = int.Parse(param);
            }
            else
            {
                back.Count = 1;
            }

            return back;


        }
    }
}