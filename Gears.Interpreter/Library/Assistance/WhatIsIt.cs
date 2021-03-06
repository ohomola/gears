﻿using System.Linq;
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.Adapters.UI.JavaScripts;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.Library.Assistance
{
    [HelpDescription("whatisit \t-\t suggests an identifier for a button (selected with mouse)")]
    public class WhatIsIt : Keyword
    {
        private int _x;
        private int _y;

        public int Timeout { get; set; } = 30000;

        [Wire]
        public IBrowserOverlay BrowserOverlay { get; set; }

        public override string Instruction
        {
            set
            {
                var strings = value.Split(' ');

                if (strings.Length < 2)
                {
                    return;
                }
                _x = int.Parse(strings[0]);
                Y = int.Parse(strings[1]);
            }
        }

        public int Y { get => _y; set => _y = value; }

        public WhatIsIt()
        {
        }

        private WhatIsIt(int x, int y)
        {
            _x = x;
            Y = y;
        }

        public override object DoRun()
        {
            var clientRect = new UserBindings.RECT();
            UserBindings.GetClientRect(Selenium.BrowserHandle, ref clientRect);

            var windowRect = new UserBindings.RECT();
            UserBindings.GetWindowRect(Selenium.BrowserHandle, ref windowRect);

            if (_x == default(int))
            {
                var point = BrowserOverlay.ShowUntilClicked("Please click on an element", Timeout);
                
                //var point = AnnotationOverlay.RunUntilClicked();

                Selenium.ConvertFromGraphicsToScreen(ref point);
                Selenium.ConvertFromScreenToWindow(ref point);
                Selenium.ConvertFromWindowToPage(ref point);

                _x = point.X;
                Y = point.Y;
            }

            var elements = Selenium.WebDriver.GetElementByCoordinates(_x, Y);
            
            var element = elements.LastOrDefault(x=>x.IsClickable());
            
            if (element != null)
            {

                var instruction = new WebElementInstruction();
                instruction.Order = 0;
                if (element.Text != null)
                {
                    instruction.SubjectName = element.Text;
                }

                if (element.TagName.ToLower() == "a")
                {
                    instruction.TagNames.Add(new TagNameSelector("a"));
                    instruction.SubjectType =WebElementType.Link;
                }
                else
                {
                    instruction.TagNames.Add(new TagNameSelector("button"));
                    instruction.TagNames.Add(new AttributeSelector("type", "button"));
                    instruction.SubjectType = WebElementType.Button;
                }

                var query = new LocationHeuristictSearchStrategy(Selenium);

                var result = query.DirectLookup(
                    instruction.TagNames, 
                    instruction.SubjectName, 
                    null, 
                    SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, 0, false);

                if (result.Success)
                {
                    if (result.MainResult.WebElement.Equals(element))
                    {
                        new Remember("it", instruction.ToString()).Execute();
                        return new SuccessAnswer(instruction.ToString());
                    }
                }

                var relatives = query.Elements(new[] {"div", "span"})
                    .RelativeTo(element, SearchDirection.RightFromAnotherElement, true,
                    0, 20)
                    .Results();
                relatives = relatives.Where(x => !string.IsNullOrEmpty(x.WebElement.Text)).ToList();

                if (relatives.Any())
                {
                    var relativeInstruction = new WebElementInstruction();
                    relativeInstruction.Order = 0;
                    relativeInstruction.Direction = SearchDirection.LeftFromAnotherElement;
                    relativeInstruction.Locale = relatives.First().WebElement.Text;
                    relativeInstruction.SubjectType = instruction.SubjectType;

                    new Remember("it", relativeInstruction.ToString()).Execute();

                    return new SuccessAnswer(relativeInstruction);
                }

                relatives = query.Elements(new[] { "div", "span" })
                    .RelativeTo(element, SearchDirection.LeftFromAnotherElement, true,
                    0, 20)
                    .Results();
                relatives = relatives.Where(x => !string.IsNullOrEmpty(x.WebElement.Text)).ToList();

                if (relatives.Any())
                {
                    var relativeInstruction = new WebElementInstruction();
                    relativeInstruction.Order = 0;
                    relativeInstruction.Direction = SearchDirection.RightFromAnotherElement;
                    relativeInstruction.Locale = relatives.First().WebElement.Text;
                    relativeInstruction.SubjectType = instruction.SubjectType;

                    new Remember("it", relativeInstruction.ToString()).Execute();

                    return new SuccessAnswer(relativeInstruction);
                }

                relatives = query.Elements(new[] { "div", "span" })
                    .RelativeTo(element, SearchDirection.AboveAnotherElement, true,
                    20, 0)
                    .Results();
                relatives = relatives.Where(x => !string.IsNullOrEmpty(x.WebElement.Text)).ToList();

                if (relatives.Any())
                {
                    var relativeInstruction = new WebElementInstruction();
                    relativeInstruction.Order = 0;
                    relativeInstruction.Direction = SearchDirection.BelowAnotherElement;
                    relativeInstruction.Locale = relatives.First().WebElement.Text;
                    relativeInstruction.SubjectType = instruction.SubjectType;

                    new Remember("it", relativeInstruction.ToString()).Execute();

                    return new SuccessAnswer(relativeInstruction);
                }

            }

            return new WarningAnswer("No element on that location.");
        }

        public override string ToString()
        {
            return "What is it?";
        }

        #region Documentation
        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Prompts user to indicate a webelement of interest to the application. The application will then attempt to figure out the simplest instruction how to describe this element.

The generated instruction will be saved in [it] variable so it can be immediatelly tested in a followup 'click [it]' command.

> Note: This is a concept version only. Currently only works on Buttons and Links and not all instructions are guaranteed to work at all circumstances.

#### Console usage
    whatisit

> Note: Your browser will be overlayed by a highlight form window. This will make your browser content unaccessible until you click the overlay.

";
        } 
        #endregion
    }
}