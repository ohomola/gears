using System.Diagnostics;
using System.Linq;
using Gears.Interpreter.App;
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Data.Serialization.Mapping.LazyResolving;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Library.UI;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration10Tests
    {
        private IInterpreter _interpreter;

        [SetUp]
        public void SetUp()
        {
            Bootstrapper.Register();
            _interpreter = Bootstrapper.ResolveInterpreter();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.Release();
        }

        [Test]
        public void ShouldBeAbleToUseTabKey()
        {
            var selenium = Bootstrapper.Container.Resolve<ISeleniumAdapter>();
            var languageResolver = Bootstrapper.Container.Resolve<ILazyExpressionResolver>();
            _interpreter.Please($"gotourl file:///{FileFinder.Find("Iteration1TestPage.html")}");
            _interpreter.AddToPlan(new Fill("login:", (string) languageResolver.Resolve("{\"UserName\"+Key.Tab+\"Password\"}")));
            _interpreter.Please(string.Empty);
            Assert.AreEqual("UserName", selenium.WebDriver.FindElement(By.Id("test4login"))
                .GetAttribute("value"));
            Assert.AreEqual("Password", selenium.WebDriver.FindElement(By.Id("test4password"))
                .GetAttribute("value"));
            _interpreter.Please("stop");
        }

        [Regression, Test]
        public void ResizeShouldWorkInChrome()
        {
            Should.Be<SuccessAnswer>(_interpreter.Please("use chrome"));
            Should.Be<InformativeAnswer>(_interpreter.Please("resize browser to 500 x 200"));
            var selenium = Bootstrapper.Container.Resolve<ISeleniumAdapter>();

            var width = selenium.BrowserWindowScreenRectangle.Right - selenium.BrowserWindowScreenRectangle.Left;
            var height = selenium.BrowserWindowScreenRectangle.Bottom - selenium.BrowserWindowScreenRectangle.Top;

            Assert.AreEqual(500, width);
            Assert.AreEqual(200, height);
        }
    }
}
