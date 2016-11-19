using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Gears.Interpreter.Tests.Pages;

namespace Gears.Interpreter.Data.Serialization.Mapping
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
            return new ProxyGenerator().CreateClassProxyWithTarget(type, objectInstance, new DataObjectInterceptor(lazyValues, this));
        }

        public object ToProxy(Type type, object objectInstance)
        {
            IDictionary<string, object> lazyValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var propertyInfo in type.GetProperties())
            {
                if (this.CanResolve(propertyInfo.GetValue(objectInstance)))
                {
                    lazyValues.Add(propertyInfo.Name, propertyInfo.GetValue(objectInstance));
                }
            }
            return new ProxyGenerator().CreateClassProxyWithTarget(type, objectInstance, new DataObjectInterceptor(lazyValues, this));
        }
    }

    
}