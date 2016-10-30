using System;
using System.Xml.Serialization;
using Castle.MicroKernel.ModelBuilder.Descriptors;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public class Clear : Keyword
    {
        private IElementSearchStrategy _searchStrategy;
        public string LabelText { get; set; }
        public SearchDirection Direction { get; set; }
        public int Order { get; set; }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

        public Clear(string what)
        {
            var spec = new Instruction(what);
            if (string.IsNullOrEmpty(spec.Locale))
            {
                LabelText = spec.SubjectName;
            }
            else
            {
                LabelText = spec.Locale;
            }

            Direction = spec.Direction;
            Order = spec.Order;
        }

        


        public override object Run()
        {
            try
            {
                _searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);

                var element = _searchStrategy.FindElementNextToAnotherElement(LabelText, Direction, true).WebElement;
                element.SendKeys(Keys.LeftControl + "a");
                element.SendKeys(Keys.Delete);
                return element;
            }
            catch (Exception)
            {
                throw new ApplicationException($"Element {LabelText} was not found");
            }
        }



        public override string ToString()
        {
            return $"Clear '{LabelText}'";
        }
    }
}