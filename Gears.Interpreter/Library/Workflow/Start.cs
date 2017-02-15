using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library.Config;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    public class Start : Keyword
    {
        private IDataContext _data;

        public Start(IDataContext data)
        {
            _data = data;
        }

        public override object DoRun()
        {
            SharedObjectDataAccess.Instance = new Lazy<SharedObjectDataAccess>();

            var eventHandlers = _data.GetAll<IApplicationEventHandler>();
            RegisterEventHandlers(eventHandlers);

            Interpreter.Plan = _data.GetAll<Keyword>().ToList();

            //ThrowIfRunScenariosAreMixedWithStandardKeywords(Interpreter.IsRunningSuite, Interpreter.Plan);

            InformativeAnswer successAnswer = new SuccessAnswer("Initialization complete.\n");

            if (Interpreter.Data.DataAccesses.OfType<FileObjectAccess>().Any())
            {
                successAnswer.Children.Add(new SuccessAnswer($"Files registered : {Interpreter.Data.DataAccesses.OfType<FileObjectAccess>().Count()}\n"));
                //foreach (var foa in Interpreter.Data.DataAccesses.OfType<FileObjectAccess>())
                //{
                //    successAnswer.Children.Add(new SuccessAnswer($"\nData Access {foa}"));
                //}
            }

            if (eventHandlers.Any())
            {
                successAnswer.Children.Add(new SuccessAnswer($"Handlers registered : {eventHandlers.Count()}\n"));
                //foreach (var applicationEventHandler in eventHandlers)
                //{
                //    successAnswer.Children.Add(new SuccessAnswer($"\nRegistered {applicationEventHandler}"));
                //}
            }

            if (Interpreter.Data.GetAll<DebugMode>().Any() || Interpreter.Plan.IsNullOrEmpty())
            {
                Interpreter.IsDebugMode = true;
            }

            return successAnswer;
        }

        //private void ThrowIfRunScenariosAreMixedWithStandardKeywords(bool isRunningSuite, IEnumerable<IKeyword> keywords)
        //{
        //    if (isRunningSuite && !keywords.All(x => x is RunScenario))
        //    {
        //        throw new CriticalFailure("Scenario cannot contain RunScenario steps as well as basic Keywords.");
        //    }
        //}



        private void RegisterEventHandlers(IEnumerable<IApplicationEventHandler> applicationEventHandlers)
        {
            foreach (var handler in applicationEventHandlers)
            {
                handler.Register(Interpreter);
            }
        }
    }
}