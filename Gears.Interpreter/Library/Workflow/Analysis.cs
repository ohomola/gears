using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    public class Analysis : Keyword
    {
        public override string CreateDocumentationMarkDown()
        {
            return
                $@"{base.CreateDocumentationMarkDown()}Turns on troubleshooting mode. During troubleshooting mode you will see detailed information in console about executed commands. Additionally each UI action will be highlighted for a short period of time on the screen before any action taken. Use AnalysisOff to turn off the troubleshooting moode.
#### Console usages
    analysis";
        }

        public override object DoRun()
        {
            Interpreter.IsAnalysis = true;

            return new DataDescriptionAnswer("Analysis ON");
        }
    }
}