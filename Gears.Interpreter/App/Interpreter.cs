using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Castle.Core;
using Gears.Interpreter.App.Workflow;
using Gears.Interpreter.App.Workflow.Library;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App
{
    public class Interpreter : IInterpreter, IDisposable
    {
        public const string RUN_NEXT_ITEM_IN_PLAN = "";

        public IDataContext Data { get; }
        public ILanguage Language { get; }
        
        private List<IKeyword> _plan = new List<IKeyword>();
        
        [DoNotWire]
        public IEnumerable<IKeyword> Plan
        {
            get => _plan;
            set => _plan = value.ToList();
        }

        public Iterator<IKeyword> Iterator { get; set; }

        [DoNotWire]
        public List<IKeyword> ExecutionHistory { get; set; } = new List<IKeyword>();

        public bool IsRunningSuite => Plan.Any(x => x is RunScenario);
        public bool IsDebugMode { get; set; }
        public bool IsAnalysis { get; set; }
        public bool IsAlive { get; set; } = true;


        public Interpreter(IDataContext data, ILanguage language)
        {
            Data = data;
            Language = language;
            Plan = Data.GetAll<Keyword>().ToList();
            Iterator = new Iterator<IKeyword>(this, x => x.Plan);
        }


        #region Events

        public event EventHandler<ScenarioFinishedEventArgs> ScenarioFinished;

            public virtual void OnScenarioFinished(ScenarioFinishedEventArgs e)
            {
                ScenarioFinished?.Invoke(this, e);
            }

            public event EventHandler<ScenarioFinishedEventArgs> SuiteFinished;

            public virtual void OnSuiteFinished(ScenarioFinishedEventArgs e)
            {
                SuiteFinished?.Invoke(this, e);
            }

            public event EventHandler<StepFinishedEventArgs> StepFinished;

            public virtual void OnStepFinished(StepFinishedEventArgs e)
            {
                StepFinished?.Invoke(this, e);
            }

        #endregion


        public IAnswer Please(string command)
        {
            if (!IsAlive)
            {
                throw new InvalidOperationException("Interpreter is stopped.");
            }

            IKeyword keyword;

            try
            {
                if (string.IsNullOrEmpty(command) && !Iterator.IsEndOfList())
                {
                    keyword = Iterator.Current;
                    Iterator.MoveNext();
                }
                else if (Language.HasKeywordFor(command))
                {
                    keyword = Language.ResolveKeyword(command);
                }
                else if (!string.IsNullOrEmpty(command))
                {
                    return new ExceptionAnswer($"Sorry, '{command}' is not a recognized command.");
                }
                else
                {
                    keyword = new Stop();
                }

                ExecutionHistory.Add(keyword);

                var result = keyword.Execute();

                OnStepFinished(new StepFinishedEventArgs(keyword));

                if (result != null && result.Equals(KeywordResultSpecialCases.Skipped))
                {
                    return new InformativeAnswer($"Skipping {keyword}");
                }

                if (result is IAnswer)
                {
                    return (IAnswer) result;
                }

                return new InformativeAnswer(result);
            }

            catch (CriticalFailure e)
            {
                return e;
            }
            catch (ApplicationException ae)
            {
                return new WarningAnswer("Step Failed")
                    .With(new WarningAnswer(ae.Message));
            }
            catch (Exception exception)
            {
                return new ExceptionAnswer($"Unexpected error \n {exception.Message}")
                    .With(new ExceptionAnswer(exception.StackTrace));
            }
        }

        public string GetNextInstruction()
        {
            if (UserInteropAdapter.IsKeyDown(Keys.Escape))
            {
                IsDebugMode = true;
            }
            if (Data.Contains<RunFirstStep>())
            {
                Data.RemoveAll<RunFirstStep>();
                return RUN_NEXT_ITEM_IN_PLAN;
            }
            else if (IsDebugMode)
            {
                UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());

                return Console.ReadLine();
            }
            else
            {
                return RUN_NEXT_ITEM_IN_PLAN;
            }
        }

        public IAnswer RunOnYourOwn()
        {
            var result = Please("start");

            while (IsAlive)
            {
                if (result is CriticalFailure)
                {
                    return result;
                }

                result = Please(string.Empty);
            }

            return result;
        }

        public void AddToPlan(IKeyword keyword)
        {
            _plan.Add(keyword);
        }

        public IEnumerable<IKeyword> GetLoggedKeywords()
        {
            return Plan.Any() ? Plan.Where(Keyword.IsLogged) : ExecutionHistory.Where(Keyword.IsLogged);
        }

        public void Dispose()
        {
        }
    }
}
