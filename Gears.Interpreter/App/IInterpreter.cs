using System;
using System.Collections.Generic;
using Gears.Interpreter.App.Workflow;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App
{
    public interface IInterpreter : IHavePlan
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

        event EventHandler<ScenarioEventArgs> ScenarioFinished;
        void OnScenarioFinished(ScenarioEventArgs e);
        string GetNextInstruction();
        IEnumerable<IKeyword> GetLoggedKeywords();
        void AddToPlan(IKeyword keyword);
        event EventHandler<StepEventArgs> StepFinished;
        event EventHandler<StepEventArgs> StepStarted;
    }
}