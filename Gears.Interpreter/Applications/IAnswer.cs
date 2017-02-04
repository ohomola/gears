using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Gears.Interpreter.Data;
using Gears.Interpreter.Library;
using JetBrains.Annotations;
using OpenQA.Selenium;

namespace Gears.Interpreter.Applications
{
    public interface IAnswer
    {
        object Body { get; }
        List<IAnswer> Children { get; }
        string Text { get; }
    }

    public interface IFollowupQuestion : IAnswer
    {
    }

    public interface IAnswerChoice
    {
        string Text { get; set; }
    }

    public interface IInformativeAnswer : IAnswer
    {
    }

    public class InformativeAnswer : IInformativeAnswer
    {
        public object Body { get; }
        public List<IAnswer> Children { get; set; } = new List<IAnswer>();
        public string Text => Body?.ToString();

        public InformativeAnswer(object response)
        {
            Body = response;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}";
        }

        public InformativeAnswer With(IInformativeAnswer childAnswer)
        {
            Children.Add(childAnswer);

            return this;
        }
    }


    public class ProblemDescriptionAnswer : InformativeAnswer, INegativeAnswer
    {
        public ProblemDescriptionAnswer(object response) : base(response)
        {
        }
    }

    public class InformativeDetailAnswer : InformativeAnswer
    {
        public InformativeDetailAnswer(object response) : base(response)
        {
        }
    }

    public class ExceptionAnswer : InformativeAnswer, INegativeAnswer
    {
        public ExceptionAnswer(object response) : base(response)
        {
        }
    }

    public class WarningAnswer : InformativeAnswer, INegativeAnswer
    {
        public WarningAnswer(object response) : base(response)
        {
        }
    }

    public class SuccessAnswer : InformativeAnswer
    {
        public SuccessAnswer(object response) : base(response)
        {
        }
    }

    public class ResultAnswer : InformativeAnswer
    {
        public int Code { get; set; }

        public ResultAnswer(int code) : base(code)
        {
            Code = code;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(Code)}: {Code}";
        }
    }

    public class CriticalFailure : Exception, IInformativeAnswer, INegativeAnswer
    {
        public CriticalFailure()
        {
            
        }

        public CriticalFailure(string message) : base(message)
        {
        }

        public CriticalFailure(string message, Exception innerException) : base(message, innerException)
        {
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

    public interface INegativeAnswer
    {
        
    }
    public class StatusAnswer : IAnswer
    {
        public StatusAnswer(IEnumerable<IKeyword> keywords, int index, IDataContext data)
        {
            Keywords = keywords;
            Index = index;
            Data = data;
        }

        public IEnumerable<IKeyword> Keywords { get; set; }
        public int Index { get; set; }
        public IDataContext Data { get; set; }
        public object Body { get; }
        public List<IAnswer> Children { get; }
        public string Text { get; }
    }
}