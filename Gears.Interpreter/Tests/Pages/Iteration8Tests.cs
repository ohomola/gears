using System;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Library.UI;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration8Tests : IDisposable
    {
        private SeleniumAdapter _selenium;

        public Iteration8Tests()
        {
            SetUp();
        }
        public void Dispose()
        {
            TearDown();
        }

        //[SetUp]
        public void SetUp()
        {
            _selenium =
                new SeleniumAdapter();
        }

        //[TearDown]
        public void TearDown()
        {
            _selenium?.Dispose();
        }

        [Test]
        public void ShouldBeAbleTo_DetectBySubstring()
        {
            new GoToUrl($"http://www.material-ui.com/#/components/snackbar")
            {
                Selenium = _selenium
            }.Execute();

            

            var isVisible = (bool)new IsVisible("button like Add to my") { Selenium = _selenium, WaitAfter = 700 }.Execute();
            Assert.IsTrue(isVisible);

            var result = new Click("button like Add to my") { Selenium = _selenium, WaitAfter = 700 }.Execute();

            Assert.IsTrue((bool)new IsVisible("Event added to your calendar") { Selenium = _selenium, WaitAfter = 700 }.Execute());

            Assert.IsTrue((bool) new IsVisible("3. button like 'Add to'") { Selenium = _selenium, WaitAfter = 700 }.Execute());
            Assert.IsFalse((bool)new IsVisible("4. button like 'Add to'") { Selenium = _selenium, WaitAfter = 700 }.Execute());
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified()
        {
            var instruction = new WebElementInstruction("like 'something long' above 'something even longer' with ' an absurdely long text with numb3rs and stuff'");
            Assert.AreEqual(CompareAccuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("something long", instruction.SubjectName);
            Assert.AreEqual("something even longer", instruction.Locale);
            Assert.AreEqual(SearchDirection.AboveAnotherElement, instruction.Direction);
            Assert.AreEqual("an absurdely long text with numb3rs and stuff", instruction.With);
        }

        [Test]
        public void ShouldParseInstruction_Bug()
        {
            var instruction = new WebElementInstruction("2nd 'Input Name' from top");
            Assert.AreEqual(1, instruction.Order);
            Assert.AreEqual(CompareAccuracy.Exact, instruction.Accuracy);
            Assert.AreEqual("Input Name", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.DownFromTopEdge, instruction.Direction);
        }

        
        


      

        [Ignore("Not sure how to do this with react checkboxes")]
        [Test]
        public void ShouldBeAbleTo_CheckSelected()
        {
            new GoToUrl($"http://www.material-ui.com/#/components/checkbox")
            {
                Selenium = _selenium
            }.Execute();

            var check = new IsSelected("Simple") { Expect = false, Selenium = _selenium, WaitAfter = 700 }.Execute();

            Assert.IsInstanceOf<SuccessAnswer>(check);

            try
            {
                check = new IsSelected("Simple") {Expect = true, Selenium = _selenium, WaitAfter = 700}.Execute();
                Assert.Fail("No exception thrown");
            }
            catch (AssertionException)
            {
                Assert.Fail("No exception thrown");
            }
            catch (Exception)
            {
                // expected
            }

            check = new IsSelected("Simple") { Selenium = _selenium, WaitAfter = 700 }.Execute();
            Assert.IsFalse((bool?) check);

            var result = new Click("Simple") { Selenium = _selenium, WaitAfter = 700 }.Execute();

            check = new IsSelected("Simple") { Selenium = _selenium, WaitAfter = 700 }.Execute();
            Assert.IsTrue((bool?)check);

            //new Wait(700).Execute();
            //new Click("1st Components from left") { Selenium = _selenium }.Execute();
            //new Wait(700).Execute();
            //new Click("1st Drawer from left") { Selenium = _selenium }.Execute();

            //try
            //{
            //    new Click("button Menu Item 2") { Selenium = _selenium }.Execute();
            //    Assert.Fail("Should throw exception when clicking on item in offset burger menu");
            //}
            //catch (Exception)
            //{
            //}
            //new IsVisible("Menu Item 2 left from Examples") { Selenium = _selenium, Expect = false }.Execute();
            //new Click("1st Toggle drawer from top") { Selenium = _selenium }.Execute();
            //new Wait(1200).Execute();
            //new Click("Menu Item 2 from left") { Selenium = _selenium }.Execute();
            //new Wait(900).Execute();

        }
    }
}
