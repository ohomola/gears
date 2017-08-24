using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [UserDescription("eval \t\t-\t  if current keyword has expressions, it evaluates them and shows their result")]
    public class Eval : Keyword
    {

        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"When your selected keyword contains unresolved expression (e.g. reference to a memorized value), this keyword will explicitly resolve that expression without executing the keyword.
Note that keywords are resolved automatically when run, so you don't need this keyword when using expressions. This keyword is intended for debugging your scenario.
#### Console usage
    eval";
        }


        public int Index { get; set; }

        public override object DoRun()
        {
            (Interpreter.Iterator.Current as Keyword).Hydrate();

            return new SuccessAnswer($"Successfuly evaluated keyword:").With(new SuccessAnswer(
                $"{Interpreter.Iterator.Current}"));
        }
    }
}