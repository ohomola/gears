using System.Collections.Generic;

namespace Gears.Interpreter.Library
{
    public interface IInstructed
    {
        void MapSyntaxToSemantics(Instruction instruction);
    }
}