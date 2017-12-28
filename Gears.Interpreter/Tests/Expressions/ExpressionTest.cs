using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Tests.Pages;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Expressions
{
    public class ExpressionTest
    {
        [TearDown]
        public void TearDown()
        {
            TestBootstrapper.TearDown();
        }

        [Test]
        public void Should_UseVariableInFill()
        {
            var interpreter = TestBootstrapper.Setup();

            interpreter.Please($"gotourl file:///{FileFinder.Find("ExpressionTestPage.html")}");
            interpreter.Please("remember textFieldName 'TextArea1'");
            interpreter.Please("fill [textFieldName] with SampleText");
            Should.Have("SampleText").InFieldWithId("IdTextArea1");
        }

        [Test]
        public void Should_UseVariableInFill_UsingFile()
        {
            var interpreter = TestBootstrapper.Setup(
                @"Discriminator,    What,               Text
                  Fill,             [textFieldName],    SampleText
                ");

            interpreter.Please($"gotourl file:///{FileFinder.Find("ExpressionTestPage.html")}");
            interpreter.Please("remember textFieldName 'TextArea1'");
            interpreter.Please(App.Interpreter.RUN_NEXT_ITEM_IN_PLAN);
            Should.Have("SampleText").InFieldWithId("IdTextArea1");
        }

        [Test]
        public void Should_UseVariableInFill_UsingFile_with_differentVariable()
        {
            var interpreter = TestBootstrapper.Setup(
                @"Discriminator,    What,               Text
                  Fill,             TextArea1,          [text]
                ");

            interpreter.Please($"gotourl file:///{FileFinder.Find("ExpressionTestPage.html")}");
            interpreter.Please("remember text SampleText");
            interpreter.Please(App.Interpreter.RUN_NEXT_ITEM_IN_PLAN);
            Should.Have("SampleText").InFieldWithId("IdTextArea1");
        }

        [Test]
        public void Should_UseVariableInFill_UsingFile_with_differentVariable2()
        {
            var interpreter = TestBootstrapper.Setup(
                @"Discriminator,    What
                  Fill,             TextArea1 with [text]
                ");

            interpreter.Please($"gotourl file:///{FileFinder.Find("ExpressionTestPage.html")}");
            interpreter.Please("remember text SampleText");
            interpreter.Please(App.Interpreter.RUN_NEXT_ITEM_IN_PLAN);
            Should.Have("SampleText").InFieldWithId("IdTextArea1");
        }
    }
}