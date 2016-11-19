using System;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Configuration;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using JetBrains.Annotations;

namespace Gears.Interpreter.Library.Reports
{
    public class CsvScenarioReport : IAutoRegistered, IApplicationEventHandler
    {
        private string _path;
        private int _filesCreated = 0;

        public CsvScenarioReport()
        {
            _path = ".\\Output\\ScenarioReport_csv{0}.csv";
        }

        public CsvScenarioReport([NotNull] string path)
        {
            _path = path;
        }

        public void Register(ApplicationLoop applicationLoop)
        {
            applicationLoop.ScenarioFinished += WriteLog;
        }

        private void WriteLog(object sender, ScenarioFinishedEventArgs e)
        {
            var path = string.Format(_path, DateTime.Now.ToString("s").Replace(":", "_") + _filesCreated++);

            Console.Out.WriteColoredLine(ConsoleColor.Gray, $"CSV Scenario Report  was saved to file \'{path}\'.");

            new FileObjectAccess(path, ServiceLocator.Instance.Resolve<ITypeRegistry>()).AddRange(e.Keywords);
        }
    }
}