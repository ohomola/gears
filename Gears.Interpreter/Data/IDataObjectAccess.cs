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

namespace Gears.Interpreter.Data
{
    public interface IDataObjectAccess
    {
        void Add<T>(T obj) where T : class;

        void AddRange(IEnumerable<object> dataObject);

        T Get<T>() where T :class;
        object Get(Type t);

        bool Contains<T>() where T : class;
        bool Contains(Type t);
        
        IEnumerable<T> GetAll<T>() where T : class;
        IEnumerable<object> GetAll(Type t);
        IEnumerable<object> GetAll();
        void RemoveAll<T>();
        void RemoveAll(Type t);
        void Remove(object obj);
    }
}