using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Data;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    public class Status : Keyword
    {
        public override object DoRun()
        {
            return new StatusAnswer(Interpreter.Plan, Interpreter.Iterator.Index, Interpreter.Data);
        }
    }

    [NotLogged]
    public class Analysis : Keyword
    {
        public override object DoRun()
        {
            InformativeAnswer successAnswer = new InformativeAnswer("\n------- Interpreter details -------\n");

            if (Interpreter.Data.DataAccesses.OfType<FileObjectAccess>().Any())
            {
                successAnswer.Children.Add(new SuccessAnswer($"Files registered : {Interpreter.Data.DataAccesses.OfType<FileObjectAccess>().Count()}\n"));
                foreach (var foa in Interpreter.Data.DataAccesses.OfType<FileObjectAccess>())
                {
                    successAnswer.Children.Add(new SuccessAnswer($"\nData Access {foa}"));
                }
            }

            return successAnswer;
        }
    }
}