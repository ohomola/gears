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
using System.IO;
using System.Linq;
using System.Threading;
using Gears.Interpreter.Adapters.Interoperability;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;

namespace Gears.Interpreter.Library
{
    public interface IHasTechnique
    {
        Technique Technique { get; set; }
    }

    public class Click : Keyword, IHasTechnique
    {
        private Instruction _spec;

        public Technique Technique { get; set; }

        public Click(Instruction spec)
        {
            this._spec = spec;
        }

        public Click(string what): this(new Instruction(what))
        {
            Javascript = false;
        }

        [Obsolete("Backward compatibility")]
        public bool Javascript {
            get { return Technique == Technique.Javascript; }
            set { Technique = value == true ? Technique.Javascript : Technique.MouseAndKeyboard; }
        }

        [Obsolete("Backward compatibility")]
        public Click(string what, string where) : this(what)
        {
            where = @where.ToLower().Trim();
            switch (@where)
            {
                case ("right"):
                    _spec.Direction = SearchDirection.LeftFromRightEdge;
                    break;
                case ("top"):
                case ("up"):
                    _spec.Direction = SearchDirection.DownFromTopEdge;
                    break;
                case ("down"):
                case ("bottom"):
                    _spec.Direction = SearchDirection.UpFromBottomEdge;
                    break;
                default:
                    _spec.Direction = SearchDirection.RightFromLeftEdge;
                    break;
            }
        }

        public override object Run()
        {
            var theButton = default(IBufferedElement);
            var query = new LocationHeuristictSearchStrategy(Selenium) as IElementSearchStrategy;

            var lookingForSpecificElements = _spec.TagNames.Any();
            query = query.Elements(_spec.TagNames).WithText(_spec.SubjectName, lookingForSpecificElements);

            if (!string.IsNullOrEmpty(_spec.Locale))
            {
                query = query.RelativeTo(_spec.Locale, _spec.Direction);
            }

            query = query.SortBy(_spec.Direction);

            var validResults = query.Results().ToList();

            if (_spec.Order >= validResults.Count())
            {
                throw new ApplicationException($"Cannot find element {(_spec.Order>0?(_spec.Order+1).ToString():"")}({validResults.Count()} results found)");
            }
            
            theButton = validResults.Skip(_spec.Order).First();

            switch (Technique)
            {
                case Technique.HighlightOnly:
                    Show.HighlightElements(Selenium, validResults);
                    break;
                case Technique.Javascript:
                    Selenium.WebDriver.Click(theButton.WebElement);
                    break;
                case Technique.MouseAndKeyboard:
                    var screenLocation = Selenium.PutElementOnScreen(theButton.WebElement);
                    UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), screenLocation);
                    Thread.Sleep(50);
                    UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());
                    break;
            }

            return theButton;
        }

        public override string ToString()
        {
            return $"Click {_spec}";
        }
    }

    public enum Technique
    {
        MouseAndKeyboard = 0,
        Javascript,
        HighlightOnly
    }

    //public class Click : Keyword
    //{
    //    public string What { get; set; }
    //    public string Where { get; set; }

    //    public bool Javascript { get; set; }

    //    public Click(string what)
    //    {
    //        What = what;
    //        Javascript = false;
    //    }

    //    public override object Run()
    //    {
    //        try
    //        {
    //            var elem = Selenium.WebDriver.GetElementByVisibleText(What, Where);

    //            if (elem == null)
    //            {
    //                elem = Selenium.WebDriver.GetByTagNameAndLocation(new TagQuery(What));
    //            }

    //            if (Javascript)
    //            {
    //                Selenium.WebDriver.Click(elem);
    //            }
    //            else
    //            {
    //                var screenLocation = Selenium.PutElementOnScreen(elem);
    //                UserInteropAdapter.ClickOnPoint(Selenium.GetChromeHandle(), screenLocation);
    //                Thread.Sleep(50);
    //                UserBindings.SetForegroundWindow(UserBindings.GetConsoleWindow());
    //            }
    //        }
    //        catch (Exception)
    //        {
    //            throw new ApplicationException($"Element was not found");
    //        }
    //        return null;
    //    }

    //    public override string ToString()
    //    {
    //        return $"Click {Where} '{What}'";
    //    }
    //}
}
