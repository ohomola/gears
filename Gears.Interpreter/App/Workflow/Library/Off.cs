using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Core;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [HelpDescription("(X) off \t-\t remove Keyword X from context.")]
    public class Off : Keyword
    {
        public string What { get; set; }

        public Off(string what)
        {
            What = what;
        }

        public Off()
        {
        }

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Removes all objects of specified Type from Context. Use this keyword to turn off a configuration previousl turned-on by Use keyword.

#### Console usages
    use SkipAssertions
    run
    off SkipAssertions

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        public override string ToString()
        {
            return $"Off '{What}'";
        }

        public override object DoRun()
        {
            var type = TypeRegistry.Types.FirstOrDefault(x => x.Name.ToLower() == What.ToLower());

            if (type == null)
            {
                throw new ArgumentException($"{What} is not recognized.");
            }

            Data.RemoveAll(type);

            return null;
        }

        public override void FromString(string textInstruction)
        {
            this.What = textInstruction;
        }
    }
}