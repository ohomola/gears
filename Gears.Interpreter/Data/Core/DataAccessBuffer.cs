#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears, a software automation and assistance framework.

Gears is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Gears is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gears.Interpreter.Data.Core
{
    public interface IDataAccessBuffer
    {
        void Add(object data);
        void AddRange(IEnumerable<object> data);
        T Get<T>() where T : class;
        object Get(Type t);
        bool Has<T>() where T : class;
        bool Has(Type t);
        IEnumerable<object> GetAll(Type type);
        IEnumerable<object> GetAll();
        bool Has<T>(Func<T,bool> predicate);
        void Wipe();
        int Size { get; }
    }

    public class DataAccessBuffer : IDataAccessBuffer
    {
        private readonly List<object> _list;

        public DataAccessBuffer()
        {
            _list = new List<object>();
        }

        public int Size
        {
            get { return _list.Count; }
        }

        public void Add(object data)
        {
            _list.Add(data);
        }

        public void AddRange(IEnumerable<object> data)
        {
            _list.AddRange(data);
        }

        public T Get<T>() where T : class
        {
            return _list.OfType<T>().LastOrDefault();
        }

        public object Get(Type t)
        {
            return _list.FirstOrDefault(t.IsInstanceOfType);
        }

        public bool Has<T>() where T : class
        {
            return _list.Any(typeof(T).IsInstanceOfType);
        }

        public bool Has(Type t)
        {
            return _list.Any(t.IsInstanceOfType);
        }

        public IEnumerable<object> GetAll(Type type)
        {
            return _list.Where(type.IsInstanceOfType);
        }

        public IEnumerable<object> GetAll()
        {
            return _list;
        }

        public bool Has<T>(Func<T, bool> predicate)
        {
            return _list.OfType<T>().Any(predicate);
        }

        public void Wipe()
        {
            _list.Clear();
        }

        public override string ToString()
        {
            return string.Join(",\n", _list);
        }
    }
}