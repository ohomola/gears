using System;

namespace Gears.Interpreter.Core.Adapters.UI.Lookup
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