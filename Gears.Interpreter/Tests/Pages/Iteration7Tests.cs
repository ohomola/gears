using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Registrations;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.Documentations;
using Gears.Interpreter.Library.Workflow;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration7Tests
    {
        private string _filePath;
        private string _clickScenarioFilePath;

        [Test]
        public void ShouldWorkWithSpecialCharacters()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();
            var response = interpreter.Please("Remember Variable1 Hello world");

            ExpectFillAs("Hello world", "fill TextArea 1 with [Variable1]", interpreter);
            ExpectFillAs("\"Hello world\"", "fill TextArea 1 with \"Hello world\"", interpreter);
            ExpectFillAs("{hello}", "fill TextArea 1 with {hello}", interpreter);
            ExpectFillAs("{Hello world}", "fill TextArea 1 with {[Variable1]}", interpreter);
        }

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

        private DirectoryInfo _outputFolder;
        private DirectoryInfo _inputFolder;
        private const string Scenario1CsvContent = "Discriminator,Url\nGoToUrl,[Env1Url]\n";
        private const string Scenario2CsvContent = "Discriminator,Url\nGoToUrl,[Env2Url]\n";

        private static void ExpectFillAs(string expectedValue, string fillCommand, IInterpreter interpreter)
        {
            var selenium = ServiceLocator.Instance.Resolve<ISeleniumAdapter>();

            var response = interpreter.Please("gotourl {\"file:///\"+FileFinder.Find(\"Iteration2TestPage.html\")}");

            response = interpreter.Please(fillCommand);
            Assert.IsInstanceOf<SuccessAnswer>(response);
            var actualValue = selenium.WebDriver.FindElement(By.Id("test1")).GetAttribute("value");

            Assert.AreEqual(expectedValue, actualValue);

            interpreter.Please("clear TextArea 1");
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
