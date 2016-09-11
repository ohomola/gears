using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gears.Interpreter.Library
{
    public class Wait : Keyword
    {
        public Wait(int what)
        {
            What = what;
        }

        public int What { get; set; }

        public override object Run()
        {
            Thread.Sleep(What);
            return null;
        }
    }
}
