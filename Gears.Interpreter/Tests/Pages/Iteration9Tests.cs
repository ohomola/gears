using System.Diagnostics;
using System.Linq;
using Gears.Interpreter.App;
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Interpretation;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration9Tests
    {
        private IInterpreter _interpreter;

        [SetUp]
        public void SetUp()
        {
            Bootstrapper.Register();
            _interpreter = Bootstrapper.ResolveInterpreter();
        }

        [TearDown]
        public void TearDown()
        {
            Bootstrapper.Release();
        }

        [Test]
        public void ShoudBeAbleToLoadObject()
        {
            Assert.IsInstanceOf<SuccessAnswer>(_interpreter.Please("use rememberedtext"));
            _interpreter.Please("stop");
        }

        [Test, Ignore("No db present")]
        public void ShoudBeAbleTo_LoadDataFromDatabase()
        {
            Should.Be<SuccessAnswer>(_interpreter.Please("use connectionstring user id=sitecore;password=scv4;Data Source=.\\sqlexpress;Database=Ori.Online.Sitecore.Master;Enlist=false"));
            Should.Be<InformativeAnswer>(_interpreter.Please("comment {SQL.Select(\"Select top 1 TITLE from GearsTestDatabase\")} "));
            _interpreter.Please("stop");
        }

        [Test]
        public void ShoudBeAbleTo_UseConnectionString()
        {
            Should.Be<SuccessAnswer>(_interpreter.Please("use connectionString abc bcd"));
            Assert.AreEqual("abc bcd", _interpreter.Data.Get<ConnectionString>().Text);
            _interpreter.Please("stop");
        }

        [Test]
        public void ShoudBeAbleTo_SwitchBetweenChromeAndFirefoxAndIE()
        {
            Should.Be<SuccessAnswer>(_interpreter.Please("use firefox"));
            Should.Be<SuccessAnswer>(_interpreter.Please("gotourl http://www.google.com"));
            
            Assert.IsTrue(Process.GetProcessesByName("firefox").Any());
            Should.Be<SuccessAnswer>(_interpreter.Please("use chrome"));
            Assert.IsFalse(Process.GetProcessesByName("firefox").Any());
            Assert.IsTrue(Process.GetProcesses().Any(x=>x.MainWindowTitle.Contains("Google Chrome")));

            Should.Be<SuccessAnswer>(_interpreter.Please("use internetexplorer"));
            Assert.IsFalse(Process.GetProcesses().Any(x => x.MainWindowTitle.Contains("Google Chrome")));
            Assert.IsTrue(Process.GetProcessesByName("iexplore").Any());

            _interpreter.Please("stop");

            Assert.IsFalse(Process.GetProcessesByName("firefox").Any());
            Assert.IsFalse(Process.GetProcesses().Any(x => x.MainWindowTitle.Contains("Google Chrome")));
        }

        /// <summary>
        /// Uses pre-built KeywordPlugins.dll that's next to test pages. With future breaking changes it might need to be rebuilt as a project using below definition (or similar):
        /// public class RunCalculator : Keyword, IGearsPlugin
        //{
        //    public override object DoRun()
        //    {
        //        Console.Out.WriteColoredLine(ConsoleColor.Cyan, "Running calculator");
        //        return new SuccessAnswer("OK");
        //    }
        //}
        /// </summary>
        [Test, Ignore("Plugin assembly reload gets broken by refactoring")]
        public void ShouldBeAbleToLoadPlugins()
        {
            Should.Be<ExceptionAnswer>(_interpreter.Please("runcalculator"));
            Should.Be<SuccessAnswer>(_interpreter.Please("open UnitTestKeywordPlugins.dll"));
            Should.Be<SuccessAnswer>(_interpreter.Please("RunUnitTestPluginSample"));
            Should.Be<SuccessAnswer>(_interpreter.Please("isvisible google"));
            _interpreter.Please("stop");
        }
    }
}
