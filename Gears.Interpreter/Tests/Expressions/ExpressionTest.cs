using System.IO;
using System.Linq;
using Gears.Interpreter.App;
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Tests.Pages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Gears.Interpreter.Tests.Expressions
{
    public class ExpressionTest
    {
        private readonly string _file = FileFinder.Find("ExpressionTestPage.html");

        [TearDown]
        public void TearDown()
        {
            TestBootstrapper.TearDown();
        }

        [Test]
        public void Should_UseVariableInFill()
        {
            var interpreter = TestBootstrapper.Setup();

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));
            Should.Be<SuccessAnswer>(interpreter.Please("remember textFieldName 'TextArea1'"));
            Should.Be<SuccessAnswer>(interpreter.Please("fill [textFieldName] with SampleText"));
            Should.Have("SampleText").InFieldWithId("IdTextArea1");
        }

        [Test]
        public void Should_UseVariableInFill_UsingFile()
        {
            var interpreter = TestBootstrapper.Setup(
                @"Discriminator,    What,               Text
                  Fill,             [textFieldName],    SampleText
                ");

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));
            Should.Be<SuccessAnswer>(interpreter.Please("remember textFieldName 'TextArea1'"));
            Should.Be<SuccessAnswer>(interpreter.Please(App.Interpreter.RUN_NEXT_ITEM_IN_PLAN));
            Should.Have("SampleText").InFieldWithId("IdTextArea1");
        }

        [Test]
        public void Should_UseVariableInFill_UsingFile_with_differentVariable()
        {
            var interpreter = TestBootstrapper.Setup(
                @"Discriminator,    What,               Text
                  Fill,             TextArea1,          [text]
                ");

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));
            Should.Be<SuccessAnswer>(interpreter.Please("remember text SampleText"));
            Should.Be<SuccessAnswer>(interpreter.Please(App.Interpreter.RUN_NEXT_ITEM_IN_PLAN));
            Should.Have("SampleText").InFieldWithId("IdTextArea1");
        }

        [Test]
        public void Should_UseVariableInFill_UsingFile_with_differentVariable2()
        {
            var interpreter = TestBootstrapper.Setup(
                @"Discriminator,    What
                  Fill,             TextArea1 with [text]
                ");

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));
            Should.Be<SuccessAnswer>(interpreter.Please("remember text SampleText"));
            Should.Be<SuccessAnswer>(interpreter.Please(App.Interpreter.RUN_NEXT_ITEM_IN_PLAN));
            Should.Have("SampleText").InFieldWithId("IdTextArea1");
        }

        [Test]
        public void CanEvaluate()
        {
            var interpreter = TestBootstrapper.Setup(
                "Discriminator, Url, What, Expect\n" +
                "GoToUrl,       {\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n" +
                "GoToUrl,       {\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n");

            Should.Be<SuccessAnswer>(interpreter.Please("skip"));

            Assert.IsFalse((interpreter.Plan.First() as Keyword).IsHydrated);
            Assert.IsFalse((interpreter.Plan.Last() as Keyword).IsHydrated);

            Should.Be<SuccessAnswer>(interpreter.Please("eval"));

            Assert.IsFalse((interpreter.Plan.First() as Keyword).IsHydrated);
            Assert.IsTrue((interpreter.Plan.Last() as Keyword).IsHydrated);


            TestBootstrapper.ModifyScenario(
                "Discriminator, Url, What, Expect\n" +
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n" +
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n" +
                "GoToUrl,{\"file:///\"+FileFinder.Find(\"Iteration1TestPageRelativeButtons.html\")},,\n");

            Should.Be<SuccessAnswer>(interpreter.Please("reload"));

            Assert.AreEqual(3, interpreter.Plan.Count());
            Assert.IsFalse(((Keyword) interpreter.Plan.First()).IsHydrated);
            Assert.IsFalse(((Keyword) interpreter.Plan.Last()).IsHydrated);
            Assert.AreEqual(1, interpreter.Iterator.Index);

            Should.Be<SuccessAnswer>(interpreter.Please("eval"));

            Assert.IsFalse(((Keyword) interpreter.Plan.First()).IsHydrated);
            Assert.IsTrue(((Keyword) interpreter.Plan.Skip(1).First()).IsHydrated);
            Assert.IsFalse(((Keyword) interpreter.Plan.Last()).IsHydrated);
        }

        [Test]
        public void ShouldWorkWithSpecialCharacters()
        {
            var interpreter = TestBootstrapper.Setup();

            Should.Be<SuccessAnswer>(interpreter.Please("Remember Variable1 Hello world"));

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));

            Should.Be<SuccessAnswer>(interpreter.Please("fill TextArea1 with [Variable1]"));
            Should.Have("Hello world").InFieldWithId("IdTextArea1");
            interpreter.Please("clear TextArea1");

            Should.Be<SuccessAnswer>(interpreter.Please("fill TextArea1 with \"Hello world\""));
            Should.Have("\"Hello world\"").InFieldWithId("IdTextArea1");
            interpreter.Please("clear TextArea1");

            Should.Be<SuccessAnswer>(interpreter.Please("fill TextArea1 with {hello}"));
            Should.Have("{hello}").InFieldWithId("IdTextArea1");
            interpreter.Please("clear TextArea1");

            Should.Be<SuccessAnswer>(interpreter.Please("fill TextArea1 with {[Variable1]}"));
            Should.Have("{Hello world}").InFieldWithId("IdTextArea1");
            interpreter.Please("clear TextArea1");
        }
    }
}