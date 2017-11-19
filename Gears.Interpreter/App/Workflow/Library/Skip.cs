using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [HelpDescription("skip (N) \t-\t skips one or N (if specified) steps")]
    public class Skip : Keyword
    {
        public int Count { get; set;}


        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Skips selected number of steps of your scenario.

#### Console usages
    skip 4
";
        }

        public override object DoRun()
        {
            var moved = Interpreter.Iterator.MoveNext(Count);

            return new SuccessAnswer($"Skipped {moved} step{(moved==1?"":"s")}.");
        }

        public override IKeyword FromString(string textInstruction)
        {
            var skip = new Skip();

            var param = ExtractSingleParameterFromTextInstruction(textInstruction);

            if (!string.IsNullOrEmpty(param))
            {
                skip.Count = int.Parse(param);
            }
            else
            {
                skip.Count = 1;
            }

            return skip;
        }
    }
}