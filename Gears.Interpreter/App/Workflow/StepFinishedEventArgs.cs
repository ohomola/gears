using Gears.Interpreter.Core;

namespace Gears.Interpreter.App.Workflow
{
    public class StepFinishedEventArgs
    {
        public IKeyword Keyword { get; set; }

        public StepFinishedEventArgs(IKeyword keyword)
        {
            Keyword = keyword;
        }
    }
}