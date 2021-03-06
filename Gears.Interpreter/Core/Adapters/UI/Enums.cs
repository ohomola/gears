﻿using System.ComponentModel;

namespace Gears.Interpreter.Core.Adapters.UI
{
    public enum WebElementType
    {
        Any = 0,
        Button,
        Input,
        Link
    }

    public enum SearchDirection
    {

        RightFromAnotherElementInclusiveOrAnywhereNextTo = 0,
        [Description("right from")]
        RightFromAnotherElement,
        [Description("left from")]
        LeftFromAnotherElement,
        [Description("above")]
        AboveAnotherElement,
        [Description("below")]
        BelowAnotherElement,
        [Description("from right")]
        LeftFromRightEdge,
        [Description("from left")]
        RightFromLeftEdge,
        [Description("from top")]
        DownFromTopEdge,
        [Description("from bottom")]
        UpFromBottomEdge
    }
}
