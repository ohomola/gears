﻿using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Data.Serialization.Mapping;

namespace Gears.Interpreter.Library
{
    public class RunScenario : Keyword
    {
        private readonly string _fileName;

        private readonly List<Keyword> _keywords;

        public RunScenario(params Keyword[] keywords)
        {
            _keywords = keywords.ToList();
        }

        public RunScenario(string fileName)
        {
            _fileName = fileName;
            _keywords = new List<Keyword>();

            if (!ServiceLocator.IsInitialised() || !ServiceLocator.Instance.Kernel.HasComponent(typeof(ITypeRegistry)))
            {
                throw new InvalidOperationException("Cannot construct RunScenario from file without Registered ITypeRegistry");
            }

            if (_fileName != null)
            {
                _keywords.AddRange(new DataContext(new FileObjectAccess(FileFinder.Find(_fileName), ServiceLocator.Instance.Resolve<ITypeRegistry>())).GetAll<Keyword>());
            }
        }

        public List<Keyword> Keywords {
            get { return _keywords.Select(x => x).ToList(); }
        }

        public override object Run()
        {
            if (_keywords.Any(x => x is RunScenario))
            {
                throw new ArgumentException("RunScenario cannot call underlying RunScenario keywords.");
            }

            foreach (var keyword in _keywords)
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
            return $"Run Scenario {_fileName ?? ""} ({_keywords.Count}) steps: \n\t{string.Join("\n\t", _keywords.Take(Math.Min(_keywords.Count, 5)))} {(_keywords.Count>5?"\n\t...("+(_keywords.Count-5)+" more)...":"")}";
        }
    }
}