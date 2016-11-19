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
    public interface ITypeRegistry
    {
        IEnumerable<Type> GetDTOTypes(bool includeAbstract = false);

        Type GetFirstDTOType(string typeName);

        void Register(Type t);
    }

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
                        .Where(x => x.Namespace != null && x.Namespace.Contains("Library")).ToList();
                }

                return _types;
            }
        }

        public IEnumerable<Type> GetDTOTypes(bool includeAbstract = false)
        {
            return Types;
        }

        public void Register(Type t)
        {
            Types.Add(t);
        }

        public Type GetFirstDTOType(string typeName)
        {
            return Types.FirstOrDefault(x => x.Name.ToLower() == typeName.ToLower());
        }

    }
}