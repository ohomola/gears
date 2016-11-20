using System;
using System.Linq;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Applications.Registrations
{
    public interface IObjectAccessFactory
    {
        ObjectDataAccess CreateFromArguments(string[] arguments);
        ObjectDataAccess CreateFromObjects(object[] objects);
    }

    public class ObjectAccessFactory : IObjectAccessFactory
    {
        private readonly ITypeRegistry _registry;

        public ObjectAccessFactory(ITypeRegistry registry)
        {
            _registry = registry;
        }

        public ObjectDataAccess CreateFromArguments(string[] arguments)
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

        public ObjectDataAccess CreateFromObjects(object[] objects)
        {
            var objectDataAccess = new ObjectDataAccess();

            foreach (var instance in objects)
            {
                objectDataAccess.Add(instance);
            }

            return objectDataAccess;
        }
    }

    public interface ILateBoundDataContext
    {
        IDataContext Value { get; }
    }

    public class LateBoundDataContext : ILateBoundDataContext
    {
        public IDataContext Value => ServiceLocator.Instance.Resolve<IDataContext>();
    }
}