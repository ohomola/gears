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
using Gears.Interpreter.Library.Workflow;

namespace Gears.Interpreter.Library
{
    [NotLogged]
    [UserDescription("savehtml \t-\t save current page source to a new HTML file")]
    public class SaveHtml:Keyword
    {
        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Saves current webpage in browser to a file. This file might be different to what your browser would save. Use this to create HTML snapshots as attachments for bugs.

#### Scenario usage
| Discriminator | 
| ------------- | 
| SaveHtml   | 

#### Console usage
    savehtml

> Note: File will be created to the same Output folder as your scenario reports an will have a unique generated name.
";
        }

        public override object DoRun()
        {
            var outputFile = string.Format(Properties.Settings.Default.ScenarioOutputPath, DateTime.Now.ToString("s").Replace(":", "_"))+".html";
            using (var fw = new StreamWriter(outputFile))
            {
                fw.Write(Selenium.WebDriver.PageSource);
            }

            return null;
        }
    }
}
