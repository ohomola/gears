using Gears.Interpreter.Core;

namespace Gears.Interpreter.App.Workflow
{
    public class StepEventArgs
    {
        public IKeyword Keyword { get; set; }

        public StepEventArgs(IKeyword keyword)
        {
            Keyword = keyword;
        }
    }
}