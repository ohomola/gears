using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    public class AnalysisOff : Keyword
    {
        public override string CreateDocumentationMarkDown()
        {
            return $@"{base.CreateDocumentationMarkDown()}Turns off troubleshooting mode. See [Analysis](#analysis).
#### Console usages
    analysisoff";
        }


        public override object DoRun()
        {
            Interpreter.IsAnalysis = false;

            return new DataDescriptionAnswer("Analysis OFF");
        }
    }
}