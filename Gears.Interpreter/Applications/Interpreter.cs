using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.Config;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Applications
{
    public interface IInterpreter
    {
        bool IsAlive { get; set; }
        bool IsRunningSuite { get; }

        IEnumerable<IKeyword> Plan { get; set; }
        
        List<IKeyword> ExecutionHistory { get; set; }
        ILanguage Language { get; }
        Iterator<IKeyword> Iterator { get; set; }
        IDataContext Data { get; }
        bool IsDebugMode { get; set;  }
        bool IsAnalysis { get; set; }

        IAnswer Please(string command);

        IAnswer RunOnYourOwn();

        event EventHandler<ScenarioFinishedEventArgs> ScenarioFinished;
        void OnScenarioFinished(ScenarioFinishedEventArgs e);
        event EventHandler<ScenarioFinishedEventArgs> SuiteFinished;
        void OnSuiteFinished(ScenarioFinishedEventArgs e);
        string Continue();
        IEnumerable<IKeyword> GetLog();
        void AddToPlan(IKeyword keyword);
    }

    public class Interpreter : IInterpreter, IDisposable
    {
        public const string LastScenarioTempFilePath = "GearsLastScenarioTemp";

        private IDependencyReloader _reloader;
        public IDataContext Data { get; }
        public ILanguage Language { get; }
        private List<IKeyword> _plan = new List<IKeyword>();

        public Iterator<IKeyword> Iterator { get; set; } 

        public bool IsAlive { get; set; } = true;

        [DoNotWire]
        public IEnumerable<IKeyword> Plan
        {
            get { return _plan; }
            set
            {
                ValidatePlan(value);
                _plan = value.ToList();
            }
        }

        public void AddToPlan(IKeyword keyword)
        {
            _plan.Add(keyword);
        }

        private void ValidatePlan(IEnumerable<IKeyword> value)
        {
            
        }

        public bool IsRunningSuite => Plan.Any(x => x is RunScenario);

        public bool IsDebugMode { get; set; }

        public bool IsAnalysis { get; set; }

        [DoNotWire]
        public List<IKeyword> ExecutionHistory { get; set; } = new List<IKeyword>();

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

        public string Continue()
        {
            if (IsDebugMode)
            {
                UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());

                return Console.ReadLine();
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        public Interpreter(IDependencyReloader reloader, IDataContext data, ILanguage language)
        {
            _reloader = reloader;
            Data = data;
            Language = language;
            Iterator = new Iterator<IKeyword>(this, x => x.Plan);
        }

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

        public IEnumerable<IKeyword> GetLog()
        {
            return Plan.Any() ? Plan.Where(Keyword.IsLogged) : ExecutionHistory.Where(Keyword.IsLogged);
        }

        public void Dispose()
        {
        }
    }


    
}
