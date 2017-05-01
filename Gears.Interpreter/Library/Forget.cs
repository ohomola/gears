using System.Linq;
using Castle.Core.Internal;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [NotLogged]
    [UserDescription("forget (x)\t\t-\t erases remembered variable (x) from memory. If x not specified, wipes all variables from memory.")]
    public class Forget : Keyword
    {
        public Forget(string what)
        {
            What = what;
        }

        public string What { get; set; }

        public Forget()
        {
        }

        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Forgets remembered values, which were previously added by Remember keyword. If specified without parameter, removes all remembered values.
#### Console usage
    forget
    remember url http://www.hello.world.com
    forget url
"
    
    ;
        }

        public override object DoRun()
        {
            var items = Interpreter.Data.GetAll<RememberedText>().ToList();

            if (What.IsNullOrEmpty())
            {
                Interpreter.Data.RemoveAll<RememberedText>();
            }
            else
            {
                var item = items.FirstOrDefault(x => x.Variable.ToLower() == What.ToLower());

                if (item == null)
                {
                    return new WarningAnswer("Variable {What} is not known. Cannot remove.");
                }
                Interpreter.Data.Remove(item);
            }
            
            return new SuccessAnswer($"Forgot {items.Count()} item{(items.Count!=1?"s":"")}.");
        }

        public override IKeyword FromString(string textInstruction)
        {
            return new Forget() { What = ExtractSingleParameterFromTextInstruction(textInstruction) };
        }
    }
}