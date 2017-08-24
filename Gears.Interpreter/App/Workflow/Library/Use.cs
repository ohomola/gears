using System;
using System.Linq;
using Gears.Interpreter.App.Configuration;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [UserDescription("use (X)\t-\t adds Keyword X to context. (e.g. 'use junitscenarioreport' to register ScenarioReport.")]
    public class Use : Keyword
    {
        public Use(string what)
        {
            What = what;
        }

        public Use()
        {
        }


        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Adds object of specified Type to Context. Use this keyword to turn on a configuration.

#### Console usages
    use SkipAssertions
    run
    SkipAssertions off

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        public override object DoRun()
        {
            object instance = null;

            if (Interpreter.Language.HasKeywordFor(What.ToLower()))
            {
                instance = Interpreter.Language.ResolveKeyword(What.ToLower());
            }
            else
            {
                var type = TypeRegistry.GetAll(false).FirstOrDefault(x => x.Name.ToLower() == What.ToLower());

                if (type == null)
                {
                    throw new ArgumentException($"{What} is not recognized.");
                }
                instance = Activator.CreateInstance(type);
            }

            Data.Add(instance);

            (instance as IAutoRegistered)?.Register(Interpreter);

            if (instance is IKeyword)
            {
                Interpreter.AddToPlan(instance as IKeyword);
            }

            return new SuccessAnswer($"Added {instance} to Data Context.");
        }

        public override IKeyword FromString(string textInstruction)
        {
            var goTo = new Use();

            var param = ExtractSingleParameterFromTextInstruction(textInstruction);

            goTo.What = param;

            return goTo;
        }

        public string What { get; set; }

        public override string ToString()
        {
            return $"Use '{What}'";
        }
    }
}