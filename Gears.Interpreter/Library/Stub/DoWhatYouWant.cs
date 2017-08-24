using Gears.Interpreter.Core;

namespace Gears.Interpreter.Library.Stub
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