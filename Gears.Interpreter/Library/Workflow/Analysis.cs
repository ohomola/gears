using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    public class Analysis : Keyword
    {
        public override object DoRun()
        {
            Interpreter.IsAnalysis = true;

            return new DataDescriptionAnswer("Analysis ON");
        }
    }
}