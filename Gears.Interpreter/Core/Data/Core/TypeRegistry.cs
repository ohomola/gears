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
using System.Reflection;

namespace Gears.Interpreter.Core.Data.Core
{
    public interface ITypeRegistry
    {
        Type FirstOrDefault(string typeName);

        void Register(Type t);
        List<Type> Register(string file);
        List<Type> Types { get; }
    }

    [Obsolete("Merged with language", true)]
    public class TypeRegistry : ITypeRegistry
    {
        private List<Type> _types = new List<Type>();
        public List<Type> Types
        {
            get
            {
                if (!_types.Any())
                {
                    _types = this.GetType().Assembly.GetTypes()
                        .Where(x => x.Namespace != null && IsRegisteredType(x)).ToList();
                }

                _types.Add(typeof(Include));

                return _types;
            }
        }

        private static bool IsRegisteredType(Type x)
        {
            return x.Namespace.Contains("Library") || x.Namespace.Contains("ConfigObject");
        }

        public void Register(Type t)
        {
            Types.Add(t);
        }

        public List<Type> Register(string file)
        {
            var returnValue = new List<Type>();

            var assembly = Assembly.LoadFrom(file);

            if (assembly == null)
            {
                throw new ArgumentException($"Could not load plugin file '{file}'.");
            }

            foreach (var t in assembly.GetExportedTypes())
            {
                if (!t.IsClass || t.IsNotPublic)
                {
                    continue;
                }

                if (typeof(IGearsPlugin).IsAssignableFrom(t))
                {
                    Types.Add(t);
                    returnValue.Add(t);
                }
            }

            return returnValue;
        }

        public Type FirstOrDefault(string typeName)
        {
            return Types.FirstOrDefault(x => x.Name.ToLower() == typeName.ToLower());
        }
    }
}