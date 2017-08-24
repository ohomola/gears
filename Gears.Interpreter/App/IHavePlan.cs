using System.Collections.Generic;
using Gears.Interpreter.Core;

namespace Gears.Interpreter.App
{
    public interface IHavePlan
    {
        IEnumerable<IKeyword> Plan { get; set; }
    }
}