using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Gears.Interpreter.Core.Interpretation
{
    public class CriticalFailure : Exception, IInformativeAnswer, INegativeAnswer
    {
        public CriticalFailure()
        {
            
        }

        public CriticalFailure(string message) : base(message)
        {
            Text = message;
        }

        public CriticalFailure(string message, Exception innerException) : base(message, innerException)
        {
            Text = message;
        }

        protected CriticalFailure([NotNull] SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }



        public List<IAnswer> Children => new List<IAnswer>()
        {
            new CriticalFailure(this.StackTrace)
        };

        public string Text { get; }
        public object Body => Message;

    }
}