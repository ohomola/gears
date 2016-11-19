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

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications.Configuration;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Data.Serialization.Mapping;
using OpenQA.Selenium.Chrome;

namespace Gears.Interpreter.Applications
{
    public class Bootstrapper
    {
        private static WindsorContainer _container;
        private static SeleniumAdapter _seleniumAdapter;

        public static void Register(IEnumerable<IDataObjectAccess> accesses)
        {
            Register();
            var i = 0;
            foreach (var ds in accesses)
            {
                _container.Register(Component.For<IDataObjectAccess>().Instance(ds).Named(ds.ToString() + i++).LifestyleSingleton());
            }
        }

        public static void Register(string[] args)
        {
            Register();

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
            _container.Register(Component.For<ISwitchesAsObjectAccessProvider>().ImplementedBy<SwitchesAsObjectAccessProvider>().LifestyleSingleton());
            _container.Register(Component.For<IDataObjectAccess>().Named("SwitchesDataAccess").UsingFactory((ISwitchesAsObjectAccessProvider f)=>f.Create(switchArguments)));
        }

        public static void Register()
        {
            _container = new WindsorContainer();

            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel));

            _container.Register(Component.For<ICodeStubResolver>().ImplementedBy<CodeStubResolver>().LifestyleSingleton());
            _container.Register(Component.For<IRememberedDataResolver>().ImplementedBy<RememberedDataResolver>().LifestyleSingleton());
            _container.Register(Component.For<ILazyExpressionResolver>().ImplementedBy<LazyExpressionResolver>().LifestyleSingleton());
            _container.Register(Component.For<IDictionaryToObjectMapper>().ImplementedBy<DictionaryToObjectMapper>().LifestyleSingleton());

            _container.Register(Component.For<ITypeRegistry>().ImplementedBy<TypeRegistry>().LifestyleSingleton());
            _container.Register(Component.For<IDataContext>().ImplementedBy<DataContext>().LifestyleSingleton());

            _seleniumAdapter = new SeleniumAdapter();
            _container.Register(Component.For<ISeleniumAdapter>().Instance(_seleniumAdapter));

            _container.Register(Component.For<IConsoleDebugger>().ImplementedBy<ConsoleDebugger>().LifestyleSingleton());

            _container.Register(Component.For<IOverlay>().ImplementedBy<Overlay>().LifestyleSingleton());

            ServiceLocator.Initialise(_container);
        }

        public static void Release()
        {
            _seleniumAdapter?.Dispose();
            _container?.Dispose();
            ServiceLocator.Release();
        }
    }
}
