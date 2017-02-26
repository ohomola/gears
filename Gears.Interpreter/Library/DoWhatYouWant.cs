using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    public class DoWhatYouWant : Keyword, IProtected
    {
        public override object DoRun()
        {
            return "Do Nothing";
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}