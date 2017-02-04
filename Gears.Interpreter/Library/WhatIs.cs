using System;
using System.Collections.Generic;
using System.Linq;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using OpenQA.Selenium;

namespace Gears.Interpreter.Library
{
    public class WhatIs : Keyword
    {
        private int _x;
        private int _y;

        private WhatIs(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public WhatIs()
        {
        }

        public override IKeyword FromString(string textInstruction)
        {
            var strings = textInstruction.Split(' ');

            if (strings.Length < 3)
            {
                return new WhatIs();
            }

            var args = ExtractTwoParametersFromTextInstruction(textInstruction);
            return new WhatIs(int.Parse(args[0]), int.Parse(args[1]));
        }

        public override object DoRun()
        {
            var outerHeight =(int)Math.Abs((long)Selenium.WebDriver.RunLibraryScript("return  window.outerHeight"));
            var outerWidth = (int)Math.Abs((long)Selenium.WebDriver.RunLibraryScript("return window.outerWidth"));
            var innerHeight = (int) Math.Abs((long)Selenium.WebDriver.RunLibraryScript("return window.innerHeight"));
            var innerWidth = (int)Math.Abs((long)Selenium.WebDriver.RunLibraryScript("return window.innerWidth"));

            var clientRect = new UserBindings.RECT();
            UserBindings.GetClientRect(Selenium.GetChromeHandle(), ref clientRect);

            var windowRect = new UserBindings.RECT();
            UserBindings.GetWindowRect(Selenium.GetChromeHandle(), ref windowRect);


            if (_x == default(int))
            {
                using (var hud = Hud.CreateClickable(Selenium))
                {
                    var point = hud.ReadClick();

                    Selenium.ConvertFromGraphicsToScreen(ref point);
                    Selenium.ConvertFromScreenToWindow(ref point);
                    Selenium.ConvertFromWindowToPage(ref point);

                    _x = point.X;
                    _y = point.Y;

                    
                }
            }

            var elements = Selenium.WebDriver.GetElementByCoordinates(_x, _y);
            
            var element = elements.LastOrDefault(x=>x.IsClickable());
            
            if (element != null)
            {

                var instruction = new Instruction();
                if (element.Text != null)
                {
                    instruction.SubjectName = element.Text;
                }

                if (element.TagName.ToLower() == "a")
                {
                    instruction.TagNames.Add(new TagNameSelector("a"));
                    instruction.SubjectType =SubjectType.Link;
                }
                else
                {
                    instruction.TagNames.Add(new TagNameSelector("button"));
                    instruction.TagNames.Add(new AttributeSelector("type", "button"));
                    instruction.SubjectType = SubjectType.Button;
                }

                var query = new LocationHeuristictSearchStrategy(Selenium);

                var result = query.DirectLookup(
                    instruction.TagNames, 
                    instruction.SubjectName, 
                    null, 
                    SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo, 0, false);

                if (result.Success)
                {
                    if (result.Result.WebElement.Equals(element))
                    {
                        new Remember("it", instruction.ToString()).Execute();
                        return new SuccessAnswer(instruction.ToString());
                    }
                }
            }

            return new WarningAnswer("No element on that location.");
        }

        
    }
}