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
    public class Iteration3Tests : IDisposable
    {
        private SeleniumAdapter _selenium;

        public Iteration3Tests()
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
        public void ShouldFillLoginPassword()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration3TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();

            var fill = new Fill("textfield next to TextArea1 with ble") { Selenium = _selenium };
            fill.Execute();
            Assert.AreEqual("ble", _selenium.WebDriver.FindElement(By.Id("test3")).GetAttribute("value"));

            //new Fill("first input above first button save from the left with blah"){Selenium = _selenium}.Execute();
            //Assert.AreEqual("blah", _selenium.WebDriver.FindElement(By.Id("test3")).GetAttribute("value"));

            //new Fill("second input above first button load from the left with blahblah") { Selenium = _selenium }.Execute();
            //Assert.AreEqual("blahblah", _selenium.WebDriver.FindElement(By.Id("test3")).GetAttribute("value"));

        }
    }
}
