﻿namespace Gears.Interpreter.Core.Adapters.UI
{
    public interface IInstructed
    {
        void MapRichSyntaxToSemantics(WebElementInstruction instruction);
    }
}