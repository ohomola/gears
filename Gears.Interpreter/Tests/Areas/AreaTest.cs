using Gears.Interpreter.App;
using Gears.Interpreter.App.Registrations;
using Gears.Interpreter.Core.Data.Core;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Pages
{
    public class AreaTest
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
        public void ShouldFindOneLabel_WhenSplitByAreas()
        {
            _interpreter.Please($"gotourl file:///{FileFinder.Find("AreaTestPage.html")}");

       //     _interpreter.Please("fill 2nd textfield like 'TextArea' from bottom in left with SampleText");
            //_interpreter.Please("fill 4th textfield from top with SampleText");

//            Should.Have("SampleText").InFieldWithId("IdTextArea3Left");


            //_interpreter.Please("isvisible Card in left expect false");
            //_interpreter.Please("isvisible Card in left menu expect false");

            //_interpreter.Please("Remember BurgerMenu horizontal div with Material-UI");
            //_interpreter.Please("isvisible Card in BurgerMenu expect false");
        }
    }
}