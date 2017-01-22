using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
{
    public class EndOfScenario : Keyword
    {
        public override object DoRun()
        {
            return new SuccessAnswer("Scenario finished");
        }
    }
}