using System;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Configuration;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Data;
using JetBrains.Annotations;

namespace Gears.Interpreter.Library.Reports
{
    public class JUnitSuiteReport : IAutoRegistered, IApplicationEventHandler
    {
        private string _path;
        private int _filesCreated = 0;

        public JUnitSuiteReport()
        {
            _path = ".\\Output\\TestSession_JUnit{0}.xml";
        }

        public JUnitSuiteReport([NotNull] string path)
        {
            _path = path;
        }

        public void Register(ApplicationLoop applicationLoop)
        {
            applicationLoop.SuiteFinished += CreateNewFile;
        }

        private void CreateNewFile(object sender, ScenarioFinishedEventArgs e)
        {
            var path = string.Format(_path, DateTime.Now.ToString("s").Replace(":", "_") + "_"+ ++_filesCreated);

            Console.Out.WriteColoredLine(ConsoleColor.Gray, $"JUnit Suite Report  was saved to file \'{path}\'.");

            new FileObjectAccess(path).AddRange(e.Keywords);
        }
    }
}