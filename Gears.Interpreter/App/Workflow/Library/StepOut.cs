using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Core;

namespace Gears.Interpreter.App.Workflow.Library
{
    public class StepOut : Keyword
    {
        public static IEnumerable<IKeyword> InterpreterPlan { get; set; }
        public static int IteratorIndex { get; set; }
        public static string FileName { get; set; }

        public StepOut()
        {
        }

        public StepOut(IEnumerable<IKeyword> interpreterPlan, int iteratorIndex, string fileName)
        {
            InterpreterPlan = interpreterPlan;
            IteratorIndex = iteratorIndex;
            FileName = fileName;
        }

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Steps out of a scenario. Used only when currently inside a scenario (via Show RunScenario).

#### Console usages
    stepout
";
        }

        public override object DoRun()
        {
            Interpreter.OnScenarioFinished(new ScenarioFinishedEventArgs(Interpreter.Plan.ToList(), FileName));

            var isScenarioFailed = Interpreter.Plan.Any(x => x.Status.ToString() == KeywordStatus.Error.ToString());

            Interpreter.Plan = InterpreterPlan;

            Interpreter.Iterator.Index = IteratorIndex;

            if (isScenarioFailed)
            {
                Interpreter.Iterator.Current.Status = KeywordStatus.Error.ToString();
            }

            Interpreter.Iterator.MoveNext();

            return true;
        }

        public override string ToString()
        {
            return $"Return to Master Scenario";
        }
    }
}