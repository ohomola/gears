using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Interpretation;
using Gears.Interpreter.Core.Registrations;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Gears.Interpreter.Tests
{
    public static class Should
    {
        public static void Be<T>(IAnswer answer)
        {
            Assert.IsInstanceOf<T>(answer, answer.Text);
        }

        public static IFluentAssertion Have(string text)
        {
            return new FluentAssertion(text);
        }
    }

    public interface IFluentAssertion
    {
        void InFieldWithId(string id);
    }

    internal class FluentAssertion : IFluentAssertion
    {
        private string _text;

        public FluentAssertion(string text)
        {
            this._text = text;
        }

        public void InFieldWithId(string id)
        {
            Assert.AreEqual(_text, ServiceLocator.Instance.Resolve<ISeleniumAdapter>().WebDriver.FindElement(By.Id(id)).GetAttribute("value"));
        }
    }
}
