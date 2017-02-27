using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
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