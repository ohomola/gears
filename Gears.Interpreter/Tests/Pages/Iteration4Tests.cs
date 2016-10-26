using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gears.Interpreter.Library;
using NUnit.Framework;

namespace Gears.Interpreter.Tests.Pages
{
    public class Iteration4Tests
    {
        [Test]
        public void ShouldParseInstructionCorrectly1()
        {
            Assert.AreEqual("blah",new Instruction("blah").SubjectName);

            var instruction = new Instruction("blah left from bleh with aa");
            Assert.AreEqual("blah", instruction.SubjectName);
            Assert.AreEqual("bleh", instruction.Locale);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
            Assert.AreEqual("aa", instruction.With);

        }

        [Test]
        public void ShouldParseInstructionCorrectly2()
        {
            var instruction = new Instruction("'something long' above 'something even longer' with ' an absurdely long text with numb3rs and stuff'");
            Assert.AreEqual("something long", instruction.SubjectName);
            Assert.AreEqual("something even longer", instruction.Locale);
            Assert.AreEqual(SearchDirection.Up, instruction.Direction);
            Assert.AreEqual("an absurdely long text with numb3rs and stuff", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly3()
        {
            var instruction = new Instruction("something long above something even longer with ' an absurdely long text with numb3rs and stuff'");
            Assert.AreEqual("something long", instruction.SubjectName);
            Assert.AreEqual("something even longer", instruction.Locale);
            Assert.AreEqual(SearchDirection.Up, instruction.Direction);
            Assert.AreEqual("an absurdely long text with numb3rs and stuff", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly4()
        {
            var instruction = new Instruction("blah blah with bluh bluh");
            Assert.AreEqual("blah blah", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusive, instruction.Direction);
            Assert.AreEqual("bluh bluh", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly5()
        {
            var instruction = new Instruction("2nd button");
            Assert.AreEqual(string.Empty, instruction.SubjectName);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromAnotherElementInclusive, instruction.Direction);
            Assert.AreEqual(1, instruction.Order);
        }

        [Test]
        public void ShouldParseInstructionCorrectly6()
        {
            var instruction = new Instruction("4 textfield left from Password with 'Hello'");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual("textfield", instruction.SubjectName);
            Assert.AreEqual(SearchDirection.LeftFromAnotherElement, instruction.Direction);
            Assert.AreEqual("Password", instruction.Locale);
            Assert.AreEqual("Hello", instruction.With);
        }

        [Test]
        public void ShouldParseInstructionCorrectly7()
        {
            var instruction = new Instruction("4. link from right");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual(string.Empty, instruction.SubjectName);
            Assert.AreEqual(SubjectType.Link, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.LeftFromRightEdge, instruction.Direction);
            Assert.AreEqual(string.Empty, instruction.Locale);
        }

        [Test]
        public void ShouldParseInstructionCorrectly8()
        {
            var instruction = new Instruction("4. button from left");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
            Assert.AreEqual(string.Empty, instruction.Locale);
        }

        [Test]
        public void ShouldParseInstructionCorrectly9()
        {
            var instruction = new Instruction("4. login button from left");
            Assert.AreEqual(3, instruction.Order);
            Assert.AreEqual("login", instruction.SubjectName);
            Assert.AreEqual(SubjectType.Button, instruction.SubjectType);
            Assert.AreEqual(SearchDirection.RightFromLeftEdge, instruction.Direction);
            Assert.AreEqual(string.Empty, instruction.Locale);
        }

    }
}
