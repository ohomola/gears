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
using System.IO;
using System.Linq;
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
        private const int CriticalErrorStatusCode = -2;
        private const int ScenarioFailureStatusCode = -1;
        private const int OkStatusCode = 0;

        public string OutputLogFile { get; set; }
             
        //todo: --- remove dependency on consoledebugger - make this class shared for debugging app as well as other entry point applications
        public int Run(string[] commandLineArguments)
        {
            OutputLogFile = string.Format(Properties.Settings.Default.ScenarioOutputPath, DateTime.Now.ToString("s").Replace(":","_"));

            Bootstrapper.RegisterForConfigurationLoad();

            var accesses = commandLineArguments.Where(x=>!x.StartsWith("-")).Select(x=>new FileObjectAccess(FileFinder.Find(x))).Cast<IDataObjectAccess>().ToList();

            accesses.Add(new ObjectDataAccess(new ConsoleDebuggerConfig
            {
                IsActive = !commandLineArguments.Contains("-nodebug")
            }));

            Bootstrapper.Release();

            try
            {
                Bootstrapper.RegisterForRuntime(accesses);
            }
            catch (Exception e)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Red, "Error starting application: "+ e.GetAllMessages());
                return CriticalErrorStatusCode;
            }

            var data = ServiceLocator.Instance.Resolve<IDataContext>();

            var debugger = ServiceLocator.Instance.Resolve<IConsoleDebugger>();

            if (data.GetAll<CorruptObject>().Any())
            {
                Console.Out.WriteColoredLine(ConsoleColor.Red, "Corrupt data loaded: ");
                var details = "\n\t" + string.Join(",\n\t", data.GetAll<CorruptObject>().Select(x => x.Exception.Message.ToString()));
                Console.Out.WriteColoredLine(ConsoleColor.DarkRed,  details);
            }

            var keywords = data.GetAll<Keyword>().ToList();

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


            if (OutputLogFile != null)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Gray, $"Results were saved to file \'{OutputLogFile}\'.");
                new FileObjectAccess(OutputLogFile).AddRange(keywords.ToList());

                if (commandLineArguments.Contains("-junitxml"))
                {
                    String junitXmlFile = ReplaceFileExtension(OutputLogFile, ".xml");
                    Console.Out.WriteColoredLine(ConsoleColor.Gray, $"JUnit output was saved to file \'{junitXmlFile}\'.");
                    new FileObjectAccess(junitXmlFile).AddRange(keywords.ToList());
                }
            }

            if (debugger.Config.IsActive)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Green, "Closing application now.");
            }

            Bootstrapper.Release();

            if (keywords.Any(x => x.Status == KeywordStatus.Error.ToString()))
            {
                return ScenarioFailureStatusCode;
            }

            return OkStatusCode;
        }

        private string ReplaceFileExtension(string outputLogFile, string extension)
        {
            return $"{Path.GetFileNameWithoutExtension(outputLogFile)}{extension}";
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
