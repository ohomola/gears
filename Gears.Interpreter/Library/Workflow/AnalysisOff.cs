using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    public class AnalysisOff : Keyword
    {
        public override object DoRun()
        {
            Interpreter.IsAnalysis = false;

            return new DataDescriptionAnswer("Analysis OFF");
        }
    }
}