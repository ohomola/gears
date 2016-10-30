using System;

namespace Gears.Interpreter.Library
{
    public class LookupFailureException : ApplicationException
    {
        public LookupResult LookupResult { get; set; }

        public LookupFailureException(LookupResult lookupResult, string message) : base(message)
        {
            LookupResult = lookupResult;
        }
    }
}