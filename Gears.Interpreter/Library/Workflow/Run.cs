using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    [UserDescription("run (N)\t\t-\t run one or N (if specified) steps")]
    public class Run : Keyword
    {
        public int Count { get; set; }

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Executes selected number of steps of your scenario. Use this if you want to run your plan up to given step.

#### Console usages
    run 4
";
        }

        public override object DoRun()
        {
            List<IAnswer> answers = new List<IAnswer>();

            var remainingSteps = Interpreter.Plan.Count()- Interpreter.Iterator.Index;
            int count = Math.Min(remainingSteps, Count);
            for (int i = 0; i < count; i++)
            {
                ConsoleView.Render(ConsoleColor.DarkGray, $"Starting step {Interpreter.Iterator.Index + 1})\t{Interpreter.Iterator.Current}");

                var answer = Interpreter.Please(string.Empty);
                answers.Add(answer);

                ConsoleView.Render(ConsoleColor.DarkGray, $"Finished step {Interpreter.Iterator.Index })\t{Interpreter.Iterator.Previous}\n{Interpreter.Iterator.Previous.Status}");
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
                run.Count = int.Parse(param);
            }
            else
            {
                run.Count = 1;
            }

            return run;


        }
    }
}