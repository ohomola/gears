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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Castle.MicroKernel;
using Castle.Windsor;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Data.Serialization.Mapping;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications
{
    public class ApplicationLoop
    {
        public ApplicationLoop(string[] args)
        {
            this._commandLineArguments = args;
            RegisterDependencies(_commandLineArguments);
            _data = ServiceLocator.Instance.Resolve<IDataContext>();
            _debugger = ServiceLocator.Instance.Resolve<IConsoleDebugger>();
        }

        public ApplicationLoop(IDataContext data, IConsoleDebugger debugger)
        {
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

        private string[] _commandLineArguments = new string[0];
        private readonly IDataContext _data;
        private readonly IConsoleDebugger _debugger;

        public List<IDataObjectAccess> CreateDataAccesses(string[] args, WindsorContainer container)
        {
            var accesses =
                args.Where(x => !x.StartsWith("-"))
                    .Select(x => container.Resolve<FileObjectAccess>(new { path = FileFinder.Find(x)}))
                    .Cast<IDataObjectAccess>()
                    .ToList();

            var arguments = args.Where(x => x.StartsWith("-"));
            var parameters = new ObjectDataAccess();
            foreach (var argument in arguments)
            {
                var dtoTypes = ServiceLocator.Instance.Resolve<ITypeRegistry>().GetDTOTypes();
                var type =
                    dtoTypes.FirstOrDefault(registeredType => registeredType.Name.ToLower() == argument.Substring(1).ToLower());

                if (type == null)
                {
                    throw new ArgumentException($"Argument {argument} is not recognized.");
                }
                var instance = Activator.CreateInstance(type);
                parameters.Add(instance);
            }
            accesses.Add(parameters);
            return accesses;
        }


        //todo: --- remove dependency on consoledebugger - make this class shared for debugging app as well as other entry point applications
        public int Run()
        {
            try
            {
                // Initialization

                foreach (var handler in _data.GetAll<IApplicationEventHandler>())
                {
                    handler.Register(this);
                }

                foreach (var dataObjectAccess in _data.DataAccesses)
                {
                    if (dataObjectAccess.GetAll<CorruptObject>().Any())
                    {
                        Console.Out.WriteColoredLine(ConsoleColor.Red, $"Corrupt data found in input source:");
                        Console.Out.WriteColoredLine(ConsoleColor.DarkRed, $"{dataObjectAccess}");
                    }
                }
                
                var keywords = _data.GetAll<Keyword>().ToList();

                ValidateKeywords(keywords);
                var isRunningSuite = keywords.Any(x => x is RunScenario);

                for (var index = _debugger.Update(-1, keywords.ToList());
                    index < keywords.Count();
                    index = _debugger.Update(index, keywords.ToList()))
                {

                    // Workflow
                    if (_debugger.Command.RunStep && _debugger.Command.SelectedKeyword != null)
                    {
                        Console.WriteLine("Running " + _debugger.Command.SelectedKeyword.ToString() + "...");
                    }
                    
                    if (_debugger.Command.Break)
                    {
                        break;
                    }

                    if (_debugger.Command.Reload)
                    {
                        HandleReload(_data);

                        keywords = _data.GetAll<Keyword>().ToList();

                        if (index >= keywords.ToList().Count)
                        {
                            index = -1;
                        }
                        continue;
                    }

                    if (!_debugger.Command.RunStep)
                    {
                        continue;
                    }
                    
                    // Execution
                    var keyword = _debugger.Command.SelectedKeyword;

                    try
                    {
                        if (isRunningSuite)
                        {
                            RegisterDependencies(_commandLineArguments);
                        }
                        
                        var result = (keyword).Execute();

                        if (result!= null && result.Equals(KeywordResultSpecialCases.Skipped))
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
                    finally
                    {
                        if (isRunningSuite)
                        {
                            OnScenarioFinished(new ScenarioFinishedEventArgs((keyword as RunScenario).Keywords.ToList()));
                        }
                    }
                }

                //TODO : the result should be processed by triage, success/failure evaluation must not be done by reports
                if (!isRunningSuite)
                {
                    OnScenarioFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
                    Console.WriteLine("\n\t - Keyword scenario ended -\n");
                }
                else
                {
                    OnSuiteFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
                    Console.WriteLine("\n\t - Keyword scenario ended -\n");
                }

                if (_debugger.Config.IsActive)
                {
                    Console.Out.WriteColoredLine(ConsoleColor.Green, "Closing application now.");
                }

                Bootstrapper.Release();

                if (keywords.Any(x => (x).Status == KeywordStatus.Error.ToString()))
                {
                    return Program.ScenarioFailureStatusCode;
                }

                return Program.OkStatusCode;
            }
            catch (Exception e)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Red, e.Message);
                return Program.CriticalErrorStatusCode;
            }
            finally
            {
                Bootstrapper.Release();
            }
        }

        private void RegisterDependencies(string[] args)
        {
            //Bootstrapper.Release();

            //Bootstrapper.Register();

            //var accesses = CreateDataAccesses(args);

            Bootstrapper.Release();

            Bootstrapper.Register(args);
        }

        private void ValidateKeywords(List<Keyword> keywords)
        {
            if (keywords.Any(x=>x is RunScenario) && !keywords.All(x => x is RunScenario))
            {
                throw new ApplicationException("Scenario cannot contain RunScenario steps as well as basic Keywords.");
            }
        }

        private void HandleReload(IDataContext Data)
        {
            if (Data.GetAll<CorruptObject>().Any())
            {
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

    
}
