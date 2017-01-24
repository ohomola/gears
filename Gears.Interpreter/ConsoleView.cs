using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Library;

namespace Gears.Interpreter
{
    public class ConsoleView
    {
        public static void Render(IAnswer answer)
        {
            var outputs = ToDisplayData(answer);

            foreach (var consoleOutput in outputs)
            {
                Console.Out.WriteColored(consoleOutput.Color, consoleOutput.Text);
            }

            Console.Out.Write("\n");
        }

        public static void Render(ConsoleColor color, string text)
        {
            Console.Out.WriteColoredLine(color, text);
        }

        public static List<ConsoleOutput> ToDisplayData(IAnswer response)
        {
            var returnValue = new List<ConsoleOutput>();

            var exceptionAnswer = response as ExceptionAnswer;
            var successRes = response as SuccessAnswer;
            var warning = response as WarningAnswer;
            var informativeResponse = response as InformativeAnswer;
            var resultAnswer = response as ResultAnswer;

            if (resultAnswer != null)
            {
                if (resultAnswer.Code == Program.CriticalErrorStatusCode)
                {
                    AddWithChildren(resultAnswer, ConsoleColor.Red, ConsoleColor.DarkRed, returnValue);
                }
                else if (resultAnswer.Code == Program.ScenarioFailureStatusCode)
                {
                    AddWithChildren(resultAnswer, ConsoleColor.Yellow, ConsoleColor.DarkYellow, returnValue);
                }
                else if (resultAnswer.Code == Program.OkStatusCode)
                {
                    AddWithChildren(resultAnswer, ConsoleColor.Green, ConsoleColor.DarkGreen, returnValue);
                }
            }
            else if (exceptionAnswer != null)
            {
                AddWithChildren(informativeResponse, ConsoleColor.Red, ConsoleColor.DarkRed, returnValue);
            }
            else if (warning != null)
            {
                AddWithChildren(informativeResponse, ConsoleColor.Yellow, ConsoleColor.DarkYellow, returnValue);
            }
            else if (successRes != null)
            {
                AddWithChildren(informativeResponse, ConsoleColor.Green, ConsoleColor.DarkGreen, returnValue);
            }
            else if (informativeResponse != null)
            {
                AddWithChildren(informativeResponse, ConsoleColor.White, ConsoleColor.Gray, returnValue);
            }

            var statusResponse = response as StatusAnswer;
            if (statusResponse != null)
            {
                BuildStatusOutputs(returnValue, statusResponse);
            }

            return returnValue;
        }

        private static void AddWithChildren(InformativeAnswer informativeResponse, ConsoleColor mainColor, ConsoleColor secondaryColor, List<ConsoleOutput> returnValue)
        {
            returnValue.Add(new ConsoleOutput(mainColor, informativeResponse.Text+ " "));

            foreach (var child in informativeResponse.Children)
            {
                returnValue.Add(new ConsoleOutput(secondaryColor, child.Text));
            }
        }

        private static void BuildStatusOutputs(List<ConsoleOutput> returnValue, StatusAnswer status)
        {
            var keywords = status.Keywords.ToList();

            AddLine(ConsoleColor.DarkGray,"\n----------------------------------------", returnValue);
            AddLine(ConsoleColor.DarkGray, "Gears Scenario Debugger", returnValue);
            AddLine(ConsoleColor.DarkGray, "----------------------------------------", returnValue);

            Keyword selectedKeyword = null;

            if (status.Keywords.Any() && status.Keywords.Count()> status.Index && 0 <= status.Index)
            {
                selectedKeyword = status.Keywords.ElementAt(Math.Max(0,status.Index)) as Keyword;
            }

            if (status.Data.Contains<RememberedText>())
            {
                foreach (var rememberedText in status.Data.GetAll<RememberedText>())
                {
                    Add(ConsoleColor.Magenta, $"[{rememberedText.Variable}]", returnValue);
                    AddLine(ConsoleColor.DarkMagenta, $" = '{rememberedText.What}' ", returnValue);
                }
            }

            AddLine(ConsoleColor.Gray, $"Scenario with {keywords.Count} steps: ", returnValue);

            //10 before
            if (status.Index > 10)
            {
                AddLine(ConsoleColor.DarkGray, (status.Index - 10) + " additional steps...", returnValue);
            }
            for (int i = Math.Max(0, status.Index - 10); i < status.Index; i++)
            {
                
                WriteKeywordLine(keywords, i, returnValue);
            }

            //Selected
            Add(ConsoleColor.Cyan, " >" + (status.Index + 1) + ") " + (selectedKeyword?.ToString() ?? " --- end of scenario ---"), returnValue);
            if (selectedKeyword != null && selectedKeyword.IsLazy() && !selectedKeyword.IsLazyHydrated())
            {
                Add(ConsoleColor.Magenta, "Expression", returnValue);
            }
            Add(ConsoleColor.DarkGray, "\n", returnValue);

            //10 after
            for (int i = Math.Min(keywords.Count(), status.Index + 1); i < Math.Min(status.Index + 10, keywords.Count()); i++)
            {
                //Console.Out.WriteColoredLine(ConsoleColor.DarkGray, "  " + (1 + i) + ") " + keywords.ElementAt(i).ToString());
                WriteKeywordLine(keywords, i, returnValue);
            }

            if (keywords.Count() - status.Index > 10)
            {
                AddLine(ConsoleColor.DarkGray, "  ...(" + (keywords.Count() - status.Index - 10) + ") more steps...", returnValue);
            }

            Add(ConsoleColor.White, "\nEnter command, type 'help', or press <enter> to continue:", returnValue);
        }

        private static void Add(ConsoleColor color, string text, List<ConsoleOutput> returnValue)
        {
            returnValue.Add(new ConsoleOutput(color, text));
        }

        private static void AddLine(ConsoleColor color, string text, List<ConsoleOutput> returnValue)
        {
            returnValue.Add(new ConsoleOutput(color, text+"\n"));
        }

        private static void WriteKeywordLine(List<IKeyword> keywords, int i, List<ConsoleOutput> returnValue)
        {
            var keyword = (Keyword)keywords.ElementAt(i);

            Add(ConsoleColor.DarkGray, $"  {(i + 1)}) {keyword} ", returnValue);
            if (keyword.Status == KeywordStatus.Ok.ToString())
            {
                Add(ConsoleColor.Green, keyword.Status, returnValue);
            }
            if (keyword.Status == KeywordStatus.Error.ToString())
            {
                Add(ConsoleColor.Yellow, keyword.Status, returnValue);
            }
            if (keyword.Status == KeywordStatus.Skipped.ToString())
            {
                Add(ConsoleColor.DarkGray, keyword.Status, returnValue);
            }
            if (keyword.IsLazy() && !keyword.IsLazyHydrated())
            {
                Add(ConsoleColor.DarkMagenta, "Expression", returnValue);
            }

            Add(ConsoleColor.DarkGray, "\n", returnValue);
        }

    }

    public class ConsoleOutput
    {
        public ConsoleColor Color;
        public string Text;

        public ConsoleOutput(ConsoleColor color, string text)
        {
            Color = color;
            Text = text;
        }

        public override string ToString()
        {
            return $"[{Color}] '{Text}'";
        }
    }
}