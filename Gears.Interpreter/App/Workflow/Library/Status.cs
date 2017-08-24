using System.Linq;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    public class Status : Keyword, IProtected
    {
        public override object DoRun()
        {
            return new StatusAnswer(Interpreter.Plan, Interpreter.Iterator.Index, Interpreter.Data, Interpreter);
        }
    }

    [NotLogged]
    public class AnalyseData : Keyword, IProtected
    {
        public override object DoRun()
        {
            InformativeAnswer successAnswer = new DataDescriptionAnswer(
            $"\n{ConsoleView.HorizontalLine}\n Data Analysis\n{ConsoleView.HorizontalLine}\n");

            if (Interpreter.Data.DataAccesses.OfType<FileObjectAccess>().Any())
            {
                successAnswer.Children.Add(new ExternalMessageAnswer($" Registered Files ({Interpreter.Data.DataAccesses.OfType<FileObjectAccess>().Count()}):\n"));
                foreach (var foa in Interpreter.Data.DataAccesses.OfType<FileObjectAccess>())
                {
                    successAnswer.Children.Add(new ExternalMessageAnswer($"\n\t - {foa}"));
                }
            }

            successAnswer.Children.Add(new DataDescriptionAnswer($"\n  Data:"));

            foreach (var foa in Interpreter.Data.GetAll())
            {
                successAnswer.Children.Add(new DataDescriptionAnswer($"\n\t<{foa.GetType().Name}>\t\t{foa}"));
            }

            return successAnswer;
        }

    }
}