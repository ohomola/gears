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
using System.Net;
using System.Xml.Serialization;
using Gears.Interpreter.Adapters.Interoperability.ExternalMethodBindings;
using Gears.Interpreter.Applications;
using Gears.Interpreter.Applications.Debugging.Overlay;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [UserDescription("gotourl <url>\t-\t navigates web browser to specified URL.")]
    public class GoToUrl : Keyword
    {
        public const string SuccessMessage = "Page loaded.";
        public virtual string Url { get; set;  }


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

        [Wire]
        [XmlIgnore]
        public IOverlay Overlay { get; set; }

        public GoToUrl(string url)
        {
            Url = url;
        }

        public override IKeyword FromString(string textInstruction)
        {
            var param = ExtractSingleParameterFromTextInstruction(textInstruction);

            return new GoToUrl(param);
        }

        public GoToUrl()
        {
        }

        public static string CombineUrl(string baseUrl, string relativeUrl)
        {
            var baseUri = new Uri(baseUrl);

            var combinedUri = new Uri(baseUri, relativeUrl);

            return combinedUri.AbsoluteUri;
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
    }
}