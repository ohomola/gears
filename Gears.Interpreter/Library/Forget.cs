using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [NotLogged]
    [UserDescription("forget \t\t-\t erases all remembered text values from memory")]
    public class Forget : Keyword
    {
        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Forgets all remembered values, which were previously added by Remember keyword.
#### Console usage
    forget";
        }

        public override object DoRun()
        {
            var items = Interpreter.Data.GetAll<RememberedText>();
            Interpreter.Data.RemoveAll<RememberedText>();
            
            return new SuccessAnswer($"Forgot {items.Count()} items.");
        }
    }
}