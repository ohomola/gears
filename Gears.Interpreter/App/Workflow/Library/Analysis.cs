using Gears.Interpreter.App.Configuration;
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    public class Analysis : IHaveDocumentation, IConfig, IAutoRegistered
    {
        public string CreateDocumentationMarkDown()
        {
            return
                $@"
## Analysis 

Turns on troubleshooting mode. During troubleshooting mode you will see detailed information in console about executed commands. Additionally each UI action will be highlighted for a short period of time on the screen before any action taken. Use AnalysisOff to turn off the troubleshooting moode.
#### Console usages
    analysis";
        }

        public string CreateDocumentationTypeName()
        {
            return "Analysis";
        }

        public override string ToString()
        {
            return "Analysis (additional information is provided to the user for troubleshooting)";
        }

        public void Register(IInterpreter interpreter)
        {
            ServiceLocator.Instance.Resolve<IBrowserOverlay>().Show("Analysis");
        }
    }
}