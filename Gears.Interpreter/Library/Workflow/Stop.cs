using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    [UserDescription("stop \t\t-\t stops execution, saves results and exits application.")]
    public class Stop : Keyword
    {
        public override object DoRun()
        {
            Interpreter.IsAlive = false;

            //TODO : the result should be processed by triage, success/failure evaluation must not be done by reports
            WriteResults(Interpreter.IsRunningSuite, Interpreter.Plan);

            SharedObjectDataAccess.Instance = new Lazy<SharedObjectDataAccess>();

            if (Interpreter.Plan.Any(x => (x).Status == KeywordStatus.Error.ToString()))
            {
                return new ResultAnswer(Program.ScenarioFailureStatusCode);
            }

            WriteTempFile(Interpreter.Plan.Any() ? Interpreter.Plan : Interpreter.ExecutionHistory.Where(IsLogged));

            return new ResultAnswer(Program.OkStatusCode);
        }

        private void WriteTempFile(IEnumerable<IKeyword> keywords)
        {
            string fileName = System.IO.Path.GetTempPath() + Applications.Interpreter.LastScenarioTempFilePath + ".csv";

            File.Delete(fileName);

            var foa = new FileObjectAccess(fileName, ServiceLocator.Instance.Resolve<ITypeRegistry>());
            
            foa.WriteObjects(keywords);

            var exists = File.Exists(fileName);
        }


        private void WriteResults(bool isRunningSuite, IEnumerable<IKeyword> keywords)
        {
            if (!isRunningSuite)
            {
                Interpreter.OnScenarioFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
            }
            else
            {
                Interpreter.OnSuiteFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
            }
        }

    }
}
