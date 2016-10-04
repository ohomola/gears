using System;
using System.Xml.Serialization;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public class Clear : Keyword
    {
        public string What { get; set; }
        public string Where { get; set; }
        public string Text { get; set; }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

        public Clear(string what)
        {
            What = what;
        }

        public override object Run()
        {
            try
            {
                var element = Selenium.WebDriver.FindInput(What, Where);
                element.SendKeys(Keys.LeftControl + "a");
                element.SendKeys(Keys.Delete);
                return element;
            }
            catch (Exception)
            {
                throw new ApplicationException($"Element {What} was not found");
            }
        }



        public override string ToString()
        {
            return $"Fill {Where} '{What}'  with '{Text}'";
        }
    }
}