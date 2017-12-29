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
using System.Drawing.Imaging;
using System.IO;
using Gears.Interpreter.Core.Interpretation;
using OpenQA.Selenium;

namespace Gears.Interpreter.Core.Library
{
    [NotLogged]
    [HelpDescription("savehtml \t-\t save current page source to a new HTML file")]
    public class SaveScreenshot:Keyword
    {
        public string What { get; set; }

        public SaveScreenshot()
        {
        }

        public SaveScreenshot(string what)
        {
            What = what;
        }

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Saves current browser screen to an image file. 
What parameter indicates a filename prefix to the created file. Suffix of the file name will be generated.


#### Scenario usage
| Discriminator  | What |
| -------------  | ---- |
| SaveScreenshot | Screen1 |
| SaveScreenshot |  |

#### Console usage
    SaveScreenshot
    SaveScreenshot test1

> Note: File will be created to the same Output folder as your scenario reports and will have a unique generated name. You can configure the default path in Gears.Interpreter.exe.config


";
        }

        public override object DoRun()
        {
            var outputFile = string.Format(Properties.Settings.Default.ScreenshotOutputPath, What + DateTime.Now.ToString("s").Replace(":", "_"));

            var ss = ((ITakesScreenshot) Selenium.WebDriver).GetScreenshot();
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
            ss.SaveAsFile(outputFile, ScreenshotImageFormat.Jpeg);

            return new SuccessAnswer("Saved file "+outputFile);
        }

        public override void FromString(string textInstruction)
        {
            What = textInstruction;
        }
    }
}
