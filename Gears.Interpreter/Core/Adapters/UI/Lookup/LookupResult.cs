using System.Collections.Generic;

namespace Gears.Interpreter.Core.Adapters.UI.Lookup
{
    public interface ILookupResult
    {
        IEnumerable<IBufferedElement> AllValidResults { get; set; }
        IBufferedElement MainResult { get; set; }
        bool Success { get; set; }
    }

    public class LookupResult : ILookupResult
    {
        public LookupResult(bool success)
        {
            Success = success;
        }

        public LookupResult(IEnumerable<IBufferedElement> allValidResults, IBufferedElement mainResult, bool success)
        {
            AllValidResults = allValidResults;
            MainResult = mainResult;
            Success = success;
        }

        public IEnumerable<IBufferedElement> AllValidResults { get; set; }
        public IBufferedElement MainResult { get; set; }
        public bool Success { get; set; }
    }
}