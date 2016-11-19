using System;
using System.Linq;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Applications
{
    public interface ISwitchesAsObjectAccessProvider
    {
        ObjectDataAccess Create(string[] arguments);
    }

    public class SwitchesAsObjectAccessProvider : ISwitchesAsObjectAccessProvider
    {
        private ITypeRegistry _registry;

        public SwitchesAsObjectAccessProvider(ITypeRegistry registry)
        {
            _registry = registry;
        }

        public ObjectDataAccess Create(string[] arguments)
        {
            var objectDataAccess = new ObjectDataAccess();

            foreach (var argument in arguments)
            {
                var dtoTypes = _registry.GetDTOTypes();
                var type =
                    dtoTypes.FirstOrDefault(registeredType => registeredType.Name.ToLower() == argument.Substring(1).ToLower());

                if (type == null)
                {
                    throw new ArgumentException($"Argument {argument} is not recognized.");
                }
                var instance = Activator.CreateInstance(type);
                objectDataAccess.Add(instance);
            }

            return objectDataAccess;
        }
    }
}