using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library
{
    public class RunScenario : Keyword
    {
        public virtual string FileName { get; set; }

        public virtual List<Keyword> Keywords { get; set; } = new List<Keyword>();

        public RunScenario()
        {
        }

        public RunScenario(params Keyword[] keywords)
        {
            Keywords = keywords.ToList();
        }

        public RunScenario(string fileName)
        {
            FileName = fileName;
        }

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

        public override object Run()
        {
            LoadKeywords();

            if (Keywords.Any(x => x is RunScenario))
            {
                throw new ArgumentException("RunScenario cannot call underlying RunScenario keywords.");
            }

            foreach (var keyword in Keywords)
            {
                var result = (keyword).Execute();

                if (result != null && result.Equals(KeywordResultSpecialCases.Skipped))
                {
                    Console.WriteLine("Skipping " + keyword);
                }
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