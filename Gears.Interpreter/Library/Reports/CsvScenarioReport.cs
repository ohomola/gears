using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gears.Interpreter.App;
using Gears.Interpreter.App.Workflow;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Library.UI;
using JetBrains.Annotations;

namespace Gears.Interpreter.Library.Reports
{
    public class CsvScenarioReport : Keyword, IApplicationEventHandler, IHasTechnique
    {
        private string _path;
        private static int _filesCreated = 0;
        private readonly string _pathTemplate;

        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Creates output file containing results of the scenario in table format.
> Note: To add this to any scenario, you can also use a command-line argument when executing Gears Interpreter -{nameof(CsvScenarioReport)}

#### Console usage
    CsvScenarioReport
    show CsvScenarioReport";
        }

        public CsvScenarioReport()
        {
            _pathTemplate = ".\\Output\\ScenarioReport_csv{0}.csv";
        }

        public CsvScenarioReport([NotNull] string path)
        {
            _pathTemplate = path;
        }

        public void Register(IInterpreter interpreter)
        {
            interpreter.ScenarioFinished += WriteLog;
        }

        private void WriteLog(object sender, ScenarioFinishedEventArgs e)
        {
            _path = string.Format(_pathTemplate, DateTime.Now.ToString("s").Replace(":", "_") + _filesCreated++);

            Directory.CreateDirectory(Path.GetDirectoryName(_path));

            Console.Out.WriteColoredLine(ConsoleColor.Gray, $"{e.Name} report was saved to CSV file \'{_path}\'.");

            new FileObjectAccess(_path, ServiceLocator.Instance.Resolve<ITypeRegistry>()).AddRange(e.Keywords);

            if (Technique == Technique.HighlightOnly)
            {
                Process.Start("explorer.exe", _path);
            }
        }

        public override object DoRun()
        {
            WriteLog(null, new ScenarioFinishedEventArgs(Interpreter.GetLog().ToList(), "Master scenario"));

            return new SuccessAnswer($"Saved report to {_path}");
        }

        public virtual Technique Technique { get; set; }

        public override string ToString()
        {
            return "Save scenario report as CSV.";
        }
    }
}