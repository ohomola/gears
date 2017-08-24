using System.Drawing;
using System.Threading;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;

namespace Gears.Interpreter.Library.UI
{
    public class Login : Keyword
    {
        public string UserName { set; get; }
        public string Password { set; get; }

        public Login(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public Login()
        {
        }

        public override object DoRun()
        {
            var chromeHandle = Selenium.GetChromeHandle();
            var clientPoint = new Point(200, 200);
            //UserInteropAdapter.ClickOnPoint(chromeHandle, clientPoint);
            Selenium.BringToFront();
            Thread.Sleep(150);
            UserInteropAdapter.SendText(chromeHandle, $"PRGORILOCAL\\{UserName}\t{Password}\n", clientPoint);

            return null;
        }
    }
}