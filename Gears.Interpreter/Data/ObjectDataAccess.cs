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
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Data
{
    public class ObjectDataAccess : IDataObjectAccess
    {
        private readonly DataAccessBuffer _buffer;

        public ObjectDataAccess(params object[] obj) 
        {
            _buffer = new DataAccessBuffer();
            _buffer.AddRange(obj);
        }

        public T Get<T>(int id) where T : class
        {
            return _buffer.Get<T>();
        }

        public T Get<T>() where T : class
        {
            return (T) Get(typeof (T));
        }

        public object Get(Type t)
        {
            return _buffer.Get(t);
        }

        public bool Contains<T>() where T : class
        {
            return Contains(typeof (T));
        }

        public bool Contains(Type t)
        {
            return _buffer.Has(t);
        }

        public void Add<T>(T obj) where T : class
        {
            _buffer.Add(obj);
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            return GetAll(typeof (T)).Cast<T>();
        }

        public IEnumerable<object> GetAll(Type t)
        {
            if (!_buffer.Has(t))
            {
                return new List<object>();
            }
            return _buffer.GetAll(t);
        }

        public IEnumerable<object> GetAll()
        {
            return _buffer.GetAll();
        }
        
        public void AddRange(IEnumerable<object> objects)
        {
            foreach (var o in objects)
            {
                Add(o);
            }
        }

        public override string ToString()
        {
            return $"Buffered data ({(_buffer.Size)} objects) {(_buffer.Size > 0 ? ": \n\t\t   " + string.Join("\n\t\t== ", _buffer.GetAll().Select(x => x.ToString())) : "")}";
        }

    }
}