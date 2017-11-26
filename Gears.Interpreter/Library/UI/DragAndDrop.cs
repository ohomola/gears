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
using System.Windows.Forms;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Adapters.UI;
using Gears.Interpreter.Core.Adapters.UI.Interoperability;
using Gears.Interpreter.Core.Adapters.UI.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Core.Adapters.UI.JavaScripts;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.Library.UI
{
    [HelpDescription("dragAndDrop <inst>\t-\t clicks a button identified by instruction and drags the mouse by a specified offset")]
    public class DragAndDrop : Keyword, IHasTechnique, IInstructed
    {
        public virtual string What
        {
            set
            {
                MapSyntaxToSemantics(new Instruction(value));
            }
        }

        #region Semantics

        public virtual int Order { get; set; }
        public virtual SubjectType SearchedType { get; set; }
        public virtual List<ITagSelector> SearchedTagNames { get; set; }
        public virtual string VisibleTextOfTheButton { get; set; }
        public virtual SearchDirection Direction { get; set; }
        public virtual string NeighbourToLookFrom { get; set; }
        //TODO parse from instruction
        public virtual bool LookForOrthogonalNeighboursOnly { get; set; }
        //TODO parse from instruction
        public virtual Technique Technique { get; set; }

        private Instruction _instruction;

        public void MapSyntaxToSemantics(Instruction instruction)
        {
            Order = instruction.Order;
            SearchedTagNames = instruction.TagNames;
            SearchedType = instruction.SubjectType;
            VisibleTextOfTheButton = instruction.SubjectName;
            Direction = instruction.Direction;
            NeighbourToLookFrom = instruction.Locale;

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

        #endregion


        #region Documentation

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Drags an element identified by a visible text on the screen. The input parameter is a query instruction passed as a string to parameter 'What'. See [Web element instructions](#web-element-instructions) for more info.
Additional parameters X and Y indicate the vector of the 'drag' action.

> With Analysis on, the drag action will become slower for visual verification. Using Show will highlight the rough line of the drag vector.


#### Scenario usages
| Discriminator | What | X | Y |
| ------------- | ---- |---|---|
| DragAndDrop   | 1st button 'Add customer' below 'New Customer'| 300 | 0 |

#### Console usages
    draganddrop 300 0 1st widget button    
    show draganddrop 300 0 1st widget button    

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  
";
        }

        #endregion

        public DragAndDrop()
        {
        }

        public override IKeyword FromString(string textInstruction)
        {
            var parts = textInstruction.Split(' ');

            var xOffset = int.Parse(parts[1]);
            var yOffset = int.Parse(parts[2]);

            textInstruction = parts[3];
            return new DragAndDrop() {What= textInstruction, X=xOffset,Y=yOffset};
        }

        public int X { get; set; }

        public int Y { get; set; }

        public DragAndDrop(string what)
        {
            MapSyntaxToSemantics(new Instruction(what));
        }

        public override object DoRun()
        {
            var query = new LocationHeuristictSearchStrategy(Selenium);

            var result = query.DirectLookup(SearchedTagNames, VisibleTextOfTheButton, NeighbourToLookFrom, Direction, Order, LookForOrthogonalNeighboursOnly, exactMatchOnly:true);

            if (Interpreter?.IsAnalysis == true)
            {
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, _instruction?.ToAnalysisString());
                Console.Out.WriteColoredLine(ConsoleColor.Magenta, $"Main Result: \n\t{result.MainResult}\nAll results:\n\t{string.Join("\n\t", result.AllValidResults)}");
            }

            if (result.Success == false)
            {
                throw new LookupFailureException(result, $"Failed {ToString()}.\nCannot find element {(Order > 0 ? (Order + 1).ToString() : "")}({result.AllValidResults.Count()} results found)");
            }

            switch (Technique)
            {
                case Technique.Show:
                    Highlighter.HighlightElements(
                        ()=>Console.ReadLine(), 
                        Selenium, 
                        result.AllValidResults, 
                        Color.FromArgb(255, 0, 150, 255), 
                        Color.FromArgb(255, 255, 0, 255),
                        Order,
                        Color.FromArgb(255, 0, 255, 150),
                        X, 
                        Y);
                    return new InformativeAnswer("Highlighting complete.");
                case Technique.Javascript:
                    Selenium.WebDriver.Click(result.MainResult.WebElement);
                    break;
                case Technique.MouseAndKeyboard:
                    Selenium.BringToFront();
                    var screenLocation = Selenium.PutElementOnScreen(result.MainResult.WebElement);
                    if (Interpreter?.IsAnalysis == true)
                    {
                        //Highlighter.HighlightPoints(750, Selenium, screenLocation);
                        Highlighter.HighlightElements(750, Selenium, new [] {result.MainResult}, Color.Aqua, Color.Red,-1,Color.Aqua);
                    }
                    UserInteropAdapter.PressOnPoint(Selenium.BrowserHandle, screenLocation);
                    
                    
                    if (Interpreter.IsAnalysis)
                    {
                        var oldPosition = Cursor.Position;

                        var tempPoint = new Point(screenLocation.X, screenLocation.Y);
                        UserBindings.ClientToScreen(Selenium.BrowserHandle, ref tempPoint);
                        Cursor.Position = tempPoint;
                        var steps = 50m;
                        var xOffset = X/ steps;
                        var yOffset = Y / steps;
                        
                        for (int i = 0; i < steps; i++)
                        {
                            Cursor.Position = new Point((int) (tempPoint.X+i*xOffset), (int)(tempPoint.Y + i * yOffset));
                            Thread.Sleep(30);
                        }

                        Cursor.Position = oldPosition;
                    }

                    screenLocation.X += X;
                    screenLocation.Y += Y;
                    Thread.Sleep(20);

                    UserInteropAdapter.ReleaseOnPoint(Selenium.BrowserHandle, screenLocation);
                    Thread.Sleep(50);
                    break;
            }

            return new SuccessAnswer($"Performed {ToString()}");
        }
        
        public override string ToString()
        {
            return $"Click {(Order+1).ToOrdinalString()} {(SearchedType == default(SubjectType) ? "" : SearchedType.ToString())} {VisibleTextOfTheButton} {(Direction==default(SearchDirection)?"":Instruction.GetDescription(Direction))} {NeighbourToLookFrom}";
        }
    }
}