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

        public override object DoRun()
        {
            List<IAnswer> answers = new List<IAnswer>();

            var remainingSteps = Interpreter.Plan.Count()- Interpreter.Iterator.Index;
            int count = Math.Min(remainingSteps, Count);
            for (int i = 0; i < count; i++)
            {
                answers.Add(Interpreter.Please(string.Empty));
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