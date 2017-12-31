using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    public class Status : Keyword, IProtected
    {
        public override object DoRun()
        {
            return new StatusAnswer(Interpreter.Plan, Interpreter.Iterator.Index, Interpreter.Data, Interpreter);
        }
    }
}