using System;
using System.Collections.Generic;
using Castle.DynamicProxy;
using Gears.Interpreter.Core.Data.Serialization.Mapping.LazyResolving;

namespace Gears.Interpreter.Core.Data.Serialization.Mapping
{
    public class ResolveAllStringParameterReferencesInProperties : StandardInterceptor
    {
        private IDictionary<string, object> _lazyValues;
        private readonly ILazyExpressionResolver _lazyExpressionResolver;
        private bool _isHydrated;

        public ResolveAllStringParameterReferencesInProperties(IDictionary<string, object> lazyValues, ILazyExpressionResolver lazyExpressionResolver)
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
            if (invocation.Method.Name != nameof(Keyword.Execute) && invocation.Method.Name != nameof(Keyword.Hydrate))
            {
                return;
            }

            foreach (var key in _lazyValues.Keys)
            {
                var newValue = _lazyExpressionResolver.Resolve(_lazyValues[key]);
                var propertyInfo = invocation.TargetType.GetProperty(key);

                if (propertyInfo == null)
                {
                    throw new ArgumentException($"{invocation.TargetType} does not have {key} property.");
                }

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
                ModifyPropertyValueToLazyResolved(invocation, invokedPropertyName);
            }
        }

        private void ModifyPropertyValueToLazyResolved(IInvocation invocation, string invokedPropertyName)
        {
            var newValue = _lazyExpressionResolver.Resolve(_lazyValues[invokedPropertyName]);
            var propertyInfo = invocation.TargetType.GetProperty(invokedPropertyName);
            var propertyInfoSetMethod = propertyInfo.SetMethod;
            propertyInfoSetMethod.Invoke(invocation.InvocationTarget, new[] {newValue});
            _lazyValues.Remove(invokedPropertyName);
        }

        public bool IsHydrated()
        {
            return _isHydrated;
        }
    }
}