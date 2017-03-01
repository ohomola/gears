using System;
using System.Linq;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;

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


            var nonparametricArguments = arguments.Where(x => !x.Contains("=")).ToList();
            var parametricArguments = arguments.Where(x => x.Contains("=")).ToList();

            foreach (var parametricArgument in parametricArguments)
            {
                var name = parametricArgument.Substring(1, parametricArgument.IndexOf('=')-1);
                var value = parametricArgument.Substring(parametricArgument.IndexOf('=')+1);
                var rememberedText = new RememberedText(name, value);
                SharedObjectDataAccess.Instance.Value.Add(rememberedText);
            }

            foreach (var argument in nonparametricArguments)
            {
                var dtoTypes = _registry.GetDTOTypes();
                var type =
                    dtoTypes.FirstOrDefault(registeredType => registeredType.Name.ToLower() == argument.Substring(1).ToLower());

                if (type == null)
                {
                    //throw new ArgumentException($"Argument {argument} is not recognized.");

                    objectDataAccess.Add(new CorruptObject() {Description = $"Argument {argument} is not recognized." });

                    continue;

                }
                var instance = Activator.CreateInstance(type);

                objectDataAccess.Add(instance);
            }

            foreach (var obj in objectDataAccess.GetAll())
            {
                (obj as Keyword)?.Hydrate();
            }

            return objectDataAccess;
        }

        public ObjectDataAccess CreateFromObjects(object[] objects)
        {
            var objectDataAccess = new ObjectDataAccess();

            foreach (var instance in objects)
            {
                objectDataAccess.Add(instance);
                (instance as Keyword)?.Hydrate();
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