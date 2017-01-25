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
using System.Xml.Serialization;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Data.Serialization.Mapping.LazyResolving;

namespace Gears.Interpreter.Data.Serialization.Mapping
{
    public interface IDictionaryToObjectMapper
    {
        object CreateObject(Type type, IDictionary<string, object> data);
      
        List<PropertyInfo> GetTypeProperties(Type type);
    }

    public class DictionaryToObjectMapper : IDictionaryToObjectMapper
    {
        private readonly ILazyExpressionResolver _lazyExpressionResolver;

        public DictionaryToObjectMapper(ILazyExpressionResolver lazyExpressionResolver)
        {
            _lazyExpressionResolver = lazyExpressionResolver;
        }

        public object CreateObject(Type type, IDictionary<string, object> data)
        {
            IDictionary<string, object> lazyValues = null;

            if (HasToBeLazyEvaulated(data))
            {
                RemoveLazyValues(data, out lazyValues, out data);
                AssertTypeCanBeProxied(type, lazyValues);
            }

            try
            {
                var matchingConstructor = GetMatchingConstructor(type, data);

                var values = MapValues(data, matchingConstructor.GetParameters()).ToArray();
                
                var objectInstance = matchingConstructor.Invoke(values);

                var properyData = RemoveMappedEntries(data, matchingConstructor.GetParameters());

                PopulateProperties(objectInstance, properyData);

                if (lazyValues != null)
                {
                    objectInstance = _lazyExpressionResolver.ToProxy(type, objectInstance, lazyValues);
                }
                
                return objectInstance;
            }
            catch (Exception ex)
            {
                return new CorruptObject {Description = type.Name, Exception = ex};
            }
        }

        

        private void AssertTypeCanBeProxied(Type type, IDictionary<string, object> lazyValues)
        {
            var properties = type.GetProperties();
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.SetMethod == null || (!propertyInfo.SetMethod.IsVirtual && lazyValues.ContainsKey(propertyInfo.Name.ToLower())))
                {
                    throw new ArgumentException($"Property {propertyInfo.Name} of {type.Name} cannot be lazy-evaluated because it is not declared virtual.");
                }
            }
        }

        private void RemoveLazyValues(IDictionary<string, object> data, out IDictionary<string, object> lazyValues,  out IDictionary<string, object> nonLazyValues)
        {
            lazyValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
            nonLazyValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var key in data.Keys)
            {
                if (_lazyExpressionResolver.CanResolve(data[key]))
                {
                    lazyValues.Add(key, data[key]);
                }
                //else
                //{
                    nonLazyValues.Add(key, data[key]);
                //}
            }
        }

        private bool HasToBeLazyEvaulated(IDictionary<string, object> data)
        {
            return data.Values.Any(x => _lazyExpressionResolver.CanResolve(x));
        }

        private List<object> MapValues(IDictionary<string, object> data, ParameterInfo[] parameterInfos)
        {
            var ctorParameters = new List<object>();
            foreach (var parameterInfo in parameterInfos)
            {
                var parameter = Convert(parameterInfo.ParameterType, data[parameterInfo.Name]);
                ctorParameters.Add(parameter);
            }
            return ctorParameters;
        }

        public List<PropertyInfo> GetTypeProperties(Type type)
        {
            return type.GetProperties().Where(p => !p.GetCustomAttributes(false).Any(a => a is XmlIgnoreAttribute)).ToList();
        }

        private void PopulateProperties(object @object, IDictionary<string, object> data)
        {
            var properties = GetTypeProperties(@object.GetType());

            foreach (var propertyInfo in properties)
            {
                if (CanSetProperty(data, propertyInfo))
                {
                    var value = Convert(propertyInfo.PropertyType, data[propertyInfo.Name]);
                    propertyInfo.SetValue(@object, value, null);
                }
            }
        }

        private static bool CanSetProperty(IDictionary<string, object> data, PropertyInfo propertyInfo)
        {
            if (!data.ContainsKey(propertyInfo.Name))
                return false;

            if (data[propertyInfo.Name] == null)
                return false;

            if (data[propertyInfo.Name] as string == string.Empty)
                return false;

            return propertyInfo.CanWrite;
        }

        private ConstructorInfo GetMatchingConstructor(Type type, IDictionary<string, object> data)
        {
            var contructors = type.GetConstructors().OrderByDescending(c => c.GetParameters().Count());

            foreach (var constructorInfo in contructors)
            {
                var parameterInfos = constructorInfo.GetParameters();

                if (AllConstructorParamsHaveData(parameterInfos, data))
                {
                    return constructorInfo;
                }
            }

            throw new ApplicationException($"Cannot read type {type.Name} with given parameters: {string.Join(",",data.Keys)}");
        }

        private object Convert(Type targetType, object value)
        {
            if (value.GetType() == targetType)
            {
                return value;
            }

            try
            {
                int o;
                if (targetType == typeof(DateTime) && int.TryParse(value as string, out o))
                {
                    var excelDate = DateTime.FromOADate(o);
                    return System.Convert.ChangeType(excelDate, targetType);
                }
            
                if (targetType.IsEnum)
                {
                    return Enum.Parse(targetType, value.ToString());
                }

                return System.Convert.ChangeType(value, targetType);
            }
            catch (Exception e)
            {
                // Expression values don't have to match
                if (_lazyExpressionResolver.CanResolve(value))
                {
                    return null;
                }

                throw new Exception($"Cannot convert value '{value}' to destination type '{targetType.Name}'", e);
            }
        }
        
        private IDictionary<string, object> RemoveMappedEntries(IDictionary<string, object> data, ParameterInfo[] parameterInfos)
        {
            var newData = new Dictionary<string, object>(data, StringComparer.InvariantCultureIgnoreCase);

            foreach (var parameterInfo in parameterInfos)
            {
                newData.Remove(parameterInfo.Name);
            }

            return newData;
        }

        private bool AllConstructorParamsHaveData(ParameterInfo[] parameters, IDictionary<string, object> data)
        {
            return parameters.All(parameterInfo => data.ContainsKey(parameterInfo.Name) && data[parameterInfo.Name] != null);
        }
    }

}
