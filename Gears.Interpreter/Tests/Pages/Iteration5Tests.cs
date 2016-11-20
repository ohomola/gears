﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Applications.Registrations;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.Reports;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration5Tests
    {
        private const string Scenario1CsvContent = "Discriminator,Text\nComment,Blah\n";
        private const string Scenario2CsvContent = "Discriminator,Text\nComment,Bleh\n";

        private const string Scenario3CsvContent =
            @"Discriminator,Text
                Comment,{""Hello World""}
                ";

        private const string Scenario4CsvContent =
                @"Discriminator,Variable,What,Text
                Comment,,,[randomVal]
                Remember,randomVal,{Generate.Word()},
                Comment,,,[randomVal]
                ";

        private const string Scenario5CsvContent =
                @"Discriminator,Url
                GoToUrl,http://www.google.com
                ";
        private const string Scenario6CsvContent =
                @"Discriminator,Url
                GoToUrl,http://www.microsoft.com
                ";

        #region Setup & teardown
        private FileInfo[] _actualOutputFiles;
        private DirectoryInfo _outputFolder;
        private int _returnCode;
        private DirectoryInfo _inputFolder;
        


        [SetUp]
        public void SetUp()
        {
            _outputFolder = Directory.CreateDirectory("./Output");
            _inputFolder = Directory.CreateDirectory("./Input");

            File.WriteAllText(_inputFolder + "\\Scenario1.csv",Scenario1CsvContent);
            File.WriteAllText(_inputFolder + "\\Scenario2.csv", Scenario2CsvContent);
            File.WriteAllText(_inputFolder + "\\Scenario3.csv", Scenario3CsvContent);
            File.WriteAllText(_inputFolder + "\\Scenario4.csv", Scenario4CsvContent);
            File.WriteAllText(_inputFolder + "\\Scenario5.csv", Scenario5CsvContent);
            File.WriteAllText(_inputFolder + "\\Scenario6.csv", Scenario6CsvContent);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete("./Output", true);
        }
        #endregion

        private void RunApplicationLoop(params object[] parameters)
        {
            Bootstrapper.Register(parameters);

            var applicationLoop = Bootstrapper.Resolve();

            try
            {
                _returnCode = applicationLoop.Run();
            }
            finally
            {
                _actualOutputFiles = _outputFolder.GetFiles();
                Bootstrapper.Release();
            }

            
        }

        [Test]
        public void ShouldCreateExpectedXmlReports1()
        {
            RunApplicationLoop(new JUnitScenarioReport(_outputFolder+"\\test.xml"));
            Assert.AreEqual(1, _actualOutputFiles.Length);
        }

        [Test]
        public void ShouldCreateExpectedXmlReports2()
        {
            RunApplicationLoop(
                new JUnitScenarioReport(), 
                new CsvScenarioReport());
            Assert.AreEqual(2, _actualOutputFiles.Length);
        }

        [Test]
        public void ShouldCreateExpectedXmlReports3()
        {
            RunApplicationLoop(
                new JUnitScenarioReport(), 
                new Comment("Blah"),
                new Comment("TTT"),
                new IsTrue(true) {Expect=false, Message="TestTest"});

            var text = File.ReadAllText(_actualOutputFiles.First().FullName);

            Assert.AreEqual(Program.ScenarioFailureStatusCode, _returnCode);
            Assert.AreEqual(1, _actualOutputFiles.Length);
            Assert.IsTrue(text.Contains("Comment: Blah"));
            Assert.IsTrue(text.Contains("TestTest"));
        }

        [Test]
        public void ShouldRunBucket1()
        {
            RunApplicationLoop(
                new JUnitScenarioReport(),
                new RunScenario(_inputFolder + "\\Scenario1.csv"),
                new RunScenario(_inputFolder + "\\Scenario2.csv")
                );
            Assert.AreEqual(Program.OkStatusCode, _returnCode);
            Assert.AreEqual(2, _actualOutputFiles.Length);
            var firstFileContent = File.ReadAllText(_actualOutputFiles.First().FullName);
            Assert.IsTrue(firstFileContent.Contains("Comment: Blah"), "Should contain first keyword");
            Assert.IsFalse(firstFileContent.Contains("<skipped />"), "Must not contain 'skipped'");

            Assert.IsTrue(File.ReadAllText(_actualOutputFiles.Last().FullName).Contains("Comment: Bleh"), "Should contain second keyword");
            Assert.IsFalse(File.ReadAllText(_actualOutputFiles.Last().FullName).Contains("<skipped />"), "Must not contain 'skipped'");
        }

        [Test]
        public void ShouldRunBucket2()
        {
            try
            {
                RunApplicationLoop(
                        new JUnitScenarioReport(),
                        new Comment("Blah"),
                        new RunScenario(new Comment("Blah")),
                        new RunScenario(new Comment("Bleh"))
                        );
                Assert.Fail("Application did not throw expected exception");
            }
            catch (Exception)
            {
            }
            Assert.AreEqual(0, _actualOutputFiles.Length);
        }

        [Test]
        public void ShouldRunBucket3()
        {
            RunApplicationLoop(
                new JUnitScenarioReport(),
                new RunScenario(new Comment("Scenario1"), new IsTrue(true) { Expect = false, Message = "FailureMessage" }),
                new RunScenario(new Comment("Scenario2"))
                );
            Assert.AreEqual(Program.ScenarioFailureStatusCode, _returnCode);
            Assert.AreEqual(2, _actualOutputFiles.Length);
            var firstFileContent = File.ReadAllText(_actualOutputFiles.First().FullName);
            Assert.IsTrue(firstFileContent.Contains("Comment: Scenario1"));
            Assert.IsTrue(firstFileContent.Contains("FailureMessage"));
            var secondFileContent = File.ReadAllText(_actualOutputFiles.Last().FullName);
            Assert.IsTrue(secondFileContent.Contains("Comment: Scenario2"));
            Assert.IsFalse(secondFileContent.Contains("FailureMessage"));
        }

        [Test]
        public void ShouldRunBucketWithSuiteReport()
        {
            RunApplicationLoop(
                new JUnitSuiteReport(),
                new RunScenario(new Comment("Scenario1"), new IsTrue(true) { Expect = false, Message = "FailureMessage" }),
                new RunScenario(new Comment("Scenario2"))
                );
            Assert.AreEqual(Program.ScenarioFailureStatusCode, _returnCode);
            Assert.AreEqual(1, _actualOutputFiles.Length);
            var fileContent = File.ReadAllText(_actualOutputFiles.First().FullName);
            Assert.IsTrue(fileContent.Contains("Comment: Scenario1"));
            Assert.IsTrue(fileContent.Contains("FailureMessage"));
            Assert.IsTrue(fileContent.Contains("Comment: Scenario2"));
        }

        #region Lazy evaluation

        public class Tree
        {
            public virtual string FruitName { get; set; }
        }

        [Test]
        public void ShouldLoadLazyObjects()
        {
            var codeStub = "{Generate.Word()}";
            var content = $"Discriminator,FruitName\nTree,{codeStub}\n";
            var path = _inputFolder + "\\Scenario3.csv";
            File.WriteAllText(path, content);

            Bootstrapper.Register();
            ServiceLocator.Instance.Resolve<ITypeRegistry>().Register(typeof(Tree));
            var tree = new FileObjectAccess(path, ServiceLocator.Instance.Resolve<ITypeRegistry>()).Get<Tree>();
            Assert.IsNotNull(tree);
            Assert.IsTrue(tree.GetType().Name.ToLower().Contains("proxy"));
            Assert.IsNotNull(codeStub, tree.FruitName);
            Assert.AreNotEqual(codeStub, tree.FruitName);
        }

        public class Car
        {
            public virtual Engine Engine { get; set; }
        }

        public class Apple
        {
            public string Engine { get; set; }
        }

        public class Engine
        {
            public int Power { get; set; }
        }

        public class NonVirtualCar
        {
            public Engine Engine { get; set; }
        }

        public class NonVirtualApple
        {
            public string Engine { get; set; }
        }

        [Test]
        public void ShouldLoadLazyObjects3()
        {
            var content =
                @"Discriminator,Engine
                NonVirtualCar,{new Gears.Interpreter.Tests.Pages.Iteration5Tests.Engine(){Power=5}}
                NonVirtualApple,ApplesDontHaveEngines
                ";

            var path = _inputFolder + "\\Scenario4.csv";
            File.WriteAllText(path, content);

            Bootstrapper.Register();
            ServiceLocator.Instance.Resolve<ITypeRegistry>().Register(typeof(NonVirtualCar));
            ServiceLocator.Instance.Resolve<ITypeRegistry>().Register(typeof(NonVirtualApple));

            var apple = new FileObjectAccess(path, ServiceLocator.Instance.Resolve<ITypeRegistry>()).GetAll().Last();
            Assert.IsInstanceOf<NonVirtualApple>(apple, apple.ToString());
            Assert.IsFalse(apple.GetType().Name.ToLower().Contains("proxy"));
            Assert.AreEqual("ApplesDontHaveEngines", (apple as NonVirtualApple).Engine);

            var obj = new FileObjectAccess(path, ServiceLocator.Instance.Resolve<ITypeRegistry>()).GetAll().First();
            Assert.IsInstanceOf<CorruptObject>(obj, obj.ToString());
            var car = obj as CorruptObject;
            Assert.IsTrue(car.Exception.Message.Contains("virtual"));
        }

        [Test]
        public void ShouldLoadLazyObjects2()
        {
            var content =
                @"Discriminator,Engine
                Car,{new Gears.Interpreter.Tests.Pages.Iteration5Tests.Engine(){Power=5}}
                Apple,None
                ";

            var path = _inputFolder + "\\Scenario4.csv";
            File.WriteAllText(path, content);

            Bootstrapper.Register();
            ServiceLocator.Instance.Resolve<ITypeRegistry>().Register(typeof(Car));
            ServiceLocator.Instance.Resolve<ITypeRegistry>().Register(typeof(Apple));

            var obj = new FileObjectAccess(path, ServiceLocator.Instance.Resolve<ITypeRegistry>()).GetAll().First();
            Assert.IsNotInstanceOf<CorruptObject>(obj, obj.ToString());
            Assert.IsInstanceOf<Car>(obj);
            var car = obj as Car;
            Assert.IsNotNull(car.Engine);
            Assert.AreEqual(5, car.Engine.Power);

            var dataContext = new DataContext(new FileObjectAccess(path, ServiceLocator.Instance.Resolve<ITypeRegistry>()));
            var cars = dataContext.GetAll<Car>();
            var apples = dataContext.GetAll<Apple>();

            Assert.AreEqual(1, cars.Count());
            Assert.AreEqual(1, apples.Count());
        }

        [Test]
        public void ShouldRunScenarioWithLazyObjects()
        {
            RunApplicationLoop(
                new CsvScenarioReport(),
                new RunScenario(_inputFolder + "\\Scenario3.csv")
                );
            Assert.AreEqual(Program.OkStatusCode, _returnCode);
            Assert.AreEqual(1, _actualOutputFiles.Length);
            var text = File.ReadAllText(_actualOutputFiles.First().FullName);
            Assert.IsTrue(text.Contains("Hello World"), "Should contain Hello World");
        }

        [Test]
        public void ShouldRunScenarioWithLazyObjects2()
        {
            RunApplicationLoop(
                new CsvScenarioReport(),
                new Include(_inputFolder + "\\Scenario3.csv")
                );
            Assert.AreEqual(Program.OkStatusCode, _returnCode);
            Assert.AreEqual(1, _actualOutputFiles.Length);
            var text = File.ReadAllText(_actualOutputFiles.First().FullName);
            Assert.IsTrue(text.Contains("Hello World"), "Should contain Hello World");
        } 
        #endregion

        [Test]
        public void ShouldRecallRememberedValues1()
        {
            var comment = new Comment("[myVariable1] [myVariable2]");
            RunApplicationLoop(
                new Remember("myVariable1", "{\"Hello\"}"),
                new Remember("myVariable2", "{\"World\"}"),
                comment
                );
            Assert.AreEqual("Hello World", comment.Text);
        }

        [Test]
        public void ShouldRecallRememberedValues2()
        {
            var comment = new Comment("{\"[myVariable1]\".ToUpper() + \" \" + \"[myVariable2]\".ToLower()}");
            RunApplicationLoop(
                new Remember("myVariable1", "{\"Hello\"}"),
                new Remember("myVariable2", "{\"World\"}"),
                comment
                );
            Assert.AreEqual("HELLO world", comment.Text);
        }

        [Test]
        public void ShouldRecallRememberedValues3()
        {
            var click = new Click("1st button [var1] [var2] from left") {Skip=true.ToString()};
            RunApplicationLoop(
                new Remember("var1", "{\"Hello\"}"),
                new Remember("var2", "{\"World\"}"),
                click
                );
            Assert.AreEqual("Hello World", click.VisibleTextOfTheButton);
        }

        public class TestHandler : IApplicationEventHandler
        {
            public List<Keyword> Keywords { get; set; } = new List<Keyword>();
            public virtual void Register(ApplicationLoop applicationLoop)
            {
                applicationLoop.ScenarioFinished += SaveKeywords;
            }

            protected virtual void SaveKeywords(object sender, ScenarioFinishedEventArgs e)
            {
                Keywords.AddRange(e.Keywords);
            }
        }

        [Test]
        public void ShouldRunScenario()
        {
            var testHandler = new TestHandler();
            RunApplicationLoop(
                testHandler,
                new RunScenario(_inputFolder + "\\Scenario4.csv"),
                new RunScenario(_inputFolder + "\\Scenario4.csv")
                );
            Assert.AreEqual("[randomVal]", ((Comment)testHandler.Keywords[0]).Text);
            Assert.AreNotEqual("[randomVal]", ((Comment)testHandler.Keywords[2]).Text);
            Assert.AreEqual("[randomVal]", ((Comment)testHandler.Keywords[3]).Text);
            Assert.AreNotEqual("[randomVal]", ((Comment)testHandler.Keywords[5]).Text);
        }

        public class IsUrl : Keyword
        {
            public override object Run()
            {
                return ExpectedUrl == Selenium.WebDriver.Url;
            }

            public string ExpectedUrl { get; set; }
        }

        [Test]
        public void ShouldRecallRememberedValues4()
        {
            var today= DateTime.Today.ToString("dd MM yyyy");
            var comment = new Comment("[today]");
            RunApplicationLoop(
                new Remember("today", "{DateTime.Today.ToString(\"dd MM yyyy\")}"),
                comment
                );
            Assert.AreEqual(today, comment.Text);
        }
    }
}