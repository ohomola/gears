﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Data.Serialization.Mapping;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.Reports;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration5Tests
    {
        private const string Scenario1CsvContent = "Discriminator,Text\nComment,Blah\n";
        private const string Scenario2CsvContent = "Discriminator,Text\nComment,Bleh\n";

        #region Setup & teardown
        private FileInfo[] _actualOutputFiles;
        private DirectoryInfo _outputFolder;
        private int _returnCode;
        private DirectoryInfo _inputFolder;
        private const string Scenario3CsvContent=
                @"Discriminator,Text
                Comment,{return ""Hello World"";}
                ";


        [SetUp]
        public void SetUp()
        {
            _outputFolder = Directory.CreateDirectory("./Output");
            _inputFolder = Directory.CreateDirectory("./Input");

            File.WriteAllText(_inputFolder + "\\Scenario1.csv",Scenario1CsvContent);
            File.WriteAllText(_inputFolder + "\\Scenario2.csv", Scenario2CsvContent);
            File.WriteAllText(_inputFolder + "\\Scenario3.csv", Scenario3CsvContent);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete("./Output", true);
        }
        #endregion

        private void TestBase(params object[] parameters)
        {
            var dataAccesses = parameters.Select(x => new ObjectDataAccess(x));

            Bootstrapper.RegisterForRuntime(dataAccesses);

            var applicationLoop = new ApplicationLoop(
                ServiceLocator.Instance.Resolve<IDataContext>(),
                ServiceLocator.Instance.Resolve<IConsoleDebugger>());

            _returnCode = applicationLoop.Run();

            _actualOutputFiles = _outputFolder.GetFiles();

            Bootstrapper.Release();
        }

        [Test]
        public void ShouldCreateExpectedXmlReports1()
        {
            TestBase(new JUnitScenarioReport(_outputFolder+"\\test.xml"));
            Assert.AreEqual(1, _actualOutputFiles.Length);
        }

        [Test]
        public void ShouldCreateExpectedXmlReports2()
        {
            TestBase(
                new JUnitScenarioReport(), 
                new CsvScenarioReport());
            Assert.AreEqual(2, _actualOutputFiles.Length);
        }

        [Test]
        public void ShouldCreateExpectedXmlReports3()
        {
            TestBase(
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
            Bootstrapper.PreRegisterForDataAccessCreation();

            TestBase(
                new JUnitScenarioReport(),
                new RunScenario(_inputFolder + "\\Scenario1.csv"),
                new RunScenario(_inputFolder + "\\Scenario2.csv")
                );
            Assert.AreEqual(Program.OkStatusCode, _returnCode);
            Assert.AreEqual(2, _actualOutputFiles.Length);
            Assert.IsTrue(File.ReadAllText(_actualOutputFiles.First().FullName).Contains("Comment: Blah"), "Should contain first keyword");
            Assert.IsFalse(File.ReadAllText(_actualOutputFiles.First().FullName).Contains("<skipped />"), "Must not contain 'skipped'");

            Assert.IsTrue(File.ReadAllText(_actualOutputFiles.Last().FullName).Contains("Comment: Bleh"), "Should contain second keyword");
            Assert.IsFalse(File.ReadAllText(_actualOutputFiles.Last().FullName).Contains("<skipped />"), "Must not contain 'skipped'");
        }

        [Test]
        public void ShouldRunBucket2()
        {
            TestBase(
                new JUnitScenarioReport(),
                new Comment("Blah"),
                new RunScenario(new Comment("Blah")),
                new RunScenario(new Comment("Bleh"))
                );
            Assert.AreEqual(Program.CriticalErrorStatusCode, _returnCode);
            Assert.AreEqual(0, _actualOutputFiles.Length);
        }

        [Test]
        public void ShouldRunBucket3()
        {
            TestBase(
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
            TestBase(
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

        private class Tree
        {
            public string FruitName { get; set; }

        }

        [Test]
        public void ShouldLoadLazyObjects()
        {
            var codeStub = "{return Generate.Word();}";
            var content = $"Discriminator,FruitName\nTree,{codeStub}\n";
            var path = _inputFolder + "\\Scenario3.csv";
            File.WriteAllText(path, content);

            Bootstrapper.PreRegisterForDataAccessCreation();
            ServiceLocator.Instance.Resolve<ITypeRegistry>().Register(typeof(Tree));
            var tree = new FileObjectAccess(path).Get<LazyObject>().Value as Tree;

            Assert.AreNotEqual(codeStub, tree.FruitName);
        }

        private class Car
        {
            public Car(Engine engine)
            {
                Engine = engine;
            }

            public Engine Engine { get; set; }
        }

        private class Apple
        {
            public string Engine { get; set; }
        }

        public class Engine
        {
            public int Power { get; set; }
        }

        [Test]
        public void ShouldLoadLazyObjects2()
        {
            var content =
                @"Discriminator,Engine
                Car,{return new Gears.Interpreter.Tests.Pages.Iteration5Tests.Engine(){Power=5};}
                Apple,None
                ";

            var path = _inputFolder + "\\Scenario4.csv";
            File.WriteAllText(path, content);

            Bootstrapper.PreRegisterForDataAccessCreation();
            ServiceLocator.Instance.Resolve<ITypeRegistry>().Register(typeof(Car));
            ServiceLocator.Instance.Resolve<ITypeRegistry>().Register(typeof(Apple));

            var obj = new FileObjectAccess(path).GetAll().First();
            Assert.IsNotInstanceOf<CorruptObject>(obj, obj.ToString());
            Assert.IsInstanceOf<LazyObject>(obj);
            var car = (obj as LazyObject).Value as Car;
            Assert.AreEqual(5, car.Engine.Power);

            var dataContext = new DataContext(new FileObjectAccess(path));
            var cars = dataContext.GetAllLazy<Car>();
            var apples = dataContext.GetAllLazy<Apple>();
            var lazies = dataContext.GetAll<LazyObject>();

            Assert.AreEqual(1, cars.Count());
            Assert.AreEqual(1, apples.Count());
            Assert.AreEqual(1, lazies.Count());
        }

        [Test]
        public void ShouldRunScenarioWithLazyObjects()
        {
            Bootstrapper.PreRegisterForDataAccessCreation();
            TestBase(
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
            Bootstrapper.PreRegisterForDataAccessCreation();
            TestBase(
                new CsvScenarioReport(),
                new Include(_inputFolder + "\\Scenario3.csv")
                );
            Assert.AreEqual(Program.OkStatusCode, _returnCode);
            Assert.AreEqual(1, _actualOutputFiles.Length);
            var text = File.ReadAllText(_actualOutputFiles.First().FullName);
            Assert.IsTrue(text.Contains("Hello World"), "Should contain Hello World");
        } 
        #endregion
    }
}
