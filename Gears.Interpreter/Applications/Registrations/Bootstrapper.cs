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

using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Data.Serialization.Mapping;
using Gears.Interpreter.Data.Serialization.Mapping.LazyResolving;

namespace Gears.Interpreter.Applications.Registrations
{
    public class Bootstrapper
    {
        private static WindsorContainer _container;

        private static SeleniumAdapter _seleniumAdapterInstance;

        private static DependencyReloader _dependencyReloaderInstance;

        public static void Register(params object[] explicitObjects)
        {
            Register();

            _dependencyReloaderInstance = new DependencyReloader(explicitObjects);
            _container.Register(Component.For<IDependencyReloader>().Instance(_dependencyReloaderInstance).LifestyleSingleton());

            _container.Register(Component.For<IObjectAccessFactory>().ImplementedBy<ObjectAccessFactory>().LifestyleSingleton());
            _container.Register(Component.For<IDataObjectAccess>().Named("ExplicitObjects")
                .UsingFactory((IObjectAccessFactory f) => f.CreateFromObjects(explicitObjects)));
        }

        public static void Register(string[] args)
        {
            Register();

            _dependencyReloaderInstance = new DependencyReloader(args);
            _container.Register(Component.For<IDependencyReloader>().Instance(_dependencyReloaderInstance).LifestyleSingleton());

            var i = 0;
            foreach (var argument in args.Where(x => !x.StartsWith("-")))
            {
                i++;
                _container.Register(
                    Component.For<IDataObjectAccess>()
                    .ImplementedBy<FileObjectAccess>()
                    .DependsOn(Dependency.OnValue("path", argument))
                    .Named(argument + i++)
                    .LifestyleTransient());
            }

            var switchArguments = args.Where(x => x.StartsWith("-")).ToArray();

            _container.Register(Component.For<IObjectAccessFactory>().ImplementedBy<ObjectAccessFactory>().LifestyleSingleton());
            _container.Register(Component.For<IDataObjectAccess>().Named("SwitchesDataAccess").UsingFactory((IObjectAccessFactory f)=>f.CreateFromArguments(switchArguments)));
        }

        public static void Register()
        {
            Release();
            _container = new WindsorContainer();

            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel));

            _container.Register(Component.For<ICodeStubResolver>().ImplementedBy<CodeStubResolver>().LifestyleSingleton());
            _container.Register(Component.For<IRememberedDataResolver>().ImplementedBy<RememberedDataResolver>().LifestyleSingleton());
            _container.Register(Component.For<ILazyExpressionResolver>().ImplementedBy<LazyExpressionResolver>().LifestyleSingleton());
            _container.Register(Component.For<IDictionaryToObjectMapper>().ImplementedBy<DictionaryToObjectMapper>().LifestyleSingleton());

            _container.Register(Component.For<ITypeRegistry>().ImplementedBy<TypeRegistry>().LifestyleSingleton());
            _container.Register(Component.For<IDataContext>().ImplementedBy<DataContext>().LifestyleSingleton());

            _seleniumAdapterInstance = new SeleniumAdapter();
            _container.Register(Component.For<ISeleniumAdapter>().Instance(_seleniumAdapterInstance));

            _container.Register(Component.For<IConsoleDebugger>().ImplementedBy<ConsoleDebugger>().LifestyleSingleton());

            _container.Register(Component.For<IOverlay>().ImplementedBy<Overlay>().LifestyleSingleton());
            _container.Register(Component.For<ILateBoundDataContext>().ImplementedBy<LateBoundDataContext>().LifestyleSingleton());

            _container.Register(Component.For<IApplicationLoop>().ImplementedBy<ApplicationLoop>().LifestyleTransient());
            ServiceLocator.Initialise(_container);
        }

        public static IApplicationLoop Resolve()
        {
            return _container.Resolve<IApplicationLoop>();
        }

        public static void Release()
        {
            _seleniumAdapterInstance?.Dispose();
            _container?.Dispose();
            ServiceLocator.Release();
        }
    }
}
