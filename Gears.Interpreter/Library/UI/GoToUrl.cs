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
using System.Xml.Serialization;
using Gears.Interpreter.App.UI.Overlay;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Extensions;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.Library.UI
{
    [HelpDescription("gotourl <url>\t-\t navigates web browser to specified URL.")]
    public class GoToUrl : Keyword
    {
        public const string SuccessMessage = "Page loaded.";
        public virtual string Url { get; set;  }

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

        public GoToUrl()
        {
        }

        public GoToUrl(string url)
        {
            Url = url;
        }

        public override IKeyword FromString(string textInstruction)
        {
            var param = ExtractSingleParameterFromTextInstruction(textInstruction);

            return new GoToUrl(param);
        }

        public override object DoRun()
        {
            Selenium.WebDriver.Navigate().GoToUrl(Url);

            return new SuccessAnswer(SuccessMessage);
        }

        public override string ToString()
        {
            return "Go to '" + Url + "'";
        }

        #region Documentation
        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Navigates browser to specified URL. Note that the url must be fully formated, (e.g. beginning with http://).
#### Scenario usage
| Discriminator | URL  | 
| ------------- | ----- | 
| GoToUrl       | https://github.com/ohomola/gears/wiki/Documentation |     

#### Console usage
    gotourl https://github.com/ohomola/gears/wiki/Documentation";
        }
        #endregion
    }
}