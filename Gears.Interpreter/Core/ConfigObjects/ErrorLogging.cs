using Gears.Interpreter.App.Workflow.Library;
using Gears.Interpreter.Core.Library;

namespace Gears.Interpreter.Core.ConfigObjects
{
    public class ErrorLogging : IHaveDocumentation, IConfig
    {
        public string CreateDocumentationMarkDown()
        {
            return $@"## {this.GetType().Name}

Configuration Type used to automatically take screenshots on error or other error messages during execution.

> Note: This is a configuration, not an active keyword. It changes behaviour of your scenario by merely existing in your data context. Use 'Use' keyword to add it to your context and 'Off' keyword to take it away.

#### Scenario usages
| Discriminator | What |
| ------------- | ---- |
| Use           | ErrorLogging |
| IsVisible         | Something which is broken |
| Off           | ErrorLogging|

#### Console usages
    use ErrorLogging
    run
    ErrorLogging off
";
        }

        public string CreateDocumentationTypeName()
        {
            return nameof(ErrorLogging);
        }

        public override string ToString()
        {
            return "ErrorLogging (screenshots automatically taken on error)";
        }

        public void Log(Keyword keyword, string s)
        {
            if (ShouldLog(keyword))
            {
                new SaveScreenshot(s).Execute();
            }
        }

        private bool ShouldLog(Keyword keyword)
        {
            if (keyword is RunScenario)
            {
                return false;
            }

            return true;
        }
    }
}