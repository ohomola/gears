using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Configuration;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using JetBrains.Annotations;

namespace Gears.Interpreter.Library.Reports
{
    public class JUnitScenarioReport : Keyword, IApplicationEventHandler, IHasTechnique
    {
        private string _path;
        private static int _filesCreated = 0;
        private readonly string _pathTemplate;

        public JUnitScenarioReport()
        {
            _pathTemplate = ".\\Output\\ScenarioReport_JUnit{0}.xml";
        }

        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Creates output file containing results of the scenario in JUnit-compatible XML format.
> Note: To add this to any scenario, you can also use a command-line argument when executing Gears Interpreter -{nameof(JUnitScenarioReport)}

#### Console usage
    JUnitScenarioReport
    show JUnitScenarioReport";
        }

        public JUnitScenarioReport([NotNull] string path)
        {
            _pathTemplate = path;
        }

        public void Register(IInterpreter applicationLoop)
        {
            applicationLoop.ScenarioFinished += CreateNewFile;
        }

        private void CreateNewFile(object sender, ScenarioFinishedEventArgs e)
        {
            _path = string.Format(_pathTemplate, DateTime.Now.ToString("s").Replace(":", "_") + "_"+ ++_filesCreated);

            Directory.CreateDirectory(Path.GetDirectoryName(_path));

            Console.Out.WriteColoredLine(ConsoleColor.Gray, $"{e.Name} report was saved to JUNit file \'{_path}\'.");

            new FileObjectAccess(_path, ServiceLocator.Instance.Resolve<ITypeRegistry>()).AddRange(e.Keywords);

            if (Technique == Technique.HighlightOnly)
            {
                Process.Start("explorer.exe", _path);
            }
        }

        public override object DoRun()
        {
            CreateNewFile(null, new ScenarioFinishedEventArgs(Interpreter.GetLog().ToList(), "Master scenario"));

            return new SuccessAnswer($"Saved report to {_path}");
        }

        public virtual Technique Technique { get; set; }

        public override string ToString()
        {
            return "Save scenario report as JUnit.";
        }
    }
}