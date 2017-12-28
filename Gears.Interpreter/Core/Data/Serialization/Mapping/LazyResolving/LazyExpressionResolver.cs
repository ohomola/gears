using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;

namespace Gears.Interpreter.Core.Data.Serialization.Mapping.LazyResolving
{
    public interface ILazyExpressionResolver
    {
        object Resolve(object obj);
        bool CanResolve(object obj);
        object ToProxy(Type type, object objectInstance, IDictionary<string, object> lazyValues);
        object ToProxy(Type type, object objectInstance);
    }

    public class LazyExpressionResolver : ILazyExpressionResolver
    {
        private readonly ICodeStubResolver _codeStubResolver;
        private readonly IRememberedDataResolver _rememberedDataResolver;

        public LazyExpressionResolver(ICodeStubResolver codeStubResolver, IRememberedDataResolver rememberedDataResolver)
        {
            _codeStubResolver = codeStubResolver;
            _rememberedDataResolver = rememberedDataResolver;
        }

        public object Resolve(object obj)
        {
            if (_rememberedDataResolver.CanResolve(obj))
            {
                obj = _rememberedDataResolver.Resolve(obj as string);
            }

            if (_codeStubResolver.CanResolve(obj))
            {
                return _codeStubResolver.Resolve(obj as string);
            }
            
            return obj;
        }

        public bool CanResolve(object obj)
        {
            return _rememberedDataResolver.CanResolve(obj) || _codeStubResolver.CanResolve(obj);
        }

        public object ToProxy(Type type, object objectInstance, IDictionary<string, object> lazyValues)
        {
            return new ProxyGenerator().CreateClassProxyWithTarget(type, objectInstance, new ResolveAllStringParameterReferencesInProperties(lazyValues, this));
        }

        public object ToProxy(Type type, object objectInstance)
        {
            IDictionary<string, object> lazyValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var propertyInfo in type.GetProperties().Where(x=>x.CanRead && x.CanWrite))
            {
                if (this.CanResolve(propertyInfo.GetValue(objectInstance)))
                {
                    if (!propertyInfo.GetMethod.IsVirtual)
                    {
                        throw new ArgumentException($"Cannot use expression here. Property {propertyInfo.Name} of {type.Name} is not virtual.");
                    }
                    lazyValues.Add(propertyInfo.Name, propertyInfo.GetValue(objectInstance));
                }
            }
            return new ProxyGenerator().CreateClassProxyWithTarget(type, objectInstance, new ResolveAllStringParameterReferencesInProperties(lazyValues, this));
        }
    }

    
}