using System;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Core
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