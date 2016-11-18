using System;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Configuration;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Data;
using JetBrains.Annotations;

namespace Gears.Interpreter.Library.Reports
{
    public class CsvReport : IAutoRegistered, IApplicationEventHandler
    {
        private string _path;
        private int _filesCreated = 0;

        public CsvReport()
        {
            _path = ".\\Output\\TestSession_csv{0}.csv";
        }

        public CsvReport([NotNull] string path)
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

            Console.Out.WriteColoredLine(ConsoleColor.Gray, $"CSV output was saved to file \'{path}\'.");

            new FileObjectAccess(path).AddRange(e.Keywords);
        }
    }
}