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
    public class Iteration1Tests : IDisposable
    {
        private SeleniumAdapter _selenium;

        public Iteration1Tests()
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
        public void ShouldLoadScenarioFileCorrectly()
        {
            Bootstrapper.RegisterForConfigurationLoad();
            var keywords= new FileObjectAccess(FileFinder.Find("Iteration1.xlsx")).GetAll<Keyword>().ToList();

            Assert.IsInstanceOf<GoToUrl>(keywords.ElementAt(0));
            Assert.AreEqual("http://www.SELENIUMhq.org/",(keywords.ElementAt(0) as GoToUrl).Url);
        }


        [Test]
        public void ShouldFillAllTextAreas()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration1TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();
            new Fill("TextArea 1", "filledText1") {Selenium = _selenium}.Execute();
            new Fill("TextArea 2", "filledText2") {Selenium = _selenium}.Execute();
            new Fill("TextArea 3", "filledText3") {Selenium = _selenium}.Execute();

            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("test1"))
                .GetAttribute("value"), "filledText1");

            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("test2"))
                .GetAttribute("value"), "filledText2");


            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("test3"))
                .GetAttribute("value"), "filledText3");
        }

        [Test]
        public void ShouldClickAllButtons()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration1TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();

            new Click("SAVE") {Selenium = _selenium, Where = "right"}.Execute();
            new Click("load") {Selenium = _selenium, Where = "left"}.Execute();

            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("but1"))
                .GetAttribute("innerText"), "success");

            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("but2"))
                .GetAttribute("innerText"), "success");
        }

        [Test]
        public void ShouldFillLoginPassword()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration1TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();

            new Fill("login:", "user1") {Selenium = _selenium}.Execute();
            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("test4login"))
                .GetAttribute("value"), "user1");

            new Fill("password:", "pass1") {Selenium = _selenium}.Execute();
            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("test4password"))
                .GetAttribute("value"), "pass1");


            new Fill("login2:", "user2") {Selenium = _selenium}.Execute();
            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("test5login"))
                .GetAttribute("value"), "user2");

            new Fill("password2:", "pass2") {Selenium = _selenium}.Execute();
            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("test5password"))
                .GetAttribute("value"), "pass2");
        }

        [Test]
        public void ShouldFillReactTextAreaWithFloatingLabel()
        {
            new GoToUrl($"http://www.material-ui.com/#/components/text-field")
            {
                Selenium = _selenium
            }.Execute();

            Assert.AreEqual("test1", (new Fill("MultiLine and FloatingLabel", "test1") {Selenium = _selenium}.Execute() as IWebElement)?.GetAttribute("value"));
            Assert.AreEqual("test2", (new Fill("Hint Text", "test2") { Selenium = _selenium }.Execute() as IWebElement)?.GetAttribute("value"));
        }
    }
}
