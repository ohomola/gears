﻿#region LICENSE
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
using System.Drawing;
using System.Linq;
using System.Threading;
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.JavaScripts;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.Library.UI
{
    [HelpDescription("click <inst>\t-\t clicks a button identified by instruction")]
    public class Click : Keyword, IHasTechnique, IInstructed
    {
        [Wire]
        public IBrowserOverlay BrowserOverlay { get; set; }

        public override string Instruction
        {
            set => What = value;
        }

        public virtual string What
        {
            set
            {
                MapRichSyntaxToSemantics(new WebElementInstruction(value));
            }
        }

        public virtual int Order { get; set; }
        public virtual WebElementType SearchedType { get; set; }
        public virtual List<ITagSelector> SearchedTagNames { get; set; } = new List<ITagSelector>();
        public virtual string VisibleTextOfTheButton { get; set; }
        public virtual SearchDirection Direction { get; set; }
        public virtual string NeighbourToLookFrom { get; set; }
        public virtual bool LookForOrthogonalNeighboursOnly { get; set; }
        public virtual Technique Technique { get; set; }
        public bool ExactMatch { get; set; }
        private WebElementInstruction _instruction;

        public void MapRichSyntaxToSemantics(WebElementInstruction instruction)
        {
            Order = instruction.Order ?? Order;
            SearchedTagNames = instruction.TagNames ?? SearchedTagNames;
            SearchedType = instruction.SubjectType ?? SearchedType;
            VisibleTextOfTheButton = instruction.SubjectName ?? VisibleTextOfTheButton;
            Direction = instruction.Direction ?? Direction;
            NeighbourToLookFrom = instruction.Locale ?? NeighbourToLookFrom;
            ExactMatch = instruction.Accuracy != CompareAccuracy.Partial;

            if (new[]
            {
                SearchDirection.AboveAnotherElement,
                SearchDirection.BelowAnotherElement,
                SearchDirection.RightFromAnotherElement,
                SearchDirection.LeftFromAnotherElement,

            }.Contains(Direction))
            {
                LookForOrthogonalNeighboursOnly = true;
            }

            _instruction = instruction;
        }

        public Click()
        {
        }

        public Click(string what)
        {
            MapRichSyntaxToSemantics(new WebElementInstruction(what));
        }

        public override object DoRun()
        {
            var query = new LocationHeuristictSearchStrategy(Selenium);

            var result = query.DirectLookup(SearchedTagNames, VisibleTextOfTheButton, NeighbourToLookFrom, Direction, Order, LookForOrthogonalNeighboursOnly, exactMatchOnly:true);

            if (Interpreter?.IsAnalysis == true)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, "DirectLookup (exact matches): " + _instruction?.ToAnalysisString());
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{result.MainResult}\nAll results:\n\t{string.Join("\n\t", result.AllValidResults)}");
            }

            if (ExactMatch == false && result.Success == false)
            {
                result = query.DirectLookup(SearchedTagNames, VisibleTextOfTheButton, NeighbourToLookFrom, Direction, Order, LookForOrthogonalNeighboursOnly, exactMatchOnly: false);

                if (Interpreter?.IsAnalysis == true)
                {
                    Console.Out.WriteColoredLine(ConsoleColor.Magenta, "DirectLookup (all matches): " + _instruction?.ToAnalysisString());
                    Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{result.MainResult}\nAll results:\n\t{string.Join("\n\t", result.AllValidResults)}");
                }
            }

            if (result.Success == false)
            {
                throw new LookupFailureException(result, $"Failed {ToString()}.\nCannot find element {(Order > 0 ? (Order + 1).ToString() : "")}({result.AllValidResults.Count()} results found)");
            }

            switch (Technique)
            {
                case Technique.Show:

                    BrowserOverlay
                        .HighlightElements((Order + 1).ToString(), Color.GreenYellow, result.MainResult)
                        .HighlightElements((Order + 1).ToString(), Color.CadetBlue, result.AllValidResults.Except(new[] { result.AllValidResults.ElementAt(Order) }))
                        .ShowUntilNextKeyword("Highlighted element will be Clicked");
                    return new OverlayAnswer(BrowserOverlay.Artifacts, "Highlighting complete.");

                case Technique.Javascript:
                    Selenium.WebDriver.Click(result.MainResult.WebElement);
                    break;
                case Technique.MouseAndKeyboard:
                    Selenium.BringToFront();
                    var screenLocation = Selenium.PutElementOnScreen(result.MainResult.WebElement);

                    if (Interpreter?.IsAnalysis == true)
                    {
                        BrowserOverlay
                            .HighlightElements((Order + 1).ToString(), Color.GreenYellow, result.MainResult)
                            .ShowFor(750,"Highlighted element will be Clicked");

                    }
                    UserInteropAdapter.ClickOnPoint(Selenium.BrowserHandle, screenLocation);
                    Thread.Sleep(50);
                    break;
            }

            return new SuccessAnswer($"Performed {ToString()}");
        }
        
        public override string ToString()
        {
            return $"Click {(Order+1).ToOrdinalString()} {(SearchedType == default(WebElementType) ? "" : SearchedType.ToString())} {VisibleTextOfTheButton} {(Direction==default(SearchDirection)?"":WebElementInstruction.GetDescription(Direction))} {NeighbourToLookFrom}";
        }

        #region Documentation

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Clicks an element identified by a visible text on the screen. The input parameter is a query instruction passed as a string to parameter 'What'. See [Web element instructions](#web-element-instructions) for more info.

#### Scenario usages
| Discriminator | What |
| ------------- | ---- |
| Click         | Save |
| Click         | 1st button 'save customer' below 'New Customer'|
| Click         | 1st button like 'save' below 'New Customer'|
| Click         | 4th button from right|

#### Console usages
    click save
    click 4th button from right
    show click save

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        #endregion
    }
}
