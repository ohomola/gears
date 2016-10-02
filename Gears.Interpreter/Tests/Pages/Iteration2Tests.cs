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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging.Overlay;
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

        [Test]
        public void ShouldBeAbleToClickOnRelativeButton()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration2TestPageRelativeButtons.html")}")
            {
                Selenium = _selenium
            }.Execute();

            new Click("first button in the right corner") { Selenium = _selenium}.Execute();
            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("b4")).GetAttribute("innerText"), "pressed");

            new Click("a button in the left corner") { Selenium = _selenium }.Execute();
            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("b1")).GetAttribute("innerText"), "pressed");
            
            new Click("second from left corner") { Selenium = _selenium }.Execute();
            Assert.AreEqual(_selenium.WebDriver.FindElement(By.Id("b2")).GetAttribute("innerText"), "pressed");
        }


        
    }
}
