using System;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Library;
using NUnit.Framework;
using OpenQA.Selenium;

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
            var instruction = new Instruction("like 'something long' above 'something even longer' with ' an absurdely long text with numb3rs and stuff'");
            Assert.AreEqual(Accuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("something long", instruction.SubjectName);
            Assert.AreEqual("something even longer", instruction.Locale);
            Assert.AreEqual(SearchDirection.AboveAnotherElement, instruction.Direction);
            Assert.AreEqual("an absurdely long text with numb3rs and stuff", instruction.With);
        }

        [Test]
        public void ShouldParseInstruction_Bug()
        {
            var instruction = new Instruction("2nd 'Input Name' from top");
            Assert.AreEqual(1, instruction.Order);
            Assert.AreEqual(Accuracy.Exact, instruction.Accuracy);
            Assert.AreEqual("Input Name", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.DownFromTopEdge, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_Bug2()
        {
            var instruction = new Instruction("'7 dwarfs' from top");
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual("7 dwarfs", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.DownFromTopEdge, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified2()
        {
            var instruction = new Instruction("1st button like submit like");
            Assert.AreEqual(Accuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("submit like", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified4()
        {
            var instruction = new Instruction("3. button like 'Add to'");
            Assert.AreEqual(2, instruction.Order);
            Assert.AreEqual(Accuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified5()
        {
            var instruction = new Instruction("button like 'Add to'");
            Assert.AreEqual(Accuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified7()
        {
            var instruction = new Instruction("button like Add to");
            Assert.AreEqual(Accuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified6()
        {
            var instruction = new Instruction("button like 'Add to' left from 'Something'");
            Assert.AreEqual(Accuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual("Something", instruction.Locale);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified3()
        {
            var instruction = new Instruction("1st button 'like submit'");
            Assert.AreEqual(Accuracy.Exact, instruction.Accuracy);
            Assert.AreEqual("like submit", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithNumber()
        {
            var instruction = new Instruction("12 records");
            Assert.AreEqual(Accuracy.Exact, instruction.Accuracy);
            Assert.AreEqual("12 records", instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithKeyWordSubstring()
        {
            var instruction = new Instruction("1st Inputs from left");
            Assert.AreEqual("Inputs", instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual(SubjectType.Any, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithKeyWordSubstring3()
        {
            var instruction = new Instruction("1st Input s from left");
            Assert.AreEqual("s", instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual(SubjectType.Input, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithKeyWordSubstring2()
        {
            var instruction = new Instruction("1st Input");
            Assert.AreEqual("", instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual(SubjectType.Input, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, instruction.Direction);
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
