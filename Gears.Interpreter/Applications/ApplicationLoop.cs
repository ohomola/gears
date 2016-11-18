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
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications
{
    public class ApplicationLoop
    {
        public ApplicationLoop(string[] args)
        {
            this._commandLineArguments = args;
            RegisterDependencies(_commandLineArguments);
        }

        public ApplicationLoop()
        {
        }

        public string OutputLogFile { get; set; }

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

        public List<IDataObjectAccess> CreateDataAccesses(string[] args)
        {
            var accesses =
                args.Where(x => !x.StartsWith("-"))
                    .Select(x => new FileObjectAccess(FileFinder.Find(x)))
                    .Cast<IDataObjectAccess>()
                    .ToList();

            var arguments = args.Where(x => x.StartsWith("-"));
            var parameters = new ObjectDataAccess();
            foreach (var argument in arguments)
            {
                var dtoTypes = ServiceLocator.Instance.Resolve<ITypeRegistry>().GetDTOTypes();
                var type =
                    dtoTypes.FirstOrDefault(registeredType => registeredType.Name.ToLower() == argument.Substring(1).ToLower());
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
                OutputLogFile = string.Format(Properties.Settings.Default.ScenarioOutputPath,
                    DateTime.Now.ToString("s").Replace(":", "_"));

                var data = ServiceLocator.Instance.Resolve<IDataContext>();

                var debugger = ServiceLocator.Instance.Resolve<IConsoleDebugger>();

                foreach (var handler in data.GetAll<IApplicationEventHandler>())
                {
                    handler.Register(this);
                }

                if (data.GetAll<CorruptObject>().Any())
                {
                    Console.Out.WriteColoredLine(ConsoleColor.Red, "Corrupt data loaded: ");
                    var details = "\n\t" +
                                  string.Join(",\n\t",
                                      data.GetAll<CorruptObject>().Select(x => x.Exception.Message.ToString()));
                    Console.Out.WriteColoredLine(ConsoleColor.DarkRed, details);
                }

                var keywords = data.GetAll<Keyword>().ToList();

                ValidateKeywords(keywords);
                var isRunningSuite = keywords.Any(x => x is RunScenario);

                for (var index = debugger.Update(-1, keywords.ToList());
                    index < keywords.Count();
                    index = debugger.Update(index, keywords.ToList()))
                {

                    if (debugger.Command.RunStep && debugger.Command.SelectedKeyword != null)
                    {
                        Console.WriteLine("Running " + debugger.Command.SelectedKeyword.ToString() + "...");
                    }

                    var keyword = debugger.Command.SelectedKeyword;

                    if (debugger.Command.Break)
                    {
                        break;
                    }

                    if (debugger.Command.Reload)
                    {
                        HandleReload(data);

                        keywords = data.GetAll<Keyword>().ToList();

                        if (index >= keywords.ToList().Count)
                        {
                            index = -1;
                        }
                        continue;
                    }

                    if (!debugger.Command.RunStep)
                    {
                        continue;
                    }

                    try
                    {
                        if (isRunningSuite)
                        {
                            RegisterDependencies(_commandLineArguments);
                        }

                        keyword.Status = KeywordStatus.Ok.ToString();

                        ServiceLocator.Instance.Resolve(keyword);

                        if (!string.IsNullOrEmpty(keyword.Skip))
                        {
                            keyword.Status = KeywordStatus.Skipped.ToString();
                            Console.WriteLine("Skipping " + keyword);
                        }

                        try
                        {
                            keyword.Execute();
                        }
                        catch (ApplicationException ae)
                        {
                            keyword.Status = KeywordStatus.Error.ToString();
                            keyword.StatusDetail = ae.Message;
                            Console.Out.WriteColoredLine(ConsoleColor.Yellow, "Step Failed");
                            Console.Out.WriteColoredLine(ConsoleColor.Yellow, ae.Message);
                        }
                        catch (Exception exception)
                        {
                            keyword.Status = KeywordStatus.Error.ToString();
                            keyword.StatusDetail = exception.Message;
                            Console.Out.WriteColoredLine(ConsoleColor.Red, "Unexpected error");
                            Console.Out.WriteColoredLine(ConsoleColor.Red, exception.Message);
                            Console.Out.WriteColoredLine(ConsoleColor.DarkRed, exception.StackTrace);
                        }
                        finally
                        {
                            if (isRunningSuite)
                            {
                                OnScenarioFinished(new ScenarioFinishedEventArgs((keyword as RunScenario).Keywords));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        keyword.Status = KeywordStatus.Error.ToString();

                        if (debugger.Config.IsActive)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Unexpected error encountered.");
                            Console.ResetColor();
                            debugger.Command.StepThrough = true;
                        }
                    }
                }

                Console.WriteLine("\n\t - Keyword scenario ended -\n");

                //TODO : the result should be processed by triage, success/failure evaluation must not be done by reports
                if (!isRunningSuite)
                {
                    OnScenarioFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
                }
                else
                {
                    OnSuiteFinished(new ScenarioFinishedEventArgs(keywords.ToList()));
                }

                if (debugger.Config.IsActive)
                {
                    Console.Out.WriteColoredLine(ConsoleColor.Green, "Closing application now.");
                }

                Bootstrapper.Release();

                if (keywords.Any(x => x.Status == KeywordStatus.Error.ToString()))
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
            Bootstrapper.Release();

            Bootstrapper.RegisterForConfigurationLoad();

            var accesses = CreateDataAccesses(args);

            Bootstrapper.Release();

            Bootstrapper.RegisterForRuntime(accesses);
        }

        private void ValidateKeywords(List<Keyword> keywords)
        {
            if (keywords.OfType<RunScenario>().Any())
            {
                if (!keywords.All(x => x is RunScenario))
                {
                    throw new ApplicationException("Scenario cannot contain RunScenario steps as well as basic Keywords.");
                }
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
