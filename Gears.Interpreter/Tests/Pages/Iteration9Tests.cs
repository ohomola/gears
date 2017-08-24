using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.Core.Interpretation;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration9Tests
    {

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
    [Test]
        public void ShouldBeAbleToLoadPlugins()
        {
            Bootstrapper.Register();
            var interpreter = Bootstrapper.ResolveInterpreter();

            Assert.IsInstanceOf<ExceptionAnswer>(interpreter.Please("runcalculator"));
            Assert.IsInstanceOf<SuccessAnswer>(interpreter.Please("open UnitTestKeywordPlugins.dll"));
            Assert.IsInstanceOf<SuccessAnswer>(interpreter.Please("RunUnitTestPluginSample"));
            Assert.IsInstanceOf<SuccessAnswer>(interpreter.Please("isvisible google"));
            interpreter.Please("stop");
        }
    }
}
