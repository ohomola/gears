﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Gears.Interpreter.App;
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.App.Workflow.Library;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Data;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.Assistance;
using Gears.Interpreter.Library.UI;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration7Tests
    {
        private string _filePath;
        private string _clickScenarioFilePath;

        [Test]
        public void ShouldBeAbleToSwitchEnvironment()
        {
            var url = "file:///"+ FileFinder.Find("Iteration1TestPage.html");
            var url2 = "file:///" + FileFinder.Find("Iteration2TestPage.html");

            Bootstrapper.Register(new Keyword[]
            {
                new Remember("Env1Url", url),
                new Remember("Env2Url", url2),
                new RunScenario(_inputFolder + "\\Scenario1.csv"),
                new RunScenario(_inputFolder + "\\Scenario2.csv"),
            });

            var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

            var interpreter = Bootstrapper.ResolveInterpreter();
            var response = interpreter.Please("start");

            interpreter.Please("remember Env1Url ");

            response = interpreter.Please("");
            response = interpreter.Please("");
            response = interpreter.Please("");

            Assert.AreEqual(new Uri(url), new Uri(selenium.WebDriver.Url));

            response = interpreter.Please("");

            Assert.AreEqual(new Uri(url2), new Uri(selenium.WebDriver.Url));
        }


        [Test]
        public void ShouldBeAbleToSkip()
        {
            var comment = new Comment("Hellp") {Skip=true};
            Bootstrapper.Register(new Keyword[]
            {
                comment,
            });
            var interpreter = Bootstrapper.ResolveInterpreter();
            var response = interpreter.Please("start");
            response = interpreter.Please(string.Empty);

            Assert.AreEqual(KeywordStatus.Skipped.ToString(), comment.Status);
        }

        [Test]
        public void ShouldBeAbleToAddEnvironmentToData()
        {
            Bootstrapper.Register();
            var fact = ServiceLocator.Instance.Resolve<IObjectAccessFactory>();
            var obj = fact.CreateFromArguments(new[] {"-url=http://www.google.com"});
            var rem = obj.Get<RememberedText>();

            Assert.IsNull(rem);

            rem = SharedObjectDataAccess.Instance.Value.Get<RememberedText>();

            Assert.IsNotNull(rem);

            Assert.AreEqual("url", rem.Variable);
            Assert.AreEqual("http://www.google.com", rem.What);
        }

        [Test]
        public void ShouldBeAbleToAskIsivisibleForANumber()
        {
            var url = "file:///" + FileFinder.Find("Iteration5TestPage.html");

            Bootstrapper.Register(new Keyword[]
            {
                new GoToUrl(url), 
                new IsVisible("Hello 102") {Expect = true},
                new IsVisible("103 hello") {Expect = true},
            });

            var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

            var interpreter = Bootstrapper.ResolveInterpreter();
            var response = interpreter.Please("start");

            response = interpreter.Please("");
            response = interpreter.Please("");
            Assert.IsInstanceOf<SuccessAnswer>(response);
            response = interpreter.Please("");
            Assert.IsInstanceOf<SuccessAnswer>(response, response.Text);

        }

        [Test]
        public void ShouldBeAbleTo_UseSkipAssertionsObject()
        {
            Bootstrapper.Register(new Keyword[]
            {
                new Use("SkipAssertions"),
            });
            Bootstrapper.ResolveInterpreter().Please("start");
            var response = Bootstrapper.ResolveInterpreter().Please(string.Empty);
            Assert.IsInstanceOf<SuccessAnswer>(response, response.Text);
        }

        [Test]
        public void ShouldBeAbleTo_SkipAssertions()
        {
            var url = "file:///" + FileFinder.Find("Iteration5TestPage.html");

            Bootstrapper.Register(new Keyword[]
            {
                new GoToUrl(url),
                new IsVisible("Hello 102") {Expect = true},
                new Use("SkipAssertions"), 
                new IsVisible("103 hello") {Expect = true},
                new IsVisible("Hello 102") {Expect = true},
            });

            var interpreter = Bootstrapper.ResolveInterpreter();
            var response = interpreter.Please("start");

            response = interpreter.Please(string.Empty);
            response = interpreter.Please(string.Empty);
            Assert.IsInstanceOf<SuccessAnswer>(response);
            response = interpreter.Please(string.Empty);
            response = interpreter.Please(string.Empty);
            Assert.IsNotInstanceOf<SuccessAnswer>(response, response.Text);
            response = interpreter.Please("off SkipAssertions");
            response = interpreter.Please(string.Empty);
            Assert.IsInstanceOf<SuccessAnswer>(response, response.Text);

        }


        [Test]
        public void Should_CloseBrowserReliably()
        {
            var url = "file:///" + FileFinder.Find("Iteration5TestPage.html");

            Bootstrapper.Register(new Keyword[]
            {
                new GoToUrl(url),
            });

            var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

            var interpreter = Bootstrapper.ResolveInterpreter();
            var response = interpreter.Please("start");
            response = interpreter.Please("");
            var processes = Process.GetProcesses();
            var isChromeDriverRunning = processes.Any(x => x.ProcessName.StartsWith("chromedriver"));
            Assert.IsTrue(isChromeDriverRunning);
            

            selenium.TerminateProcess("chromedriver");
            isChromeDriverRunning = Process.GetProcesses().Any(x => x.ProcessName.StartsWith("chromedriver"));
            Assert.IsFalse(isChromeDriverRunning);
        }

        private DirectoryInfo _outputFolder;
        private DirectoryInfo _inputFolder;
        private const string Scenario1CsvContent = "Discriminator,Url\nGoToUrl,[Env1Url]\n";
        private const string Scenario2CsvContent = "Discriminator,Url\nGoToUrl,[Env2Url]\n";


        [Test]
        public void ShouldBeAbleToResize()
        {
            //TBC
        }

        [SetUp]
        public void SetUp()
        {
            _outputFolder = Directory.CreateDirectory("./Output");
            _inputFolder = Directory.CreateDirectory("./Input");

            File.WriteAllText(_inputFolder + "\\Scenario1.csv", Scenario1CsvContent);
            File.WriteAllText(_inputFolder + "\\Scenario2.csv", Scenario2CsvContent);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete("./Input", true);
            Directory.Delete("./Output", true);
            Bootstrapper.Release();
        }
    }

    

    
}
