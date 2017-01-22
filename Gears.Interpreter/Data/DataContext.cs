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
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Data
{
    public interface IDataContext : IDataObjectAccess
    {
        List<IDataObjectAccess> DataAccesses { get; set; }
        void Include(IDataObjectAccess dataAccess);
        void Include(string fileName);
        void Exclude(IDataObjectAccess dataAccess);
    }

    public class DataContext : IDataContext
    {
        public static object EntityWasNotFoundNullObject = null;

        public DataContext(): this(new List<IDataObjectAccess>())
        {
        }

        public DataContext(params IDataObjectAccess[] dataSources)
        {
            DataAccesses = dataSources.ToList();
        }

        public DataContext(IEnumerable<IDataObjectAccess> dataSources)
        {
            DataAccesses = dataSources.ToList();
        }

        public List<IDataObjectAccess> DataAccesses { get; set; }

        public void Include(IDataObjectAccess dataAccess)
        {
            if (DataAccesses.Contains(dataAccess))
            {
                throw new ArgumentException($"Already included data: \n{dataAccess}\nat{new System.Diagnostics.StackTrace()}");
            }

            DataAccesses.Add(dataAccess);
        }

        public void Include(string fileName)
        {
            Include(new FileObjectAccess(fileName, ServiceLocator.Instance.Resolve<ITypeRegistry>()));
        }

        public void Exclude(IDataObjectAccess dataAccess)
        {
            if (!DataAccesses.Contains(dataAccess))
            {
                throw new ArgumentException($"Cannot exclude. File is not included: \n{dataAccess}\nat{new System.Diagnostics.StackTrace()}");
            }

            DataAccesses.Remove(dataAccess);
        }

        public T Get<T>() where T : class
        {
            T selectedClass = DataAccesses.Select(da => da.Get<T>()).FirstOrDefault(entity => entity != EntityWasNotFoundNullObject);

            return selectedClass;
        }

        public object Get(Type t)
        {
            foreach (var da in DataAccesses)
            {
                var entity = da.Get(t);
                if (entity != EntityWasNotFoundNullObject) return entity;
            }
            return null;
        }

        public bool Contains<T>()where T : class
        {
            foreach (var da in DataAccesses)
            {
                if (da.Contains<T>()) return true;
            }
            return false;
        }

        public void Add<T>(T obj) where T : class
        {
            if (!DataAccesses.OfType<ObjectDataAccess>().Any())
            {
                DataAccesses.Add(new ObjectDataAccess());
            }

            var dataAccess = DataAccesses.OfType<ObjectDataAccess>().First();
            dataAccess.Add(obj);
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            foreach (IDataObjectAccess da in DataAccesses)
                foreach (T unknown in da.GetAll<T>())
                {
                    yield return unknown;        
                }
        }

        public IEnumerable<object> GetAll(Type t)
        {
            foreach (IDataObjectAccess da in DataAccesses)
                foreach (object o in da.GetAll(t))
                    yield return o;
        }

        public IEnumerable<object> GetAll()
        {
            foreach (IDataObjectAccess da in DataAccesses)
                foreach (object o in da.GetAll())
                    yield return o;
        }

        public void RemoveAll<T>()
        {
            foreach (var oda in DataAccesses.OfType<ObjectDataAccess>())
            {
                oda.RemoveAll<T>();
            }
        }

        public bool Contains(Type t)
        {
            foreach (var da in DataAccesses)
            {
                if (da.Contains(t)) return true;
            }
            return false;
        }

        public override string ToString()
        {
            return "Data context ({0} items):\n\n\t--\t".FormatWith(DataAccesses.Count().ToString()) + string.Join("\n\n\t--\t", this.DataAccesses);
        }

        public void AddRange(IEnumerable<object> dataObjects)
        {
            if (!DataAccesses.OfType<ObjectDataAccess>().Any())
            {
                DataAccesses.Add(new ObjectDataAccess());
            }

            foreach (var dataObject in dataObjects)
            {
                DataAccesses.First().Add(dataObject);
            }
        }
        
    }
}
