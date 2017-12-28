using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    public class Start : Keyword, IProtected
    {
        public override object DoRun()
        {
            var eventHandlers = Data.GetAll<IApplicationEventHandler>();
            RegisterEventHandlers(eventHandlers);

            //Interpreter.Plan = Data.GetAll<Keyword>().ToList();

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

        private void RegisterEventHandlers(IEnumerable<IApplicationEventHandler> applicationEventHandlers)
        {
            foreach (var handler in applicationEventHandlers)
            {
                handler.Register(Interpreter);
            }
        }
    }
}