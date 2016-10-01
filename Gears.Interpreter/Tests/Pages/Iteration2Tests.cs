using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration2Tests : IDisposable
    {
        private SeleniumAdapter _selenium;

        public Iteration2Tests()
        {
            SetUp();
        }
        public void Dispose()
        {
            TearDown();
        }
        
        //[SetUp]
        public void SetUp()
        {
            _selenium =
                new SeleniumAdapter(new ChromeDriver(Path.GetDirectoryName(FileFinder.Find("chromedriver.exe")),
                    new ChromeOptions()));
        }

        //[TearDown]
        public void TearDown()
        {
            _selenium.Dispose();
        }

        [Test]
        public void ShouldFindOnlyVisibleText()
        {
            new GoToUrl($"http://www.material-ui.com/#/components/snackbar")
            {
                Selenium = _selenium
            }.Execute();

            new IsVisible("Event added to your calendar") { Selenium = _selenium, Expect = false }.Execute();
            new Click("Add to my calendar") {Selenium = _selenium, Where = "top"}.Execute();
            new Wait(400).Execute();
            new IsVisible("Event added to your calendar") {Selenium = _selenium, Expect = true}.Execute();
            new Wait(400).Execute();
            new Click("Add to my calendar") { Selenium = _selenium, Where = "top" }.Execute();
            new Wait(400).Execute();
            new IsVisible("Event added to your calendar") { Selenium = _selenium, Expect = false }.Execute();
        }

        
    }
}
