using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gears.Interpreter.Core.Interpretation;
using NUnit.Framework;

namespace Gears.Interpreter.Tests
{
    public static class Should
    {
        public static void Be<T>(IAnswer answer)
        {
            Assert.IsInstanceOf<T>(answer, answer.Text);
        }
    }
}
