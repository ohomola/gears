using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using NUnit.Framework.Constraints;

namespace Gears.Interpreter.Library
{
    public class RunScenario : Keyword, IHasTechnique
    {
        public virtual string FileName { get; set; }

        [DoNotWire]
        public virtual List<IKeyword> Keywords { get; set; } = new List<IKeyword>();

        public override IKeyword FromString(string textInstruction)
        {
            return new RunScenario(ExtractSingleParameterFromTextInstruction(textInstruction));
        }

        public RunScenario()
        {
        }

        public RunScenario(params IKeyword[] keywords)
        {
            Keywords = keywords.ToList();
        }

        public RunScenario(string fileName)
        {
            FileName = fileName;
        }

        #region Documentation

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Execute entire scenario plan file. Use this keyword to define scenario-of-scenarios (i.e. Suite).

> Note: You can debug a scenario containing RunScenario steps normally, however the entire RunScenarios will be called end-to-end, you cannot step inside them.

#### Scenario usage
| Discriminator | FileName           | 
| ------------- | -----              |
| RunScenario   | ./Test1.xlsx       |
";
        }

        #endregion

        private void LoadKeywords()
        {
            if (Keywords.Any())
            {
                return;
            }

            if (!ServiceLocator.IsInitialised() || !ServiceLocator.Instance.Kernel.HasComponent(typeof(ITypeRegistry)))
            {
                throw new InvalidOperationException("Cannot construct RunScenario from file without Registered ITypeRegistry");
            }

            if (FileName != null)
            {
                Keywords.AddRange(
                    new DataContext(new FileObjectAccess(FileFinder.Find(FileName),
                        ServiceLocator.Instance.Resolve<ITypeRegistry>())).GetAll<Keyword>());
            }
        }

        public override object DoRun()
        {
            LoadKeywords();

            if (Technique == Technique.HighlightOnly && Interpreter.IsDebugMode)
            {
                return StepIntoScenario();
            }

            if (Keywords.Any(x => x is RunScenario))
            {
                throw new ArgumentException("RunScenario cannot call underlying RunScenario keywords.");
            }

            try
            {
                foreach (var keyword in Keywords)
                {
                    var result = (keyword).Execute();

                    if (result != null && result.Equals(KeywordResultSpecialCases.Skipped))
                    {
                        Console.WriteLine("Skipping " + keyword);
                    }
                }
            }
            finally
            {
                Interpreter.OnScenarioFinished(new ScenarioFinishedEventArgs(Keywords.ToList(), FileName));
            }
            
            return null;
        }

        private object StepIntoScenario()
        {
            var list = new List<IKeyword>(Keywords);

            list.Add(new StepOut(Interpreter.Plan, Interpreter.Iterator.Index, FileName));

            Interpreter.Plan = list;

            Interpreter.Iterator.Index = 0;

            return new InformativeAnswer("Stepping into Sub-Scenario");
        }

        public override string ToString()
        {
            //return $"Run Scenario {FileName ?? ""} ({Keywords.Count}) steps: \n\t{string.Join("\n\t", Keywords.Take(Math.Min(Keywords.Count, 5)))} {(Keywords.Count>5?"\n\t...("+(Keywords.Count-5)+" more)...":"")}";
            return $"Run Scenario {FileName ?? ""}";
        }

        public Technique Technique { get; set; }
    }
}