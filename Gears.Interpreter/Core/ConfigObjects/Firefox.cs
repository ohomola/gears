using Gears.Interpreter.App;
using Gears.Interpreter.App.Configuration;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.Core.ConfigObjects
{
    public class Firefox : IAutoRegistered
    {
        public void Register(IInterpreter interpreter)
        {
            if (ServiceLocator.IsInitialised())
            {
                var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

                selenium.SetBrowserType(SeleniumAdapterBrowserType.Firefox);
            }
        }
    }

    public class InternetExplorer : IAutoRegistered
    {
        public void Register(IInterpreter interpreter)
        {
            if (ServiceLocator.IsInitialised())
            {
                var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

                selenium.SetBrowserType(SeleniumAdapterBrowserType.InternetExplorer);
            }
        }
    }

    public class Chrome : IAutoRegistered
    {
        public void Register(IInterpreter interpreter)
        {
            if (ServiceLocator.IsInitialised())
            {
                var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

                selenium.SetBrowserType(SeleniumAdapterBrowserType.Chrome);
            }
        }
    }
}