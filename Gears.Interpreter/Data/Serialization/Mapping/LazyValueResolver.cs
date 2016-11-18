using Gears.Interpreter.Tests.Pages;

namespace Gears.Interpreter.Data.Serialization.Mapping
{
    public interface ILazyValueResolver
    {
        object Resolve(object obj);
        bool CanResolve(object obj);
    }

    public class LazyValueResolver : ILazyValueResolver
    {
        private ICodeStubResolver _codeStubResolver;

        public LazyValueResolver(ICodeStubResolver codeStubResolver)
        {
            _codeStubResolver = codeStubResolver;
        }

        public object Resolve(object obj)
        {
            if (CanResolve(obj))
            {
                return _codeStubResolver.Resolve(obj as string);
            }
            
            return obj;
        }

        public bool CanResolve(object obj)
        {
            var s = (obj as string);

            if (s == null)
            {
                return false;
            }

            return s.Contains("{") && s.Contains("}");
        }
    }
}