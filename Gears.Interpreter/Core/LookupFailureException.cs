using System;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Core
{
    public class LookupFailureException : ApplicationException
    {
        public ILookupResult LookupResult { get; set; }

        public LookupFailureException(ILookupResult lookupResult, string message) : base(message)
        {
            LookupResult = lookupResult;
        }
    }
}