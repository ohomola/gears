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
    public class RunScenario : Keyword, IHasTechnique, IHavePlan
    {
        
        public virtual string FileName { get; set; }

        public Iterator<IKeyword> Iterator { get; set; }

        [DoNotWire]
        public virtual IEnumerable<IKeyword> Plan
        {
            get { return _plan; }
            set { _plan = value.ToList(); }
        }
        private List<IKeyword> _plan = new List<IKeyword>();

        public override IKeyword FromString(string textInstruction)
        {
            return new RunScenario(ExtractSingleParameterFromTextInstruction(textInstruction));
        }

        public RunScenario()
        {
            Iterator = new Iterator<IKeyword>(this, x => Plan);
        }

        public RunScenario(params IKeyword[] keywords)
        {
            Plan = keywords.ToList();
            Iterator = new Iterator<IKeyword>(this, x => Plan);
        }

        public RunScenario(string fileName)
        {
            FileName = fileName;
            Iterator = new Iterator<IKeyword>(this, x => Plan);
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
            if (Plan.Any())
            {
                return;
            }

            if (!ServiceLocator.IsInitialised() || !ServiceLocator.Instance.Kernel.HasComponent(typeof(ITypeRegistry)))
            {
                throw new InvalidOperationException("Cannot construct RunScenario from file without Registered ITypeRegistry");
            }

            if (FileName != null)
            {
                _plan.AddRange(
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

            if (_plan.Any(x => x is RunScenario))
            {
                throw new ArgumentException("RunScenario cannot call underlying RunScenario keywords.");
            }

            while (!Iterator.IsEndOfList())
            {
                var keyword = Iterator.Current;
                
                try
                {
                    ConsoleView.Render(ConsoleColor.DarkGray, $"RunScenario: Starting step {Iterator.Index+1})\t{Iterator.Current}");

                    var result = (keyword).Execute();

                    if (result != null && result.Equals(KeywordResultSpecialCases.Skipped))
                    {
                        Console.WriteLine("Skipping " + keyword);
                    }

                    ConsoleView.Render(ConsoleColor.DarkGray, $"RunScenario: Finished step {Iterator.Index+1})\t{Iterator.Current}\n{Iterator.Current.Status}");

                    if (result is IAnswer)
                    {
                        ConsoleView.Render((IAnswer) result);
                    }
                    else
                    {
                        ConsoleView.Render(new InformativeAnswer(result));
                    }

                    Iterator.MoveNext();
                }
                catch (Exception e)
                {
                    if (Interpreter.IsDebugMode)
                    {
                        Interpreter.Iterator.MoveBack(1);
                        Iterator.MoveNext();
                        return new CanIContinueRunScenarioEvenThoughIFoundError(this) { Children = new List<IAnswer>() {new ExceptionAnswer(e)} };
                    }

                    Interpreter.OnScenarioFinished(new ScenarioFinishedEventArgs(Plan.ToList(), FileName));
                    Iterator.Index = 0;
                    throw;
                }

            }

            Interpreter.OnScenarioFinished(new ScenarioFinishedEventArgs(Plan.ToList(), FileName));

            Iterator.Index = 0;

            return null;
        }

        private object StepIntoScenario()
        {
            var list = new List<IKeyword>(Plan);

            list.Add(new StepOut(Interpreter.Plan, Interpreter.Iterator.Index, FileName));

            Interpreter.Plan = list;

            Interpreter.Iterator.Index = this.Iterator.Index;

            return new InformativeAnswer("Stepping into Sub-Scenario. Call 'StepOut' to return to main scenario.");
        }

        public override string ToString()
        {
            //return $"Run Scenario {FileName ?? ""} ({Keywords.Count}) steps: \n\t{string.Join("\n\t", Keywords.Use(Math.Min(Keywords.Count, 5)))} {(Keywords.Count>5?"\n\t...("+(Keywords.Count-5)+" more)...":"")}";
            return $"Run Scenario {FileName ?? ""}";
        }

        public Technique Technique { get; set; }
    }

    public class CanIContinueRunScenarioEvenThoughIFoundError : FollowupQuestion
    {
        private string _message;

        public CanIContinueRunScenarioEvenThoughIFoundError(RunScenario runScenario)
        {
            Options = new List<IKeyword>()
            {
                new Yes(() => runScenario.Execute()),
                new No(() =>
                {
                    runScenario.Interpreter.OnScenarioFinished(new ScenarioFinishedEventArgs(runScenario.Plan.ToList(), runScenario.FileName));
                    runScenario.Iterator.Index = 0;
                    return "OK.";
                }), 
            };

            _message =
                $"Error occured in RunScenario step {runScenario.Iterator.Index}:\n\t{runScenario.Iterator.Current}.\n Should I continue with step {runScenario.Iterator.Index + 1}? (Yes / No / Show)\n";
        }

        public override object Body {
            get
            {
                return _message;
            }
        }
    }

    public class Yes : Keyword
    {
        private readonly Func<object> _func;

        public Yes()
        {
        }

        public Yes(Func<object> func)
        {
            _func = func;
        }

        public override object DoRun()
        {
            if (_func == null)
            {
                return "Sorry, not understood";
            }

            return _func.Invoke();
        }
    }

    public class No : Keyword
    {
        private readonly Func<object> _func;

        public No()
        {
        }

        public No(Func<object> func)
        {
            _func = func;
        }

        public override object DoRun()
        {
            if (_func == null)
            {
                return "Sorry, not understood";
            }

            return _func.Invoke();
        }
    }
}