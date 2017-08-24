using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [UserDescription("run (N)\t\t-\t run one or N (if specified) steps")]
    public class Run : Keyword
    {
        [Wire]
        [XmlIgnore]
        public virtual IInterpreter Interpreter2 { get; set; }

        public int Count { get; set; }

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Executes selected number of steps of your scenario. Use this if you want to run your plan up to given step. Use 'all' instead of a number to execute from current step to end of scenario.

#### Console usages
    run 4
    run all
";
        }

        public override object DoRun()
        {
            List<IAnswer> answers = new List<IAnswer>();

            var remainingSteps = Interpreter2.Plan.Count()- Interpreter.Iterator.Index;
            int count = Math.Min(remainingSteps, Count);
            for (int i = 0; i < count; i++)
            {
                ConsoleView.Render(ConsoleColor.DarkGray, $"Starting step {Interpreter.Iterator.Index + 1})\t{Interpreter.Iterator.Current}");

                var answer = Interpreter2.Please(string.Empty);
                answers.Add(answer);

                //ConsoleView.Render(ConsoleColor.DarkGray, $"Finished step {Interpreter.Iterator.Index })\t{Interpreter.Iterator.Previous}\n{Interpreter.Iterator.Previous.Status}");
                ConsoleView.Render(Interpreter.Please("status"));

                if (UserInteropAdapter.IsKeyDown(Keys.Escape))
                {
                    Interpreter.IsDebugMode = true;
                    return "Escape pressed. Interrupting...";
                }
            }

            if (answers.Any(x => x is INegativeAnswer))
            {
                var baseAnswer = new ExceptionAnswer($"Ran {count} step{(count == 1 ? "" : "s")}.");
                baseAnswer.Children = answers;
                return baseAnswer;
            }
            else
            {
                var baseAnswer = new SuccessAnswer($"Ran {count} step{(count == 1 ? "" : "s")}.");
                baseAnswer.Children = answers;
                return baseAnswer;
            }
        }

        public override IKeyword FromString(string textInstruction)
        {
            var run = new Run();

            var param = ExtractSingleParameterFromTextInstruction(textInstruction);

            if (!string.IsNullOrEmpty(param))
            {
                if (param.ToLower() == "all")
                {
                    run.Count = int.MaxValue;
                }
                else
                {
                    run.Count = int.Parse(param);
                }
            }
            else
            {
                run.Count = 1;
            }

            return run;
        }
    }
}