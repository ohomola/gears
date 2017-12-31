using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core.Data.Core;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Tests.Pages;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Library.Show
{
    public class ShowTest
    {
        private readonly string _file = FileFinder.Find("ShowTestPage.html");

        [TearDown]
        public void TearDown()
        {
            TestBootstrapper.TearDown();
        }

        [Test]
        public void ShouldBeAbleTo_CallOtherTechniques()
        {
            var interpreter = TestBootstrapper.Setup();

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));
            Assert.IsTrue(interpreter.Please("show clear TextArea1").Children.First() is OverlayAnswer);
            Thread.Sleep(500);
            Should.Be<OverlayAnswer>(interpreter.Please("show dragAndDrop 100 200 Button1").Children.First());
        }

        [Test]
        public void ShouldBeAbleTo_ShowTwiceInARow()
        {
            var interpreter = TestBootstrapper.Setup();

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));

            Should.Be<SuccessAnswer>(interpreter.Please("show fill TextArea1"));
            Thread.Sleep(500);
            Should.Be<SuccessAnswer>(interpreter.Please("show fill TextArea1"));
        }

        [Test]
        public void ShouldBeAbleTo_ShowClick()
        {
            var interpreter = TestBootstrapper.Setup(
                @"  Discriminator,  What
                    Show,           Click Button1");

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));

            var answer = interpreter.Please(App.Interpreter.RUN_NEXT_ITEM_IN_PLAN);
            Should.Be<SuccessAnswer>(answer);
            Thread.Sleep(2000);
            Assert.IsTrue(answer.Children.First() is OverlayAnswer);
            Assert.AreEqual(46, (answer.Children.First() as OverlayAnswer).Artifacts.First().Rectangle.Width);
            Assert.AreEqual(16, (answer.Children.First() as OverlayAnswer).Artifacts.First().Rectangle.X);

            //Assert.IsTrue(ServiceLocator.Instance.Resolve<IAnnotationOverlay>().IsShowing);
            //Assert.IsFalse(ServiceLocator.Instance.Resolve<IAnnotationOverlay>().Form.IsDisposed);

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl http://www.google.com"));
            Thread.Sleep(2000);
            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));
            Thread.Sleep(2000);
            //Assert.IsFalse(ServiceLocator.Instance.Resolve<IAnnotationOverlay>().IsShowing);
            //Assert.IsNull(ServiceLocator.Instance.Resolve<IAnnotationOverlay>().Form);
            answer = interpreter.Please("show click Button1");

            Assert.IsTrue(answer.Children.First() is OverlayAnswer);
            Assert.AreEqual(46, (answer.Children.First() as OverlayAnswer).Artifacts.First().Rectangle.Width);
            Assert.AreEqual(16, (answer.Children.First() as OverlayAnswer).Artifacts.First().Rectangle.X);
            Thread.Sleep(4000);
            Assert.IsTrue(ServiceLocator.Instance.Resolve<IAnnotationOverlay>().IsShowing);
        }

        [Test]
        public void ShouldBeAbleTo_ShowFill()
        {
            var interpreter = TestBootstrapper.Setup(
                @"  Discriminator,  What  
                    Fill,           TextArea1 with Test Completed!");

            Should.Be<SuccessAnswer>(interpreter.Please($"gotourl file:///{_file}"));

            var answer = interpreter.Please("show");
            Should.Be<SuccessAnswer>(answer);
            Assert.IsTrue(answer.Children.First() is OverlayAnswer);
            Assert.AreEqual(173, (answer.Children.First() as OverlayAnswer).Artifacts.First().Rectangle.Width);
            Assert.AreEqual(75, (answer.Children.First() as OverlayAnswer).Artifacts.First().Rectangle.X);
            Thread.Sleep(2000);

            Assert.IsTrue(ServiceLocator.Instance.Resolve<IAnnotationOverlay>().IsShowing);

            Should.Be<SuccessAnswer>(interpreter.Please(App.Interpreter.RUN_NEXT_ITEM_IN_PLAN));
            Thread.Sleep(500);
            Assert.IsFalse(ServiceLocator.Instance.Resolve<IAnnotationOverlay>().IsShowing);
            Should.Have("Test Completed!").InFieldWithId("IdTextArea1");
            Thread.Sleep(500);

        }
    }
}
