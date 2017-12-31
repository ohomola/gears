using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Data.Serialization.Mapping.LazyResolving;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Library.UI;
using Gears.Interpreter.Tests.Pages;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Instructions
{
    public class InstructionTest
    {
        [Test]
        public void ShouldParseInstructionCorrectly1()
        {
            Assert.AreEqual("blah", new WebElementInstruction("blah").SubjectName);

            var instruction = new WebElementInstruction("blah left from bleh with aa");
            Assert.AreEqual("blah", instruction.SubjectName);
            Assert.AreEqual("bleh", instruction.Locale);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
            Assert.AreEqual("aa", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly2()
        {
            var instruction = new WebElementInstruction("'something long' above 'something even longer' with ' an absurdely long text with numb3rs and stuff'");
            Assert.AreEqual("something long", instruction.SubjectName);
            Assert.AreEqual("something even longer", instruction.Locale);
            Assert.AreEqual(SearchDirection.AboveAnotherElement, instruction.Direction);
            Assert.AreEqual("an absurdely long text with numb3rs and stuff", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly3()
        {
            var instruction = new WebElementInstruction("something long above something even longer with ' an absurdely long text with numb3rs and stuff'");
            Assert.AreEqual("something long", instruction.SubjectName);
            Assert.AreEqual("something even longer", instruction.Locale);
            Assert.AreEqual(SearchDirection.AboveAnotherElement, instruction.Direction);
            Assert.AreEqual("an absurdely long text with numb3rs and stuff", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly4()
        {
            var instruction = new WebElementInstruction("blah blah with bluh bluh");
            Assert.AreEqual("blah blah", instruction.SubjectName);
            Assert.AreEqual("bluh bluh", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly5()
        {
            var instruction = new WebElementInstruction("2nd button");
            Assert.AreEqual(null, instruction.SubjectName);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
            Assert.AreEqual(1, instruction.Order);
        }

        [Test]
        public void ShouldParseInstructionCorrectly6()
        {
            var instruction = new WebElementInstruction("4 textfield blah left from Password with 'Hello'");
            Assert.AreEqual(null, instruction.Order);
            Assert.AreEqual(null, instruction.SubjectType);
            Assert.AreEqual("4 textfield blah", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
            Assert.AreEqual("Password", instruction.Locale);
            Assert.AreEqual("Hello", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly6b()
        {
            var instruction = new WebElementInstruction("2nd textfield 4 blah 4 left from Password with 'Hello'");
            Assert.AreEqual(1, instruction.Order);
            Assert.AreEqual(WebElementType.Input, instruction.SubjectType);
            Assert.AreEqual("4 blah 4", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
            Assert.AreEqual("Password", instruction.Locale);
            Assert.AreEqual("Hello", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly7()
        {
            var instruction = new WebElementInstruction("4. link from right");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual(null, instruction.SubjectName);
            Assert.AreEqual(WebElementType.Link, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.LeftFromRightEdge, instruction.Direction);
            Assert.AreEqual(null, instruction.Locale);
        }

        [Test]
        public void ShouldParseInstructionCorrectly8()
        {
            var instruction = new WebElementInstruction("4. button from left");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
            Assert.AreEqual(null, instruction.Locale);
        }

        [Test]
        public void ShouldParseInstructionCorrectly9()
        {
            var instruction = new WebElementInstruction("4. button login from left");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual("login", instruction.SubjectName);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
            Assert.AreEqual(null, instruction.Locale);
        }

        [Test]
        public void ShouldParseInstructionCorrectly10()
        {
            var instruction = new WebElementInstruction("223421th input from bottom with flowers");
            Assert.AreEqual(223420, instruction.Order);
            Assert.AreEqual(null, instruction.SubjectName);
            Assert.AreEqual(WebElementType.Input, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.UpFromBottomEdge, instruction.Direction);
            Assert.AreEqual(null, instruction.Locale);
            Assert.AreEqual("flowers", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly11()
        {
            var instruction = new WebElementInstruction("1st button 'blah' from bottom with flowers");
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual("blah", instruction.SubjectName);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.UpFromBottomEdge, instruction.Direction);
            Assert.AreEqual(null, instruction.Locale);
            Assert.AreEqual("flowers", instruction.With);
        }

        [Test]
        public void Should_NotOverwriteValues()
        {
            var interpreter = TestBootstrapper.Setup(
                @"Discriminator,    What
                  Fill,             {""TextArea1 with SampleText""}
                ");

            var fill = interpreter.Plan.First() as Fill;
            fill.Hydrate();
            Assert.AreEqual("SampleText", fill.Text);
            Assert.AreEqual("TextArea1", fill.LabelText);
        }

        [Test]
        public void Should_BeAbleTo_ParseSpecification()
        {
            var interpreter = TestBootstrapper.Setup(
                @"Discriminator,    Instruction
                  Fill,             1st TextArea1 from left with SampleText
                ");
            Assert.AreEqual("SampleText", interpreter.Plan.OfType<Fill>().First().Text);
            Assert.AreEqual("TextArea1", interpreter.Plan.OfType<Fill>().First().LabelText);
            
        }

        [Test]
        public void ShouldParseInstruction_Bug2()
        {
            var instruction = new WebElementInstruction("'7 dwarfs' from top");
            Assert.AreEqual(null, instruction.Order);
            Assert.AreEqual("7 dwarfs", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.DownFromTopEdge, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified2()
        {
            var instruction = new WebElementInstruction("1st button like submit like");
            Assert.AreEqual(CompareAccuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("submit like", instruction.SubjectName);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified4()
        {
            var instruction = new WebElementInstruction("3. button like 'Add to'");
            Assert.AreEqual(2, instruction.Order);
            Assert.AreEqual(CompareAccuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
            Assert.AreEqual(null, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified5()
        {
            var instruction = new WebElementInstruction("button like 'Add to'");
            Assert.AreEqual(CompareAccuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified7()
        {
            var instruction = new WebElementInstruction("button like Add to");
            Assert.AreEqual(CompareAccuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified6()
        {
            var instruction = new WebElementInstruction("button like 'Add to' left from 'Something'");
            Assert.AreEqual(CompareAccuracy.Partial, instruction.Accuracy);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual("Something", instruction.Locale);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithAccuracySpecified3()
        {
            var instruction = new WebElementInstruction("1st button 'like submit'");
            Assert.AreEqual(CompareAccuracy.Exact, instruction.Accuracy);
            Assert.AreEqual("like submit", instruction.SubjectName);
        }

        [Test]
        public void ShouldParseInstruction_WithNumber()
        {
            var instruction = new WebElementInstruction("12 records");
            Assert.AreEqual(CompareAccuracy.Exact, instruction.Accuracy);
            Assert.AreEqual("12 records", instruction.SubjectName);
            Assert.AreEqual(null, instruction.Order);
            Assert.AreEqual(null, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithKeyWordSubstring()
        {
            var instruction = new WebElementInstruction("1st Inputs from left");
            Assert.AreEqual("Inputs", instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual(null, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithKeyWordSubstring3()
        {
            var instruction = new WebElementInstruction("1st Input s from left");
            Assert.AreEqual("s", instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual(WebElementType.Input, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithKeyWordSubstring2()
        {
            var instruction = new WebElementInstruction("1st Input");
            Assert.AreEqual(null, instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual(WebElementType.Input, instruction.SubjectType);
            Assert.AreEqual(null, instruction.Direction);
        }

        [Test]
        public void ShouldParseInstruction_WithArea()
        {
            var instruction = new WebElementInstruction("1st Input inside left center");
            Assert.AreEqual(null, instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual(WebElementType.Input, instruction.SubjectType);
            Assert.AreEqual(null, instruction.Direction);
            Assert.AreEqual("left center", instruction.Area);
        }

        [Test]
        public void ShouldParseInstruction_WithArea2()
        {
            var instruction = new WebElementInstruction("45th button like 'Add to' left from 'Something' inside Marshmallow village with Green Butter");
            Assert.AreEqual(CompareAccuracy.Partial, instruction.Accuracy);
            Assert.AreEqual(44, instruction.Order);
            Assert.AreEqual("Add to", instruction.SubjectName);
            Assert.AreEqual("Something", instruction.Locale);
            Assert.AreEqual(WebElementType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
            Assert.AreEqual("Green Butter", instruction.With);
            Assert.AreEqual("Marshmallow village", instruction.Area);
        }

        [Test]
        [Ignore("Known Bug")]
        public void ShouldParseInstruction_WithKeyWordSubstring4()
        {
            Assert.AreEqual("within", new WebElementInstruction("1st withinside").SubjectName);

            Assert.AreEqual("fillin", new WebElementInstruction("1st button fillinside").SubjectName);
        }

        [Test]
        public void ShouldParseInstruction_WithKeyWordSubstring5()
        {
            var instruction = new WebElementInstruction("1st 'inwith' with 'inside'");
            Assert.AreEqual("inwith", instruction.SubjectName);
            Assert.AreEqual(0, instruction.Order);
            Assert.AreEqual("inside", instruction.With);
        }

        // Test for potential mismatches with 'In' control word
        [Test]
        public void ShouldParseInstruction_WithKeyWordSubstring6()
        {
            Assert.AreEqual("blink", new WebElementInstruction("1st button blink").SubjectName);

            Assert.AreEqual("interceptor", new WebElementInstruction("1st button interceptor").SubjectName);
        }

    }
}
