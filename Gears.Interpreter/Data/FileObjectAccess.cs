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
using System.Dynamic;
using System.IO;
using System.Linq;
using Castle.DynamicProxy;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Data.Serialization;
using Microsoft.CSharp.RuntimeBinder;

namespace Gears.Interpreter.Data
{
    public class TempFileObjectAccess : FileObjectAccess
    {
        public TempFileObjectAccess(string fileName, ITypeRegistry typeRegistry) : base(System.IO.Path.GetTempPath() + System.IO.Path.PathSeparator + fileName, typeRegistry)
        {
        }
    }

    public class FileObjectAccess : IDataObjectAccess
    {
        [Wire]
        public ITypeRegistry TypeRegistry { get; set; }

        private readonly IDataAccessBuffer _buffer;

        public string Path { get; set; }
        
        public FileObjectAccess(string path, ITypeRegistry typeRegistry)
        {
            Path = path;
            _buffer = new DataAccessBuffer();

            TypeRegistry = typeRegistry;
        }



        public IDataAccessBuffer LazyLoadBuffer()
        {
            if (_buffer.Size == 0)
            {
                if (File.Exists(Path))
                {
                    var loadRange = ReadAllObjects();
                    _buffer.AddRange(loadRange);
                }
                else
                {
                    throw new FileNotFoundException($"File {Path} does not exist");
                }
            }
            return _buffer;
        }

        public void ForceReload()
        {
            this._buffer.Wipe();
            LazyLoadBuffer();
        }

        public T Get<T>() where T : class
        {
            return (T) Get(typeof (T));
        }

        public object Get(Type t)
        {
            return LazyLoadBuffer().Get(t);
        }

        public bool Contains<T>() where T : class
        {
            return Contains(typeof (T));
        }

        public bool Contains(Type t)
        {
            return LazyLoadBuffer().Has(t);
        }

        public void Add<T>(T obj) where T : class
        {
            AddRange(new [] {obj});
        }

        public void AddRange(IEnumerable<object> objects)
        {
            if (File.Exists(Path))
            {
                LazyLoadBuffer().Wipe();
            }

            if (!File.Exists(Path))
            {
                Append(Enumerable.Empty<object>());
            }

            var existingContent = ReadAllObjects().ToList();

            var newContent = objects.Select(UnwrapProxy).ToList();
            newContent.AddRange(existingContent);
            Append(newContent);
        }

        internal static TType UnwrapProxy<TType>(TType proxy)
        {
            if (!ProxyUtil.IsProxy(proxy))
                return proxy;

            try
            {
                dynamic dynamicProxy = proxy;
                return dynamicProxy.__target;
            }
            catch (RuntimeBinderException)
            {
                return proxy;
            }
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            return GetAll(typeof (T)).Cast<T>();
        }

        public IEnumerable<object> GetAll(Type t)
        {
            return LazyLoadBuffer().GetAll(t);
        }

        public IEnumerable<object> GetAll()
        {
            return LazyLoadBuffer().GetAll();
        }

        public void RemoveAll<T>()
        {
            throw new NotImplementedException();
        }

        public void RemoveAll(Type t)
        {
            throw new NotImplementedException();
        }

        public bool CanAdd<T>() where T : class
        {
            return true;
        }

        public bool CanAdd(Type type)
        {
            return true;
        }
        
        public override string ToString()
        {
            return
                $"File \'{System.IO.Path.GetFileName(Path)}\'\tat: '{this.Path}'" +
                $"\n\t\tContent  {(_buffer.Size > 0 ? $"({_buffer.Size.ToString()} buffered objects)" : "")}{(_buffer.Size > 0 ? ": \n\t\t\t - " + string.Join("\n\t\t\t - ", _buffer.GetAll()) : "")}";
        }
        
        public override bool Equals(object obj)
        {
            var o = obj as FileObjectAccess;

            if (o == null)
            {
                return false;
            }

            return this.Path.Equals(o.Path);
        }

        public override int GetHashCode()
        {
            return this.Path.GetHashCode();
        }

        public IEnumerable<object> ReadAllObjects()
        {
            var types = TypeRegistry.GetDTOTypes().OrderBy(x => x.Name).ToList();

            if (!types.Any(x => x.Equals(typeof(Include))))
            {
                types.Add(typeof(Include));
            }

            types = types.Union(GetSubTypes(types)).ToList();

            using (ISerializer adapter = new SerializerFactory().GetSerializerByPath(Path))
            {
                try
                {
                    var result = new List<object>();

                    foreach (var o in adapter.Deserialize())
                    {
                        if (o is Include)
                        {
                            result.AddRange(((Include)o).RecursiveRead(Path));
                        }
                        else
                        {
                            result.Add(o);
                        }
                    }

                    return result;
                }
                catch (Exception e)
                {
                    throw new IOException($"Exception occured when reading file {Path}\n{e.GetAllMessages()}", e);
                }
            }
        }

        public void Delete()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }

        public void Write(IEnumerable<object> dataObjects)
        {
            Delete();
            Append(dataObjects);
        }

        public void Write(object obj)
        {
            Write(new[] {obj});
        }

        public void Append(IEnumerable<object> dataObjects)
        {
            ISerializer serializer = null;

            try
            {
                serializer = new SerializerFactory().GetSerializerByPath(Path);
                serializer.Serialize(dataObjects);
            }
            finally
            {
                if (serializer is IDisposable)
                {
                    (serializer as IDisposable).Dispose();
                }
            }
        }

        protected IEnumerable<Type> GetSubTypes(IEnumerable<Type> knownTypes)
        {
            return knownTypes.SelectMany(x => x.Assembly.GetTypes().Where(x.IsAssignableFrom));
        }
    }
}