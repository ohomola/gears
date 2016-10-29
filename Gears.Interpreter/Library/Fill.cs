#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears, a software automation and assistance framework.

Gears is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Gears is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;
using Gears.Interpreter.Tests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Gears.Interpreter.Library
{
    public class Fill : Keyword, IHasTechnique
    {
        private IElementSearchStrategy _searchStrategy;

        public string Text { get; set; }

        public string LabelText { get; set; }

        public SearchDirection Direction { get; set; }

        public Technique Technique { get; set; }

        public int Order { get; set; }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

        [Obsolete("Backward compatibility")]
        public bool Javascript
        {
            get { return Technique == Technique.Javascript; }
            set { Technique = value == true ? Technique.Javascript : Technique.MouseAndKeyboard; }
        }

        public Fill(string what)
        {
            var _spec = new Instruction(what);

            if (string.IsNullOrEmpty(_spec.Locale))
            {
                LabelText = _spec.SubjectName;
            }
            else
            {
                LabelText = _spec.Locale;
            }

            Direction = _spec.Direction;
            Text = _spec.With;
            Order = _spec.Order;
        }

        public Fill(string what, string text) : this(what)
        {
            Text = text;
        }
        
        [Obsolete("Backward compatibility")]
        public Fill(string what, string where, string text) : this(what, text)
        {
            where = @where.ToLower().Trim();
            switch (@where)
            {
                case ("right"):
                    Direction = SearchDirection.LeftFromRightEdge;
                    break;
                case ("top"):
                case ("up"):
                    Direction = SearchDirection.DownFromTopEdge;
                    break;
                case ("down"):
                case ("bottom"):
                    Direction = SearchDirection.UpFromBottomEdge;
                    break;
                default:
                    Direction = SearchDirection.RightFromLeftEdge;
                    break;
            }
        }



        public override object Run()
        {
            IBufferedElement theInput;
            IEnumerable<IBufferedElement> validResults;

            _searchStrategy = new LocationHeuristictSearchStrategy(this.Selenium);

            var allInputs = _searchStrategy.Elements(new[] { "input", "textArea" });

            var allInputsWithText = allInputs.WithText(LabelText, matchWhenTextIsInChild:false);

            if (allInputsWithText.Any())
            {
                validResults = allInputsWithText
                    .SortBy(Direction)
                    .Results()
                    .Skip(Order);
            }
            else
            {
                validResults = allInputs
                    .RelativeTo(LabelText, Direction)
                    .SortBy(Direction)
                    .Results()
                    .Skip(Order);
            }

            theInput = validResults.FirstOrDefault();

            if (theInput == null)
            {
                throw new ApplicationException("Input not found");
            }

            switch (Technique)
            {
                case Technique.HighlightOnly:
                    Show.HighlightElements(Selenium, validResults);
                    break;
                case Technique.Javascript:
                    theInput.WebElement.SendKeys(Text);
                    break;
                case Technique.MouseAndKeyboard:
                    var handle = Selenium.GetChromeHandle();

                    var screenLocation = Selenium.PutElementOnScreen(theInput.WebElement);

                    UserInteropAdapter.ClickOnPoint(handle, screenLocation);
                    Thread.Sleep(50);
                    UserInteropAdapter.SendText(handle, Text, screenLocation);
                    Thread.Sleep(50);
                    UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());
                    break;
            }

            return theInput;
        }

        

        //public override object Run()
            //{
            //    if(Javascript)
            //    {
            //        try
            //        {
            //            var element = Selenium.WebDriver.FindElementNextToAnotherElement(LabelText, Where);
            //            element.SendKeys(Text);
            //            return element;
            //        }
            //        catch (Exception)
            //        {
            //            throw new ApplicationException($"Element {LabelText} was not found");
            //        }
            //    }
            //    else
            //    {
            //        var element = Selenium.WebDriver.FindElementNextToAnotherElement( LabelText, Where);

            //        var handle = Selenium.GetChromeHandle();

            //        var screenLocation = Selenium.PutElementOnScreen(element);

            //        UserInteropAdapter.ClickOnPoint(handle, screenLocation);
            //        Thread.Sleep(50);
            //        UserInteropAdapter.SendText(handle, Text, screenLocation);
            //        Thread.Sleep(50);
            //        UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());

            //        return element;
            //    }
            //}

        public override string ToString()
        {
            return $"Fill '{LabelText}' with '{Text}'";
        }
    }
}
