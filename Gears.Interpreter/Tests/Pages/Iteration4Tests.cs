using System;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
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
        public void ShouldParseInstructionCorrectly1()
        {
            Assert.AreEqual("blah",new Instruction("blah").SubjectName);

            var instruction = new Instruction("blah left from bleh with aa");
            Assert.AreEqual("blah", instruction.SubjectName);
            Assert.AreEqual("bleh", instruction.Locale);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
            Assert.AreEqual("aa", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly2()
        {
            var instruction = new Instruction("'something long' above 'something even longer' with ' an absurdely long text with numb3rs and stuff'");
            Assert.AreEqual("something long", instruction.SubjectName);
            Assert.AreEqual("something even longer", instruction.Locale);
            Assert.AreEqual(SearchDirection.AboveAnotherElement, instruction.Direction);
            Assert.AreEqual("an absurdely long text with numb3rs and stuff", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly3()
        {
            var instruction = new Instruction("something long above something even longer with ' an absurdely long text with numb3rs and stuff'");
            Assert.AreEqual("something long", instruction.SubjectName);
            Assert.AreEqual("something even longer", instruction.Locale);
            Assert.AreEqual(SearchDirection.AboveAnotherElement, instruction.Direction);
            Assert.AreEqual("an absurdely long text with numb3rs and stuff", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly4()
        {
            var instruction = new Instruction("blah blah with bluh bluh");
            Assert.AreEqual("blah blah", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
            Assert.AreEqual("bluh bluh", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly5()
        {
            var instruction = new Instruction("2nd button");
            Assert.AreEqual(string.Empty, instruction.SubjectName);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
            Assert.AreEqual(1, instruction.Order);
        }

        [Test]
        public void ShouldParseInstructionCorrectly6()
        {
            var instruction = new Instruction("4 textfield blah left from Password with 'Hello'");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual(SubjectType.Input, instruction.SubjectType);
            Assert.AreEqual("blah", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
            Assert.AreEqual("Password", instruction.Locale);
            Assert.AreEqual("Hello", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly7()
        {
            var instruction = new Instruction("4. link from right");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual(string.Empty, instruction.SubjectName);
            Assert.AreEqual(SubjectType.Link, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.LeftFromRightEdge, instruction.Direction);
            Assert.AreEqual(string.Empty, instruction.Locale);
        }

        [Test]
        public void ShouldParseInstructionCorrectly8()
        {
            var instruction = new Instruction("4. button from left");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
            Assert.AreEqual(string.Empty, instruction.Locale);
        }

        [Test]
        public void ShouldParseInstructionCorrectly9()
        {
            var instruction = new Instruction("4. button login from left");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual("login", instruction.SubjectName);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
            Assert.AreEqual(string.Empty, instruction.Locale);
        }

        [Test]
        public void ShouldParseInstructionCorrectly10()
        {
            var instruction = new Instruction("223421th input from bottom with flowers");
            Assert.AreEqual(223420, instruction.Order);
            Assert.AreEqual(string.Empty, instruction.SubjectName);
            Assert.AreEqual(SubjectType.Input, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.UpFromBottomEdge, instruction.Direction);
            Assert.AreEqual(string.Empty, instruction.Locale);
            Assert.AreEqual("flowers", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly11()
        {
            var instruction = new Instruction("1st button 'blah' from bottom with flowers");
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual("blah", instruction.SubjectName);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.UpFromBottomEdge, instruction.Direction);
            Assert.AreEqual(string.Empty, instruction.Locale);
            Assert.AreEqual("flowers", instruction.With);
        }


        [Test]
        public void ShoulClickOnExpectedButtons1()
        {
            new GoToUrl($"http://www.material-ui.com/#/components/snackbar")
            {
                Selenium = _selenium
            }.Execute();

            new Click("1st Drawer from top") { Selenium = _selenium }.Execute();
            
            try
            {
                new Click("button Menu Item 2") { Selenium = _selenium }.Execute();
                Assert.Fail("Should throw exception when clicking on item in offset burger menu");
            }
            catch (Exception)
            {
            }
            new IsVisible("Menu Item 2 left from Examples") { Selenium = _selenium, Expect = false }.Execute();
            new Click("1st Toggle drawer from top") { Selenium = _selenium }.Execute();
            new Wait(700).Execute();
            new Click("button Menu Item 2") { Selenium = _selenium }.Execute();
            new Wait(900).Execute();
            new IsVisible("Menu Item 2 left from Examples") { Selenium = _selenium, Expect = true }.Execute();
            new Click("1st Toggle drawer from top") { Selenium = _selenium }.Execute();
            new Wait(900).Execute();
            new IsVisible("Menu Item 2 left from Examples") { Selenium = _selenium, Expect = false }.Execute();

            new IsVisible("AppBar right from Examples") { Selenium = _selenium, Expect = false }.Execute();
            new Click("2nd Toggle drawer from top") { Selenium = _selenium }.Execute();
            new Wait(900).Execute();
            new IsVisible("AppBar right from Examples") { Selenium = _selenium, Expect = true }.Execute();
            new Click("2nd Toggle drawer from top") { Selenium = _selenium }.Execute();
            new Wait(900).Execute();
            new IsVisible("AppBar right from Examples") { Selenium = _selenium, Expect = false }.Execute();
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
