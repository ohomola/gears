using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Data;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
using Gears.Interpreter.Library.Reports;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration5Tests
    {
        #region Setup & teardown
        private FileInfo[] files;
        private DirectoryInfo folder;
        private int returnCode;

        [SetUp]
        public void SetUp()
        {
            folder = Directory.CreateDirectory("./Output");

            File.WriteAllText(folder+ "\\Scenario1.csv","Discriminator,Text\nComment,Blah\n");
            File.WriteAllText(folder + "\\Scenario2.csv", "Discriminator,Text\nComment,Blah\n");
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete("./Output", true);
        } 
        #endregion

        [Test]
        public void ShouldCreateExpectedXMLReports1()
        {
            TestBase(new JUnitReport(folder+"\\test.xml"));
            Assert.AreEqual(1, files.Length);
        }

        [Test]
        public void ShouldCreateExpectedXMLReports2()
        {
            TestBase(
                new JUnitReport(), 
                new CsvReport());
            Assert.AreEqual(2, files.Length);
        }

        [Test]
        public void ShouldCreateExpectedXMLReports3()
        {
            TestBase(
                new JUnitReport(), 
                new Comment("Blah"),
                new Comment("TTT"),
                new IsTrue(true) {Expect=false, Message="TestTest"});
            var text = File.ReadAllText(files.First().FullName);
            Assert.AreEqual(Program.ScenarioFailureStatusCode, returnCode);
            Assert.AreEqual(1, files.Length);
            Assert.IsTrue(text.Contains("Comment: Blah"));
            Assert.IsTrue(text.Contains("TestTest"));
        }

        private void TestBase(params object[] parameters )
        {
            var accesses = parameters.Select(x => new ObjectDataAccess(x));
            Bootstrapper.RegisterForRuntime(accesses);
            var applicationLoop = new ApplicationLoop();

            returnCode = applicationLoop.Run();
            files = folder.GetFiles();
        }

        [Test]
        public void ShouldRunBucket1()
        {
            TestBase(
                new JUnitReport(),
                new RunScenario(folder + "\\Scenario1.csv"),
                new RunScenario(folder + "\\Scenario2.csv")
                );
            Assert.AreEqual(Program.OkStatusCode, returnCode);
            Assert.AreEqual(2, files.Length);
            Assert.IsTrue(File.ReadAllText(files.First().FullName).Contains("Comment: Blah"));
            Assert.IsTrue(File.ReadAllText(files.Last().FullName).Contains("Comment: Bleh"));
        }

        [Test]
        public void ShouldRunBucket2()
        {
            TestBase(
                new JUnitReport(),
                new Comment("Blah"),
                new RunScenario(new Comment("Blah")),
                new RunScenario(new Comment("Bleh"))
                );
            Assert.AreEqual(Program.CriticalErrorStatusCode, returnCode);
            Assert.AreEqual(0, files.Length);
        }


        [Test]
        public void ShouldRunBucket3()
        {
            TestBase(
                new JUnitReport(),
                new RunScenario(new Comment("Scenario1"), new IsTrue(true) { Expect = false, Message = "FailureMessage" }),
                new RunScenario(new Comment("Scenario2"))
                );
            Assert.AreEqual(Program.OkStatusCode, returnCode);
            Assert.AreEqual(2, files.Length);
            Assert.IsTrue(File.ReadAllText(files.First().FullName).Contains("Comment: Scenario1"));
            Assert.IsTrue(File.ReadAllText(files.First().FullName).Contains("FailureMessage"));
            Assert.IsTrue(File.ReadAllText(files.Last().FullName).Contains("Comment: Scenario2"));
            Assert.IsFalse(File.ReadAllText(files.Last().FullName).Contains("FailureMessage"));
        }
    }
}
