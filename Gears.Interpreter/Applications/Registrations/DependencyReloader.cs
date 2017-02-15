using System.Collections.Generic;

namespace Gears.Interpreter.Applications.Registrations
{
    public interface IDependencyReloader
    {
        void Reload();
    }

    public class DependencyReloader : IDependencyReloader
    {
        private readonly object[] _explicitObjects;
        private readonly IEnumerable<string> _args;

        public DependencyReloader(object[] explicitObjects)
        {
            _explicitObjects = explicitObjects;
        }

        public DependencyReloader(IEnumerable<string> args)
        {
            _args = args;
        }

        public void Reload()
        {
            if (_args == null)
            {
                Bootstrapper.Register(_explicitObjects);
            }
            else
            {
                Bootstrapper.RegisterArguments(_args);
            }
        }
    }
}