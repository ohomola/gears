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

using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Facilities.TypedFactory;
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
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Applications.Registrations
{
    public class Bootstrapper
    {
        public static WindsorContainer Container { get; private set; }

        private static SeleniumAdapter _seleniumAdapterInstance;

        private static DependencyReloader _dependencyReloaderInstance;

        public static void Register(params object[] explicitObjects)
        {
            Register();

            _dependencyReloaderInstance = new DependencyReloader(explicitObjects);
            Container.Register(Component.For<IDependencyReloader>().Instance(_dependencyReloaderInstance).LifestyleSingleton());

            Container.Register(Component.For<IObjectAccessFactory>().ImplementedBy<ObjectAccessFactory>().LifestyleSingleton());
            Container.Register(Component.For<IDataObjectAccess>().Named("ExplicitObjects").UsingFactory((IObjectAccessFactory f) => f.CreateFromObjects(explicitObjects)));
        }

        public static void RegisterArguments(params string[] args)
        {
            RegisterArguments(args.ToList());
        }

        public static void RegisterArguments(IEnumerable<string> args)
        {
            Register();

            _dependencyReloaderInstance = new DependencyReloader(args);
            Container.Register(Component.For<IDependencyReloader>().Instance(_dependencyReloaderInstance).LifestyleSingleton());

            var i = 0;
            foreach (var argument in args.Where(x => !x.StartsWith("-")))
            {
                i++;
                Container.Register(
                    Component.For<IDataObjectAccess>()
                    .ImplementedBy<FileObjectAccess>()
                    .DependsOn(Dependency.OnValue("path", FileFinder.Find(argument)))
                    .Named(argument + i++)
                    .LifestyleTransient());
            }

            var switchArguments = args.Where(x => x.StartsWith("-")).ToArray();

            Container.Register(Component.For<IObjectAccessFactory>().ImplementedBy<ObjectAccessFactory>().LifestyleSingleton());
            Container.Register(Component.For<IDataObjectAccess>().Named("SwitchesDataAccess").UsingFactory((IObjectAccessFactory f)=>f.CreateFromArguments(switchArguments)));
        }

        private static void Register()
        {
            Release();
            Container = new WindsorContainer();

            
            Container.Kernel.Resolver.AddSubResolver(new CollectionResolver(Container.Kernel));

            Container.Register(Classes.FromAssemblyContaining<IKeyword>()
                .BasedOn<Keyword>().WithServiceSelf().WithServiceAllInterfaces());


            Container.Register(Component.For<IDataObjectAccess>().Named("SharedObjectDataAccess").Instance(SharedObjectDataAccess.Instance.Value).LifestyleSingleton());

            Container.Register(Component.For<ICodeStubResolver>().ImplementedBy<CodeStubResolver>().LifestyleSingleton());
            Container.Register(Component.For<IRememberedDataResolver>().ImplementedBy<RememberedDataResolver>().LifestyleSingleton());
            Container.Register(Component.For<ILazyExpressionResolver>().ImplementedBy<LazyExpressionResolver>().LifestyleSingleton());
            Container.Register(Component.For<IDictionaryToObjectMapper>().ImplementedBy<DictionaryToObjectMapper>().LifestyleSingleton());

            Container.Register(Component.For<ITypeRegistry>().ImplementedBy<TypeRegistry>().LifestyleSingleton());
            Container.Register(Component.For<IDataContext>().ImplementedBy<DataContext>().LifestyleSingleton());

            _seleniumAdapterInstance = new SeleniumAdapter();
            Container.Register(Component.For<ISeleniumAdapter>().Instance(_seleniumAdapterInstance));

            Container.Register(Component.For<IConsoleDebugger>().ImplementedBy<ConsoleDebugger>().LifestyleSingleton());

            Container.Register(Component.For<ILanguage>().ImplementedBy<Language>().LifestyleSingleton());

            Container.Register(Component.For<IOverlay>().ImplementedBy<Overlay>().LifestyleSingleton());
            Container.Register(Component.For<ILateBoundDataContext>().ImplementedBy<LateBoundDataContext>().LifestyleSingleton());

            Container.Register(Component.For<IApplicationLoop>().ImplementedBy<ApplicationLoop>().LifestyleTransient());
            Container.Register(Component.For<IInterpreter>().ImplementedBy<Interpreter>().LifestyleSingleton());

            Container.Kernel.AddFacility<TypedFactoryFacility>();
            ServiceLocator.Initialise(Container);
        }

        public static IApplicationLoop Resolve()
        {
            return Container.Resolve<IApplicationLoop>();
        }

        public static IInterpreter ResolveInterpreter()
        {
            if (Container == null)
            {
                throw new InvalidOperationException("Bootstrapper is not registered");
            }

            return Container.Resolve<IInterpreter>();
        }

        public static void Release()
        {
            _seleniumAdapterInstance?.Dispose();
            Container?.Dispose();
            Container = null;
            ServiceLocator.Release();
        }
    }
}
