using System;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.UI;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration4Tests : IDisposable
    {
        private SeleniumAdapter _selenium;

        public Iteration4Tests()
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
                new SeleniumAdapter();
        }

        //[TearDown]
        public void TearDown()
        {
            _selenium?.Dispose();
        }

        


        [Test]
        public void ShoulClickOnExpectedButtons1()
        {
            new GoToUrl($"http://www.material-ui.com/#/")
            {
                Selenium = _selenium
            }.Execute();

            new Click("1st button from top") { Selenium = _selenium }.Execute();
            new Wait(700).Execute();
            new Click("1st Components from left") { Selenium = _selenium }.Execute();
            new Wait(700).Execute();
            new Click("1st Drawer from left") { Selenium = _selenium }.Execute();

            try
            {
                new Click("button Menu Item 2") { Selenium = _selenium }.Execute();
                Assert.Fail("Should throw exception when clicking on item in offset burger menu");
            }
            catch (Exception)
            {
            }
            new Wait(500).Execute();
            new IsVisible("Menu Item 2 left from Examples") { Selenium = _selenium, Expect = false }.Execute();
            new Click("1st Toggle drawer from top") { Selenium = _selenium }.Execute();
            new Wait(1200).Execute();
            new Click("Menu Item 2 from left") { Selenium = _selenium }.Execute();
            new Wait(900).Execute();

        }


        [Test]
        public void ShoulClickOnExpectedButtons2()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration4TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();

            new Fill("2nd textfield from bottom with login2") { Selenium = _selenium }.Execute();
            Assert.AreEqual("login2", _selenium.WebDriver.FindElement(By.Id("test5login")).GetAttribute("value"));

            new Fill("2nd textfield above Password2: with logog") { Selenium = _selenium }.Execute();
            Assert.AreEqual("logog", _selenium.WebDriver.FindElement(By.Id("test4")).GetAttribute("value"));

            new Fill("2nd textfield below TextArea 2 with gugugu") { Selenium = _selenium }.Execute();
            Assert.AreEqual("gugugu", _selenium.WebDriver.FindElement(By.Id("test3")).GetAttribute("value"));
        }

        [Test]
        public void ShoulClickOnTheButtonNotOnTheSpan()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration4TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();

            new Click("save") { Selenium = _selenium }.Execute();
            Assert.AreEqual("success", _selenium.WebDriver.FindElement(By.Id("span1")).GetAttribute("innerText"));

        }

        [Test]
        public void ShouldFillSpecialCharacters()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration4TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();

            new Fill("Password: with <|>=5") { Selenium = _selenium }.Execute();
            Assert.AreEqual("<|>=5", _selenium.WebDriver.FindElement(By.Id("test4password")).GetAttribute("value"));

        }
    }
}
