using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Gears.Interpreter.Data.Serialization.Mapping.LazyResolving;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Data.Serialization.Mapping
{
    public class DataObjectInterceptor : StandardInterceptor
    {
        private IDictionary<string, object> _lazyValues;
        private readonly ILazyExpressionResolver _lazyExpressionResolver;
        private bool _isHydrated;

        public DataObjectInterceptor(IDictionary<string, object> lazyValues, ILazyExpressionResolver lazyExpressionResolver)
        {
            this._lazyValues = lazyValues;
            _lazyExpressionResolver = lazyExpressionResolver;
        }

        protected override void PreProceed(IInvocation invocation)
        {
            InterceptGetter(invocation);
            InterceptExecute(invocation);

            base.PreProceed(invocation);
        }

        private void InterceptExecute(IInvocation invocation)
        {
            if (!invocation.Method.Name.Contains("Execute")&& invocation.Method.Name!="Hydrate")
            {
                return;
            }

            foreach (var key in _lazyValues.Keys)
            {
                var newValue = _lazyExpressionResolver.Resolve(_lazyValues[key]);
                var propertyInfo = invocation.TargetType.GetProperty(key);
                var propertyInfoSetMethod = propertyInfo.SetMethod;
                if (propertyInfo.SetMethod == null)
                {
                    throw new ArgumentException($"{propertyInfo.Name} has no Setter");
                }
                propertyInfoSetMethod.Invoke(invocation.InvocationTarget, new[] { newValue });
                
            }
            if (invocation.InvocationTarget is Keyword)
            {
                (invocation.InvocationTarget as Keyword).IsHydrated = true;
            }
            _lazyValues = new Dictionary<string, object>();
        }

        private void InterceptGetter(IInvocation invocation)
        {
            var invokedPropertyName = invocation.Method.Name;

            if (invokedPropertyName.Contains("get_"))
            {
                invokedPropertyName = invokedPropertyName.Substring(4);
            }

            if (_lazyValues.ContainsKey(invokedPropertyName))
            {
                var newValue = _lazyExpressionResolver.Resolve(_lazyValues[invokedPropertyName]);
                var propertyInfo = invocation.TargetType.GetProperty(invokedPropertyName);
                var propertyInfoSetMethod = propertyInfo.SetMethod;
                propertyInfoSetMethod.Invoke(invocation.InvocationTarget, new[] {newValue});
                _lazyValues.Remove(invokedPropertyName);
            }
        }

        public bool IsHydrated()
        {
            return _isHydrated;
        }
    }
}