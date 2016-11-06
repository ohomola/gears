using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration5Tests : IDisposable
    {
        private SeleniumAdapter _selenium;

        public Iteration5Tests()
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
            _selenium?.Dispose();
        }
        
        [Test]
        public void ShoulClickOnExpectedButtons2()
        {
            new GoToUrl($"file:///{FileFinder.Find("Iteration5TestPage.html")}")
            {
                Selenium = _selenium
            }.Execute();

            new IsVisible("Hello 34") { Selenium = _selenium, Expect = true}.Execute();
        }
    }
}
