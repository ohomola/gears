using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    public class LastPlan : Keyword, IHasTechnique, IProtected
    {
        public override object DoRun()
        {
            var foa = new TempFileObjectAccess(Applications.Interpreter.LastScenarioTempFilePath + ".csv", ServiceLocator.Instance.Resolve<ITypeRegistry>());

            if (Technique == Technique.HighlightOnly)
            {
                Process.Start("explorer.exe", foa.Path);
                return new InformativeAnswer($"Opening folder {foa.Path}");
            }

            Interpreter.Plan = foa.ReadAllObjects().OfType<Keyword>();

            return true;
        }

        public Technique Technique { get; set; }
    }
}