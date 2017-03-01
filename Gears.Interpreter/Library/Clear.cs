using System;
using System.Xml.Serialization;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Library.Documentations;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public class Clear : Keyword
    {
        public virtual string LabelText { get; set; }
        public virtual SearchDirection Direction { get; set; }
        public virtual int Order { get; set; }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }


        public override IKeyword FromString(string textInstruction)
        {
            return new Clear(ExtractSingleParameterFromTextInstruction(textInstruction));
        }

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Clears a text input element (or dropdown) located by a visible text on the screen. The input parameter is a query instruction passed as a string to parameter **What**. 

See [Fill](#fill) for more info.

#### Scenario usages
| Discriminator | What  | 
| ------------- | ----- | 
| Clear          | login |     
| Clear          | input right from login |

#### Console usages
    clear

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        public Clear()
        {
        }

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

        public override object DoRun()
        {
            try
            {
                var searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);

                var lookupResult = searchStrategy.DirectLookupWithNeighbours(LabelText, Direction, Order);

                if (lookupResult.Success == false)
                {
                    throw new LookupFailureException(lookupResult, "Input not found");
                }

                lookupResult.Result.WebElement.SendKeys(Keys.LeftControl + "a");
                lookupResult.Result.WebElement.SendKeys(Keys.Delete);
                return lookupResult.Result.WebElement;
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