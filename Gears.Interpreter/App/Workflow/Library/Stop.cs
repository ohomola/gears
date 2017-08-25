using System;
using System.Linq;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [UserDescription("stop \t\t-\t stops execution, saves results and exits application.")]
    public class Stop : Keyword
    {

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Stops the scenario execution and closes the application.

#### Console usages
    stop
";
        }


        public override object DoRun()
        {
            Interpreter.IsAlive = false;

            //TODO : the result should be processed by triage, success/failure evaluation must not be done by reports
            //WriteResults(Interpreter.IsRunningSuite, Interpreter.Plan);

            SharedObjectDataAccess.Instance = new Lazy<SharedObjectDataAccess>();

            if (Interpreter.Plan.Any(x => (x).Status == KeywordStatus.Error.ToString()))
            {
                return new ResultAnswer(Program.ScenarioFailureStatusCode);
            }

            var objectsToBeWritten = Interpreter.GetLog();

            Selenium.Dispose();

            //new TempFileObjectAccess(Applications.Interpreter.LastScenarioTempFilePath + ".csv", ServiceLocator.Instance.Resolve<ITypeRegistry>())
            //    .Write(objectsToBeWritten);

            return new ResultAnswer(Program.OkStatusCode);
        }


        //private void WriteResults(bool isRunningSuite, IEnumerable<IKeyword> keywords)
        //{
        //    if (!isRunningSuite)
        //    {
        //        Interpreter.OnScenarioFinished(new ScenarioFinishedEventArgs(keywords.ToList(), "Master scenario"));
        //    }
        //    else
        //    {
        //        Interpreter.OnSuiteFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
        //    }
        //}

    }
}
