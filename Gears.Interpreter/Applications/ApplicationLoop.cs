#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears, a software automation and assistance framework.

Gears is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Gears is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Registrations;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications
{
    

    public interface IApplicationLoop
    {
        IApplicationLoop Start();

        List<Keyword> Keywords { get; set; }
    }

    public class ApplicationLoop : IApplicationLoop
    {
        private readonly IDependencyReloader _reloader;
        private readonly IDataContext _data;
        private readonly IConsoleDebugger _debugger;
        private int _index = -1;
        private const string LastScenarioTempFilePath = "GearsLastScenarioTemp";

        

        public List<Keyword> Keywords { get; set; } = new List<Keyword>();

        public List<Keyword> ExecutionHistory { get; set; } = new List<Keyword>();
        public bool IsAlive { get; set; }

        private bool IsRunningSuite
        {
            get { return Keywords.Any(x => x is RunScenario); }
        }

        public ApplicationLoop(IDependencyReloader reloader, IDataContext data, IConsoleDebugger debugger)
        {
            _reloader = reloader;
            _data = data;
            _debugger = debugger;
        }

        #region Events

        public event EventHandler<ScenarioFinishedEventArgs> ScenarioFinished;

        protected virtual void OnScenarioFinished(ScenarioFinishedEventArgs e)
        {
            ScenarioFinished?.Invoke(this, e);
        }

        public event EventHandler<ScenarioFinishedEventArgs> SuiteFinished;

        protected virtual void OnSuiteFinished(ScenarioFinishedEventArgs e)
        {
            SuiteFinished?.Invoke(this, e);
        }
        #endregion

        public IAnswer RunOnYourOwn()
        {
            while (IsAlive)
            {
                Please(()=>string.Empty);
            }

            return new InformativeAnswer(0);
        }

        public IAnswer Please(string command)
        {
            return Please(() => command);
        }

        public IAnswer Please(Func<string> input)
        {
            _index = _debugger.Update(_index, Keywords.ToList(), input, this);

            // Workflow
            var shouldContinue = false;
            var shouldBreak = false;

            ChangeLoopFlowByCommand(_debugger.Command, ref shouldBreak, ref shouldContinue, _data);

            if (shouldBreak)
            {
                _index = int.MaxValue;
                var exitCode = Exit();
                return new InformativeAnswer(exitCode);
            }

            if (!shouldContinue)
            {
                return null; // Do(_debugger.Command.SelectedKeyword);
            }

            return null;
        }


        public IApplicationLoop Start()
        {
            SharedObjectDataAccess.Instance = new Lazy<SharedObjectDataAccess>();

            // Initialization
            RegisterEventHandlers(_data.GetAll<IApplicationEventHandler>());

            WriteErrorsForCorruptObjects(_data);

            Keywords = _data.GetAll<Keyword>().ToList();

            ThrowIfRunScenariosAreMixedWithStandardKeywords(IsRunningSuite, Keywords);

            return this;
        }

        private int Exit()
        {
            //TODO : the result should be processed by triage, success/failure evaluation must not be done by reports

            SharedObjectDataAccess.Instance = new Lazy<SharedObjectDataAccess>();

            if (Keywords.Any(x => (x).Status == KeywordStatus.Error.ToString()))
            {
                return Program.ScenarioFailureStatusCode;
            }


            return Program.OkStatusCode;
        }


        public static IEnumerable<Keyword> ReadTempFile()
        {
            string fileName = System.IO.Path.GetTempPath() + LastScenarioTempFilePath + ".csv";

            var foa = new FileObjectAccess(fileName, ServiceLocator.Instance.Resolve<ITypeRegistry>());
            return foa.ReadAllObjects().OfType<Keyword>();
        }


        private void WriteResults(bool isRunningSuite, List<Keyword> keywords)
        {
            if (!isRunningSuite)
            {
            //    OnScenarioFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
                Console.WriteLine("\n\t - Keyword scenario ended -\n");
            }
            else
            {
            //    OnSuiteFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
                Console.WriteLine("\n\t - Keyword scenario ended -\n");
            }

            if (_debugger.Config.IsActive)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Green, "Closing application now.");
            }
        }

        private void ExecuteKeyword(Keyword keyword)
        {
            try
            {
                var result = keyword.Execute();

                if (result != null && result.Equals(KeywordResultSpecialCases.Skipped))
                {
                    Console.WriteLine("Skipping " + keyword);
                }
            }
            catch (ApplicationException ae)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Yellow, "Step Failed");
                Console.Out.WriteColoredLine(ConsoleColor.Yellow, ae.Message);
            }
            catch (Exception exception)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Red, "Unexpected error");
                Console.Out.WriteColoredLine(ConsoleColor.Red, exception.Message);
                Console.Out.WriteColoredLine(ConsoleColor.DarkRed, exception.StackTrace);
            }
        }

        private void ChangeLoopFlowByCommand(ConsoleDebuggerCommand command, ref bool shouldBreak, ref bool shouldContinue, IDataContext dataContext)
        {
            if (command.RunStep && command.SelectedKeyword != null)
            {
                Console.WriteLine("Running " + command.SelectedKeyword.ToString() + "...");
            }

            if (command.Break)
            {
                shouldBreak = true;
            }

            if (command.Reload)
            {
                HandleReload(dataContext);

                Keywords = dataContext.GetAll<Keyword>().ToList();

                if (_index >= Keywords.ToList().Count)
                {
                    _index = -1;
                }
                shouldContinue = true;
            }

            if (!command.RunStep)
            {
                shouldContinue = true;
            }
        }

        private void WriteErrorsForCorruptObjects(IDataContext dataContext)
        {
            foreach (var dataObjectAccess in dataContext.DataAccesses)
            {
                if (dataObjectAccess.GetAll<CorruptObject>().Any())
                {
                    Console.Out.WriteColoredLine(ConsoleColor.Red, $"Corrupt data found in input source:");
                    Console.Out.WriteColoredLine(ConsoleColor.DarkRed, $"{dataObjectAccess}");
                }
            }
        }

        private void RegisterEventHandlers(IEnumerable<IApplicationEventHandler> applicationEventHandlers)
        {
            //foreach (var handler in applicationEventHandlers)
            //{
            //    handler.Register(this);
            //}
        }

        private void ThrowIfRunScenariosAreMixedWithStandardKeywords(bool isRunningSuite, List<Keyword> keywords)
        {
            if (isRunningSuite && !keywords.All(x => x is RunScenario))
            {
                throw new ApplicationException("Scenario cannot contain RunScenario steps as well as basic Keywords.");
            }
        }

        private void HandleReload(IDataContext Data)
        {
            if (!Data.GetAll<CorruptObject>().Any())
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var invalidObject in Data.GetAll<CorruptObject>())
            {
                Console.Out.WriteLine();
                Console.Out.WriteLine("Invalid object loaded: " + invalidObject.Exception.Message);
            }
            Console.ResetColor();
        }

    }
}
