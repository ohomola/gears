using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library.Workflow
{
    public class LastPlan : Keyword
    {
        public override object DoRun()
        {
            Interpreter.Plan = ReadTempFile();

            return true;
        }


        public static IEnumerable<Keyword> ReadTempFile()
        {
            string fileName = System.IO.Path.GetTempPath() + Applications.Interpreter.LastScenarioTempFilePath + ".csv";

            var foa = new FileObjectAccess(fileName, ServiceLocator.Instance.Resolve<ITypeRegistry>());
            return foa.ReadAllObjects().OfType<Keyword>();
        }
    }
}