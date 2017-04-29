using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications
{
    public class Iterator<TItem>
    {
        public int Index { get; set; } =0;

        private readonly IHavePlan _interpreter;
        private readonly Func<IHavePlan, IEnumerable<TItem>> _func;

        public Iterator(IHavePlan interpreter, Func<IHavePlan, IEnumerable<TItem>> func)
        {
            _interpreter = interpreter;
            _func = func;
        }

        public TItem Current => _func.Invoke(_interpreter).ElementAt(Index);
        public TItem Previous => _func.Invoke(_interpreter).ElementAt(Math.Max(0,Index-1));


        public bool MoveNext()
        {
            if (IsEndOfList())
            {
                return false;
            }

            Index++;
            return true;
        }

        public bool IsEndOfList()
        {
            return !HasItem(Index);
        }

        public int MoveNext(int count)
        {
            var moved = 0;
            for (int i = 0; i < count; i++)
            {
                moved += MoveNext()?1:0;
            }

            return moved;
        }

        public int MoveBack(int count)
        {
            var moved = Math.Min(Index, count);

            Index = Index - moved;

            return moved;
        }

        public bool PeekNext()
        {
            return HasItem(Index+1);
        }

        private bool HasItem(int index)
        {
            return _func.Invoke(_interpreter).ElementAtOrDefault(index) != null;
        }

        
    }
}