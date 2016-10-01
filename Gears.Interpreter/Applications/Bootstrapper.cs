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
using OpenQA.Selenium.Chrome;

namespace Gears.Interpreter.Applications
{
    public class Bootstrapper
    {

        private static WindsorContainer _container;
        private static SeleniumAdapter _seleniumAdapter;

        public static void RegisterForRuntime(IEnumerable<IDataObjectAccess> dataSources)
        {
            _container = new WindsorContainer();

            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel));

            _container.Register(Component.For<ITypeRegistry>().ImplementedBy<TypeRegistry>().LifestylePerThread());

            var i = 0;
            foreach (var ds in dataSources)
            {
                _container.Register(Component.For<IDataObjectAccess>().Instance(ds).Named(ds.ToString()+ i++).LifestyleSingleton());
            }

            i = 0;

            foreach (var configObject in dataSources.SelectMany(ds=> ds.GetAll<IConfig>()))
            {
                _container.Register(Component.For(configObject.GetType()).Instance(configObject).Named(configObject.ToString() + i++).LifestyleTransient());
            }


            var path = FileFinder.Find("chromedriver.exe");
            var selenium = new ChromeDriver(Path.GetDirectoryName(path), new ChromeOptions());

            _seleniumAdapter = new SeleniumAdapter(selenium);
            _container.Register(Component.For<ISeleniumAdapter>().Instance(_seleniumAdapter));

            _container.Register(Component.For<IConsoleDebugger>().ImplementedBy<ConsoleDebugger>().LifestyleSingleton());

            _container.Register(Component.For<IDataContext>().ImplementedBy<DataContext>().LifestyleSingleton());
            _container.Register(Component.For<IOverlay>().ImplementedBy<Overlay>().LifestyleSingleton());

            ServiceLocator.Initialise(_container);
        }

        public static void RegisterForConfigurationLoad()
        {
            _container = new WindsorContainer();

            _container.Kernel.Resolver.AddSubResolver(new CollectionResolver(_container.Kernel));

            _container.Register(Component.For<ITypeRegistry>().ImplementedBy<TypeRegistry>().LifestyleSingleton());

            ServiceLocator.Initialise(_container);
        }

        public static void Release()
        {
            _seleniumAdapter?.Dispose();
            _container.Dispose();
        }
    }
}
