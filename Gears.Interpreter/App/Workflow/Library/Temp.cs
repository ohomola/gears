using System.Diagnostics;
using System.Linq;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    public class Temp : Keyword, IHasTechnique, IProtected
    {
        public const string LastScenarioTempFilePath = "GearsLastScenarioTemp";

        public override object DoRun()
        {
            var foa = new TempFileObjectAccess(LastScenarioTempFilePath + ".csv", ServiceLocator.Instance.Resolve<ITypeRegistry>());

            if (Technique == Technique.HighlightOnly)
            {
                Process.Start("explorer.exe", foa.Path);
                return new InformativeAnswer($"Opening folder {foa.Path}");
            }

            Interpreter.Data.Include(foa);

            Interpreter.Plan = Interpreter.Plan.Union(foa.GetAll<IKeyword>().ToList()).ToList();

            return true;
        }

        public Technique Technique { get; set; }
    }
}