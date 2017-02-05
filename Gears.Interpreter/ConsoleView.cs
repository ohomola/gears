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
        public const string HorizontalLine = "------------------------------------------------------------------------------------";

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


        private static Dictionary<Type, ConsoleColor> MasterColors;
        private static Dictionary<Type, ConsoleColor> ChildColors;

        public static List<ConsoleOutput> ToDisplayData(IAnswer response)
        {
            MasterColors = new Dictionary<Type, ConsoleColor>()
            {
                { typeof(ExceptionAnswer), ConsoleColor.Red},
                { typeof(CriticalFailure), ConsoleColor.Red},
                { typeof(WarningAnswer), ConsoleColor.Yellow},
                { typeof(ExternalMessageAnswer), ConsoleColor.Blue},
                { typeof(DataDescriptionAnswer), ConsoleColor.Magenta},

                { typeof(SuccessAnswer), ConsoleColor.Green},
                { typeof(IInformativeAnswer), ConsoleColor.White},
            };

            ChildColors = new Dictionary<Type, ConsoleColor>()
            {
                { typeof(ExceptionAnswer), ConsoleColor.DarkRed},
                { typeof(CriticalFailure), ConsoleColor.DarkRed},
                { typeof(WarningAnswer), ConsoleColor.DarkYellow},
                { typeof(ExternalMessageAnswer), ConsoleColor.Blue},
                { typeof(DataDescriptionAnswer), ConsoleColor.DarkMagenta},

                { typeof(SuccessAnswer), ConsoleColor.DarkGreen},
                { typeof(IInformativeAnswer), ConsoleColor.White},
            };

            var returnValue = new List<ConsoleOutput>();

            var exceptionAnswer = response as ExceptionAnswer;
            var successRes = response as SuccessAnswer;
            var warning = response as WarningAnswer;
            var informativeResponse = response as IInformativeAnswer;
            var resultAnswer = response as ResultAnswer;
            var failure = response as CriticalFailure;

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
            else if(response != null)
            {
                AddWithChildrenColored(response, returnValue);
            }

            var statusResponse = response as StatusAnswer;
            if (statusResponse != null)
            {
                BuildStatusOutputs(returnValue, statusResponse);
            }

            return returnValue;
        }

        private static void AddWithChildrenColored(IAnswer response, List<ConsoleOutput> returnValue)
        {
            foreach (var type in MasterColors.Keys)
            {
                if (type.IsAssignableFrom(response.GetType()))
                {
                    returnValue.Add(new ConsoleOutput(MasterColors[type], response.Text + " "));
                    break;
                }
            }

            if (response.Children != null)
            {
                foreach (var child in response.Children)
                {
                    foreach (var type in ChildColors.Keys)
                    {
                        if (type.IsAssignableFrom(child.GetType()))
                        {
                            returnValue.Add(new ConsoleOutput(ChildColors[type], child.Text + " "));
                            break;
                        }
                    }
                }
            }
        }

        private static void AddWithChildren(IInformativeAnswer informativeResponse, ConsoleColor mainColor, ConsoleColor secondaryColor, List<ConsoleOutput> returnValue)
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

            AddLine(ConsoleColor.DarkGray, $"\n{HorizontalLine}", returnValue);
            AddLine(ConsoleColor.DarkGray, "Gears Scenario Debugger", returnValue);
            AddLine(ConsoleColor.DarkGray, HorizontalLine, returnValue);

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