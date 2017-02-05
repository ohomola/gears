using System;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Configuration;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;
using Microsoft.Office.Interop.Excel;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    [UserDescription("take (X)\t-\t adds Keyword X to context. (e.g. 'Take junitscenarioreport' to register ScenarioReport.")]
    public class Take : Keyword
    {
        [Wire]
        public ITypeRegistry TypeRegistry { get; set; }

        public override object DoRun()
        {
            object instance = null;

            if (Interpreter.Language.HasKeywordFor(What.ToLower()))
            {
                instance = Interpreter.Language.ResolveKeyword(What.ToLower());
            }
            else
            {
                var type = TypeRegistry.GetDTOTypes(false).FirstOrDefault(x => x.Name.ToLower() == What.ToLower());

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
            var goTo = new Take();

            var param = ExtractSingleParameterFromTextInstruction(textInstruction);

            goTo.What = param;

            return goTo;
        }

        public string What { get; set; }
    }
}