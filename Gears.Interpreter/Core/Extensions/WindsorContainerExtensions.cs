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
using System.Linq;
using System.Reflection;
using Castle.Windsor;

namespace Gears.Interpreter.Core.Extensions
{
    public static class WindsorContainerExtensions
    {
        public static T Resolve<T>(this WindsorContainer container, T existingInstance)
        {
            //var propertyInfos = existingInstance.GetType().GetProperties();
            var propertyInfos = existingInstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldInfos = existingInstance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetCustomAttributes(true).Any(attr => attr is WireAttribute) && container.Kernel.HasComponent(propertyInfo.PropertyType))
                {
                    propertyInfo.SetValue(existingInstance, container.Resolve(propertyInfo.PropertyType), new object[] { });
                }
            }

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.GetCustomAttributes(true).Any(attr => attr is WireAttribute) && container.Kernel.HasComponent(fieldInfo.FieldType))
                {
                    fieldInfo.SetValue(existingInstance, container.Resolve(fieldInfo.FieldType));
                }
            }

            return existingInstance;
        }
    }

    public class WireAttribute : Attribute
    {
    }
}
