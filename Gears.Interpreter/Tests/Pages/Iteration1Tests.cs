﻿#region LICENSE
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
using System.Linq;
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.UI;
using NUnit.Framework;
using OpenQA.Selenium;

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
                new SeleniumAdapter();
        }

        //[TearDown]
        public void TearDown()
        {
            _selenium.Dispose();
            Bootstrapper.Release();
        }

        [Test]
        public void ShouldLoadScenarioFileCorrectly()
        {
            Bootstrapper.Register();
            var keywords= new FileObjectAccess(FileFinder.Find("Iteration1.xlsx"), ServiceLocator.Instance.Resolve<ITypeRegistry>()).GetAll<Keyword>().ToList();

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
            new Fill("Text Area 1", "filledText1") {Selenium = _selenium}.Execute();
            new Fill("Text Area 2", "filledText2") {Selenium = _selenium}.Execute();
            new Fill("Text Area 3", "filledText3") {Selenium = _selenium}.Execute();

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

            new Click("SAVE from right") {Selenium = _selenium}.Execute();
            new Click("load from left") {Selenium = _selenium}.Execute();

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

            var response = new Fill("MultiLine and FloatingLabel", "test1") {Selenium = _selenium}.Execute();
            Assert.IsInstanceOf<SuccessAnswer>(response);
            response = new Fill("Hint Text", "test2") { Selenium = _selenium }.Execute();
            Assert.IsInstanceOf<SuccessAnswer>(response);
        }


        [Test]
        public void ShouldFindOnlyVisibleText()
        {
            new GoToUrl($"http://www.material-ui.com/#/components/snackbar")
            {
                Selenium = _selenium
            }.Execute();

            
            new IsVisible("Event added to your calendar") { Selenium = _selenium, Expect = false }.Execute();
            new Click("Add to my calendar from top") { LookForOrthogonalNeighboursOnly = true, Selenium = _selenium}.Execute();
            new Wait(800).Execute();
            new IsVisible("Event added to your calendar") { Selenium = _selenium, Expect = true }.Execute();
            new Wait(4000).Execute();//Wait til it hides
            new IsVisible("Event added to your calendar") { Selenium = _selenium, Expect = false }.Execute();
        }

        [Test]
        [Ignore("Covered elsewhere")]
        public void ShouldBeAbleToClickOnRelativeButton()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration1TestPageRelativeButtons.html")}")
            {
                Selenium = _selenium
            }.Execute();

            //new Highlighter("buttons from right") { Selenium = _selenium}.Execute();

            new Click("1st button from right") { LookForOrthogonalNeighboursOnly = true, Selenium = _selenium }.Execute();
            Assert.AreEqual("pressed", _selenium.WebDriver.FindElement(By.Id("b4")).GetAttribute("innerText"));

            new Click("2nd button left from pressed") { LookForOrthogonalNeighboursOnly = true, Selenium = _selenium }.Execute();
            Assert.AreEqual("pressed", _selenium.WebDriver.FindElement(By.Id("b2")).GetAttribute("innerText"));

            new Click("4th button from left") { LookForOrthogonalNeighboursOnly = true, Selenium = _selenium }.Execute();
            Assert.AreEqual("pressed", _selenium.WebDriver.FindElement(By.Id("b6")).GetAttribute("innerText"));

            new Click("1st button right from Button7") { LookForOrthogonalNeighboursOnly = true, Selenium = _selenium }.Execute();
            Assert.AreEqual("pressed", _selenium.WebDriver.FindElement(By.Id("b8")).GetAttribute("innerText"));


        }
    }
}
