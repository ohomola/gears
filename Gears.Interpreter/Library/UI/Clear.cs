using System;
using System.Xml.Serialization;
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library.UI
{
    public class Clear : Keyword, IHasTechnique
    {
        public virtual string LabelText { get; set; }
        public virtual SearchDirection Direction { get; set; }
        public virtual int Order { get; set; }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

        public Technique Technique { get; set; }

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
            ExactMatch = spec.Accuracy != Accuracy.Partial;
        }

        public bool ExactMatch { get; set; }

        public override object DoRun()
        {
            try
            {
                //var lookupResult = searchStrategy.DirectLookupWithNeighbours(LabelText, Direction, Order);
                var lookupResult = new TextFieldLookupStrategy(Selenium, ExactMatch, Order, Direction, LabelText).LookUp();

                if (lookupResult.Success == false)
                {
                    throw new LookupFailureException(lookupResult, "Input not found");
                }

                switch (Technique)
                {
                    case Technique.HighlightOnly:
                        Highlighter.HighlightElements(Selenium, lookupResult.AllValidResults);
                        return new InformativeAnswer("Highlighting complete.");
                    case Technique.MouseAndKeyboard:
                        lookupResult.MainResult.WebElement.SendKeys(Keys.LeftControl + "a");
                        lookupResult.MainResult.WebElement.SendKeys(Keys.Delete);
                        return "Erased";
                }

                return "";
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