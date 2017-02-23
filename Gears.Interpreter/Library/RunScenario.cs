using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library
{
    public class RunScenario : Keyword
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
| Discriminator | FileName           | 
| ------------- | -----              |
| RunScenario   | ./Test1.xlsx       |
";
        }

        #endregion

        private void LoadKeywords()
        {
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

        public override string ToString()
        {
            //return $"Run Scenario {FileName ?? ""} ({Keywords.Count}) steps: \n\t{string.Join("\n\t", Keywords.Take(Math.Min(Keywords.Count, 5)))} {(Keywords.Count>5?"\n\t...("+(Keywords.Count-5)+" more)...":"")}";
            return $"Run Scenario {FileName ?? ""}";
        }
    }
}