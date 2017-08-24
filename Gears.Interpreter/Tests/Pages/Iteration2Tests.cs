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
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Library.UI;
using NUnit.Framework;
using OpenQA.Selenium;

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
                new SeleniumAdapter();
        }

        //[TearDown]
        public void TearDown()
        {
            _selenium.Dispose();
        }

        [Test]
        public void ShouldClearLoginPassword()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration2TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();

            new Fill("login:", "user1") {Selenium = _selenium}.Execute();
            Assert.AreEqual("user1", _selenium.WebDriver.FindElement(By.Id("test4login")).GetAttribute("value"));

            new Clear("login:") {Selenium = _selenium}.Execute();
            Assert.AreEqual("", _selenium.WebDriver.FindElement(By.Id("test4login")).GetAttribute("value"));

        }
    }
}
