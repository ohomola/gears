using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.Config;

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
                { typeof(IFollowupQuestion), ConsoleColor.White},
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
                { typeof(IFollowupQuestion), ConsoleColor.White},
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


        private static void WriteErrorsForCorruptObjects(IDataContext dataContext, List<ConsoleOutput> returnValue)
        {
            foreach (var dataObjectAccess in dataContext.DataAccesses)
            {
                if (dataObjectAccess.GetAll<CorruptObject>().Any())
                {
                    AddLine(ConsoleColor.Yellow, $"WARNING, Invalid data loaded:", returnValue);
                    foreach (var corruptObject in dataObjectAccess.GetAll<CorruptObject>())
                    {
                        AddLine(ConsoleColor.Red, $"\t - {corruptObject}", returnValue);
                    }

                }
            }
        }

        private static void BuildStatusOutputs(List<ConsoleOutput> returnValue, StatusAnswer status)
        {
            var keywords = status.Keywords.ToList();

            //AddLine(ConsoleColor.DarkGray, $"\n{HorizontalLine}", returnValue);
            //AddLine(ConsoleColor.DarkGray, "Gears Scenario Debugger", returnValue);
            //AddLine(ConsoleColor.DarkGray, HorizontalLine, returnValue);

            WriteErrorsForCorruptObjects(status.Data, returnValue);

            Keyword selectedKeyword = null;

            if (status.Keywords.Any() && status.Keywords.Count()> status.Index && 0 <= status.Index)
            {
                selectedKeyword = status.Keywords.ElementAt(Math.Max(0,status.Index)) as Keyword;
            }

            // Memory variables
            if (status.Data.Contains<RememberedText>())
            {
                foreach (var rememberedText in status.Data.GetAll<RememberedText>())
                {
                    Add(ConsoleColor.Magenta, $"[{rememberedText.Variable}]", returnValue);
                    AddLine(ConsoleColor.DarkMagenta, $" = '{rememberedText.What}' ", returnValue);
                }
            }

            if (status.Data.Contains<SkipAssertions>())
            {
                foreach (var setting in status.Data.GetAll<SkipAssertions>().Distinct())
                {
                    Add(ConsoleColor.DarkMagenta, $"{setting}\n", returnValue);
                }
            }

            var isSteppedIn = false;
            if (keywords.Any(x => x is StepOut))
            {
                AddLine(ConsoleColor.DarkCyan, HorizontalLine, returnValue);
                AddLine(ConsoleColor.Cyan, $"Stepped in RunScenario {/*keywords.OfType<StepOut>().First()*/StepOut.FileName}",
                    returnValue);
                AddLine(ConsoleColor.DarkCyan, HorizontalLine, returnValue);
                isSteppedIn = true;
            }
            else
            {
                AddLine(ConsoleColor.Gray, $"Scenario with {keywords.Count} steps: ", returnValue);
            }

            var indent = isSteppedIn ? "\t---\t" : "";

            //10 before
            if (status.Index > 10)
            {
                AddLine(ConsoleColor.DarkGray, indent+(status.Index - 10) + " additional steps...", returnValue);
            }
            for (int i = Math.Max(0, status.Index - 10); i < status.Index; i++)
            {
                
                WriteKeywordLine(keywords, i, returnValue, indent);
            }

            //Selected
            Add(ConsoleColor.Cyan,
                $"{indent} >{(status.Index + 1)}) {(selectedKeyword?.ToString() ?? " --- end of scenario ---")}", returnValue);
            if (selectedKeyword != null && selectedKeyword.IsLazy() && !selectedKeyword.IsLazyHydrated())
            {
                Add(ConsoleColor.Magenta, "Expression", returnValue);
            }
            if (selectedKeyword!= null && selectedKeyword.Skip)
            {
                Add(ConsoleColor.Green, " (Skip)", returnValue);
            }
            Add(ConsoleColor.DarkGray, "\n", returnValue);

            //10 after
            for (int i = Math.Min(keywords.Count(), status.Index + 1); i < Math.Min(status.Index + 10, keywords.Count()); i++)
            {
                //Console.Out.WriteColoredLine(ConsoleColor.DarkGray, "  " + (1 + i) + ") " + keywords.ElementAt(i).ToString());
                WriteKeywordLine(keywords, i, returnValue, indent);
            }

            if (keywords.Count() - status.Index > 10)
            {
                AddLine(ConsoleColor.DarkGray, "  ...(" + (keywords.Count() - status.Index - 10) + ") more steps...", returnValue);
            }

            if (isSteppedIn)
            {
                Add(ConsoleColor.Cyan, "\nUse 'StepOut' to return to main scenario.\n", returnValue);
            }
            Add(ConsoleColor.White, "\nEnter command, type 'help', or press <enter> to continue:", returnValue);
            //if (selectedKeyword != null && !status.Interpreter.Language.Options.IsNullOrEmpty())
            //{
            //    Add(ConsoleColor.DarkGray,
            //        $"\n\tAdditional Options: {string.Join(", ", selectedKeyword.Interpreter.Language.Options.Select(x => x.GetType().Name))}",
            //        returnValue);
            //}
        }

        private static void Add(ConsoleColor color, string text, List<ConsoleOutput> returnValue)
        {
            returnValue.Add(new ConsoleOutput(color, text));
        }

        private static void AddLine(ConsoleColor color, string text, List<ConsoleOutput> returnValue)
        {
            returnValue.Add(new ConsoleOutput(color, text+"\n"));
        }

        private static void WriteKeywordLine(List<IKeyword> keywords, int i, List<ConsoleOutput> returnValue, string indent)
        {
            var keyword = (Keyword)keywords.ElementAt(i);

            Add(ConsoleColor.DarkGray, $"{indent}  {(i + 1)}) {keyword} ", returnValue);
            if (keyword.Status == KeywordStatus.Ok.ToString())
            {
                Add(ConsoleColor.Green, keyword.Status, returnValue);
            }
            if (keyword.Status == KeywordStatus.Error.ToString())
            {
                Add(ConsoleColor.Yellow, keyword.Status, returnValue);
            }
            if (keyword.Skip && keyword.Status != KeywordStatus.Skipped.ToString())
            {
                Add(ConsoleColor.DarkGreen, " (Skip)", returnValue);
            }
            else if (keyword.Status == KeywordStatus.Skipped.ToString())
            {
                Add(ConsoleColor.DarkGreen, keyword.Status, returnValue);
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