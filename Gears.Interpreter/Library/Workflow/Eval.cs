using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    [UserDescription("eval \t\t-\t  if current keyword has expressions, it evaluates them and shows their result")]
    public class Eval : Keyword
    {
        public int Index { get; set; }

        public override object DoRun()
        {
            (Interpreter.Iterator.Current as Keyword).Hydrate();

            return new SuccessAnswer($"Successfuly evaluated keyword:").With(new SuccessAnswer(
                $"{Interpreter.Iterator.Current}"));
        }
    }
}