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
using System.IO;
using System.Linq;
using Gears.Interpreter.Adapters;
using Gears.Interpreter.Data.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Gears.Interpreter.Library
{
    public class Fill : Keyword
    {
        public string What { get; set; }
        public string Where { get; set; }
        public string Text { get; set; }


        public Fill(string what, string text)
        {
            What = what;
            Text = text;
        }

        public override object Run()
        {
            //try
            //{
            //    var directScript = $"return ([firstByLocation(\"{Where}\", getExactMatches(\"{What}\"))]);";
            //    var r = Selenium.WebDriver.RunLibraryScript(directScript);
            //    var directElement = (r as IEnumerable<IWebElement>);
            //    if (directElement != null && (directElement.First().TagName.ToLower() =="textarea" || directElement.First().TagName.ToLower() == "input"))
            //    {
            //        directElement.First().SendKeys(Text);
            //        return null;
            //    }

            //    var formattableString = string.Empty;
            //    if (Where == null)
            //    {
            //        formattableString = $"return tagMatches(clickFirstMatch(firstByDistance(getExactMatches(\"{What}\")[0],getOrthogonalInputs(getExactMatches(\"{What}\")))));";
            //    }
            //    else
            //    {
            //        formattableString = $"return tagMatches([firstByLocation(\"{Where}\",getOrthogonalInputs(getExactMatches(\"{What}\")))]);";
            //    }
            //    var matches = Selenium.WebDriver.RunLibraryScript(formattableString);

            //var elements = (matches as IEnumerable<IWebElement>);
            //elements.First(x => x.Displayed && x.Enabled).SendKeys(Text);
            //}
            //catch (Exception)
            //{
            //    throw new ApplicationException($"Element {What} was not found");
            //}

            //return null;
            var results = Selenium.WebDriver.RunLibraryScript($"return findInput(\"{What}\",\"{Where}\")");
            var element = (results as IWebElement);
            element.SendKeys(Text);


            return element;
        }

        


        public override string ToString()
        {
            return $"Fill {Where} '{What}'  with '{Text}'";
        }
    }
}
