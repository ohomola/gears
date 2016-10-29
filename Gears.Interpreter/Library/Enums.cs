using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gears.Interpreter.Library
{
    public enum SubjectType
    {
        Any = 0,
        Button,
        Input,
        Link
    }

    public enum SearchDirection
    {
        RightFromAnotherElementInclusiveOrAnywhereNextTo = 0,
        RightFromAnotherElement,
        LeftFromAnotherElement,
        AboveAnotherElement,
        LeftFromRightEdge,
        RightFromLeftEdge,
        BelowAnotherElement,
        DownFromTopEdge,
        UpFromBottomEdge
    }
}
