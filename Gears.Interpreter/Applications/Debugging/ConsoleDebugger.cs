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
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications.Debugging
{
    public interface IConsoleDebugger
    {
        int Update(int index, IEnumerable<Keyword> keywords);
        ConsoleDebuggerConfig Config { get; set; }
        ConsoleDebuggerCommand Command { get; set; }
    }

    public class ConsoleDebugger : IConsoleDebugger
    {
        public ConsoleDebuggerCommand Command { get; set; }
        public ConsoleDebuggerConfig Config { get; set; }
        public IDataContext Data { get; set; }
        public ISeleniumAdapter Selenium { get; set; }

        public ConsoleDebugger(IDataContext data, ConsoleDebuggerConfig config, ISeleniumAdapter selenium)
        {
            Data = data;
            Config = config;
            Selenium = selenium;
            Command = new ConsoleDebuggerCommand(-1);
        }

        public int Update(int index, IEnumerable<Keyword> keywords)
        {
            index = index + 1;

            //if (!keywords.Any() || index >= keywords.Count())
            //{
            //    return keywords.Count();
            //}

            Keyword selectedKeyword;

            if (!keywords.Any() || index >= keywords.Count())
            {
                selectedKeyword = null;
            }
            else
            {
                selectedKeyword = keywords.ElementAt(index);
            }

            ResetCommand(index, selectedKeyword);

            if (Command.StepThrough == false)
            {
                return Command.NextIndex;
            }
            var hooks = GetActionHooks(index, keywords);
            
            OutputStatusText(index, keywords, selectedKeyword, hooks);

            try
            {
                ParseInput(hooks, index, keywords);
            
                
                Console.ResetColor();
            }
            catch (Exception exception)
            {
                Console.Out.WriteColored(ConsoleColor.Red, exception.Message);
                DontDoAnything(index);
            }

            return Command.NextIndex;
        }

        private void ResetCommand(int index, Keyword currentKeyword)
        {
            Command.Reload = false;


            if (Config.IsActive && Command.InitialisedUserControl == false)
            {
                KernelInteropAdapter.ConfigureConsoleWindow();

                Command.StepThrough = true;

                Command.InitialisedUserControl = true;
            }

            Command.RunStep = true;
            Command.NextIndex = index;
            Command.SelectedKeyword = currentKeyword;

        }

        private void ParseInput(List<ConsoleDebuggerActionHook> commands, int index, IEnumerable<Keyword> keywords)
        {
            var userInput = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(userInput))
            {
                if (index == keywords.Count())
                {
                    Command.Break = true;
                }

                return;
            }

            foreach (var c in commands)
            {
                if (c.Matches(userInput))
                {
                    c.Action(userInput);
                    return;
                }
            }

            Console.Out.WriteColoredLine(ConsoleColor.Yellow, $"'{userInput}' is not a recognized command.");
            DontDoAnything(index);
        }

        private List<ConsoleDebuggerActionHook> GetActionHooks(int index, IEnumerable<Keyword> allKeywords)
        {
            var commands = new List<ConsoleDebuggerActionHook>()
            {
                new ConsoleDebuggerActionHook("click (.+)", "click X: use this to click on element ad-hoc", input =>
                {
                    var arg = ParseArguments(input, 1).First();
                    var click = new Click(arg);
                    ServiceLocator.Instance.Resolve(click);
                    Command.SelectedKeyword = click;
                    Command.NextIndex = Command.NextIndex-1;
                }),
                new ConsoleDebuggerActionHook("isvisible (.+)", "isvisible X: checks visibility of text", input =>
                {
                    var arg = ParseArguments(input, 1).First();
                    var isVisible = new IsVisible(arg) {Expect = true};
                    ServiceLocator.Instance.Resolve(isVisible);
                    Command.SelectedKeyword = isVisible;
                    Command.NextIndex = Command.NextIndex-1;
                }),
                new ConsoleDebuggerActionHook("fill (.+) (.+)", "fill X Y: use this to fill on element ad-hoc", input =>
                {
                    var args = ParseArguments(input, 2);
                    var fill = new Fill(args.First(), args.Last());
                    ServiceLocator.Instance.Resolve(fill);
                    Command.SelectedKeyword = fill;
                    Command.NextIndex = Command.NextIndex-1;
                }),
                new ConsoleDebuggerActionHook("show", "show : currently selected keyword will not preform any controller action but instead will only highlight the element it owuld normally interact with.", input =>
                {
                    var technique = Command.SelectedKeyword as IHasTechnique;
                    if (technique != null)
                    {
                        DontDoAnything(index);
                        Technique oldValue = technique.Technique;
                        technique.Technique = Technique.HighlightOnly;
                        try
                        {
                            ServiceLocator.Instance.Resolve(Command.SelectedKeyword);
                            Command.SelectedKeyword.Execute();
                        }
                        finally
                        {
                            technique.Technique = oldValue;
                        }
                    }
                    else
                    {
                        Console.Out.WriteColoredLine(ConsoleColor.Yellow, "The selected keyword cannot be called with highlight technique.");
                        DontDoAnything(index);
                    }
                }),
                new ConsoleDebuggerActionHook("show click (.+)", "show click X:  a full-text instruction to display on screen.", input =>
                {
                    var args = ParseArguments(input, 2);
                    var show = new Click(args.Last()) {Technique = Technique.HighlightOnly};
                    ServiceLocator.Instance.Resolve(show);
                    Command.SelectedKeyword = show;
                    Command.NextIndex = Command.NextIndex-1;
                }),
                new ConsoleDebuggerActionHook("show fill (.+)", "show fill X:  a full-text instruction to display on screen.", input =>
                {
                    var args = ParseArguments(input, 2);
                    var show = new Fill(args.Last()) {Technique = Technique.HighlightOnly};
                    ServiceLocator.Instance.Resolve(show);
                    Command.SelectedKeyword = show;
                    Command.NextIndex = Command.NextIndex-1;
                }),
                new ConsoleDebuggerActionHook("goto (.+)", "goto X: use this to click on element ad-hoc", input =>
                {
                    var arg = ParseArguments(input, 1).First();
                    var go = new GoToUrl(arg);
                    ServiceLocator.Instance.Resolve(go);
                    Command.SelectedKeyword = go;
                    Command.NextIndex = Command.NextIndex-1;
                }),
                new ConsoleDebuggerActionHook("restart", "restart : go back to the beginning of the test", input =>
                {
                    Command.NextIndex = -1;
                    Command.RunStep = false;
                }),
                new ConsoleDebuggerActionHook("savehtml", "savehtml : save current page source to a new HTML file", input =>
                {
                    var save = new SaveHtml();
                    ServiceLocator.Instance.Resolve(save);
                    Command.SelectedKeyword = save;
                    Command.NextIndex = Command.NextIndex-1;
                }),
                new ConsoleDebuggerActionHook("run ([0-9]*)", "run <N>: runs N steps", input =>
                {
                    var args2 = ParseArguments(input, 1);
                    Command.StopOnIndex = Math.Min(allKeywords.Count() - 2, index + 1 + int.Parse(args2.First()));
                    Command.StepThrough = false;
                }),
                new ConsoleDebuggerActionHook("run", "run : Run selected step", input => { Command.StepThrough = false; }),
                new ConsoleDebuggerActionHook("back ([0-9]*)", "back <N>: Return selection N steps back", input =>
                {
                    var args3 = ParseArguments(input, 1);
                    Command.NextIndex = Math.Max(-1, index - 1 - int.Parse(args3.First()));
                    Command.RunStep = false;
                }),
                new ConsoleDebuggerActionHook("back", "back : Return selection to previous step", input =>
                {
                    Command.NextIndex = Math.Max(-1, index - 2);
                    Command.RunStep = false;
                }),
                new ConsoleDebuggerActionHook("skip ([0-9]*)", "skip N : Skips N steps", input =>
                {
                    var args4 = ParseArguments(input, 1);
                    Command.NextIndex = Math.Min(allKeywords.Count() - 2, index - 1 + int.Parse(args4.First()));
                    Command.RunStep = false;
                }),
                new ConsoleDebuggerActionHook("skip", "skip : Skips one step", input => { Command.RunStep = false; }),
                new ConsoleDebuggerActionHook("stop", "stop : Stops the test", input =>
                {
                    Command.Break = true;
                    DontDoAnything(index);
                }),
                new ConsoleDebuggerActionHook("reload", "reload : re-reads all input files", input =>
                {
                    foreach (var dataAccess1 in Data.DataAccesses.OfType<FileObjectAccess>())
                    {
                        dataAccess1.ForceReload();
                    }
                    Command.Reload = true;
                    DontDoAnything(index);
                })
            };
            return commands;
        }

        private void DontDoAnything(int index)
        {
            Command.NextIndex = index - 1;
            Command.RunStep = false;
        }
        
        private void OutputStatusText(int index, IEnumerable<Keyword> keywords, Keyword selectedKeyword, IEnumerable<ConsoleDebuggerActionHook> commands)
        {
            var horizontalLine = "----------------------------------------";
            Console.Out.WriteColoredLine(ConsoleColor.Gray, horizontalLine);
            Console.Out.WriteColoredLine(ConsoleColor.White, "Scenario Debugger");
            Console.Out.WriteColoredLine(ConsoleColor.Gray, horizontalLine);
            Console.WriteLine("Console commands:");

            foreach (var c in commands)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Gray, "\t"+c.Description);
            }

            Console.Out.WriteColoredLine(ConsoleColor.Gray, horizontalLine);

            Console.WriteLine("Scenario with "+keywords.Count()+" steps.");
            Console.WriteLine("Selected step is:\t");

            Write10PreviousKeywords(index, keywords);

            WriteSelectedKeyword(index, selectedKeyword);

            WriteNext10Keywords(index, keywords);

            WriteNumberOfKeywordsBeyond10(index, keywords);

            Console.Out.WriteColoredLine(ConsoleColor.White, "\nEnter command, or press <enter> to continue:");
        }

        private static void WriteSelectedKeyword(int index, Keyword selectedKeyword)
        {
            Console.Out.WriteColoredLine(ConsoleColor.Cyan, " >" + (index + 1) + ") " + (selectedKeyword?.ToString() ?? " --- end of scenario ---"));
        }

        private static void WriteNumberOfKeywordsBeyond10(int index, IEnumerable<Keyword> keywords)
        {
            if (keywords.Count() - index > 10)
            {
                Console.Out.WriteColoredLine(ConsoleColor.DarkGray, "  ...(" + (keywords.Count() - index - 10) + ") more steps...");
            }
        }

        private static void WriteNext10Keywords(int index, IEnumerable<Keyword> keywords)
        {
            for (int i = Math.Min(keywords.Count(), index + 1); i < Math.Min(index + 10, keywords.Count()); i++)
            {
                //Console.Out.WriteColoredLine(ConsoleColor.DarkGray, "  " + (1 + i) + ") " + keywords.ElementAt(i).ToString());
                WriteKeywordLine(keywords, i);
            }
        }

        private static void Write10PreviousKeywords(int index, IEnumerable<Keyword> keywords)
        {
            if (index > 10)
            {
                Console.Out.WriteColoredLine(ConsoleColor.DarkGray, (index - 10) + " additional steps...");
            }
            for (int i = Math.Max(0, index - 10); i < index; i++)
            {
                WriteKeywordLine(keywords, i);
            }
        }

        private static void WriteKeywordLine(IEnumerable<Keyword> keywords, int i)
        {
            var keyword = keywords.ElementAt(i);
            Console.Out.WriteColored(ConsoleColor.DarkGray, $"  {(i + 1)}) {keyword} ");
            if (keyword.Status == KeywordStatus.Ok.ToString())
            {
                Console.Out.WriteColored(ConsoleColor.Green, keyword.Status);
            }
            if (keyword.Status == KeywordStatus.Error.ToString())
            {
                Console.Out.WriteColored(ConsoleColor.Yellow, keyword.Status);
            }
            if (keyword.Status == KeywordStatus.Skipped.ToString())
            {
                Console.Out.WriteColored(ConsoleColor.DarkGray, keyword.Status);
            }
            Console.Out.WriteLine();
        }

        private List<string> ParseArguments(string command, int numberOfArguments)
        {
            var separator = " ";
            
            var strings = command.Split(new [] {separator}, StringSplitOptions.RemoveEmptyEntries);

            if (strings.Count() <= numberOfArguments)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Out.Write("Must provide a parameter.");
                Console.ResetColor();
                throw new ArgumentException("Must provide a parameter.");
            }

            var values = strings.Skip(1).Take(numberOfArguments - 1).ToList();

            values.Add(string.Join(" ", strings.Skip(numberOfArguments)));
            return values.ToList();
        }
    }
}