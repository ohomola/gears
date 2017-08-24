using System;
using System.Drawing;
using Gears.Interpreter.Core;

namespace Gears.Interpreter.Library.UI
{
    public class Resize : Keyword
    {
        public string What { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }

        public override string CreateDocumentationMarkDown()
        {
            return $@"
{base.CreateDocumentationMarkDown()}
Forces window to take a certain size. The purpose is to stabilise tests in responsive-design applications.

#### Scenario usages
| Discriminator | What    | Width | Height |
| ------------- | -----   | ----- | -----  |
| Resize        | browser | 640 | 480    |

#### Console usage
    resize browser 640 480

### Additional properties
* [Common Keyword properties](Documentation#common-keyword-properties)  

";
        }

        public Resize()
        {
        }

        public Resize(string what, int width, int height)
        {
            What = what;
            Height = height;
            Width = width;
        }

        public override IKeyword FromString(string textInstruction)
        {
            var parts = textInstruction.Split(' ');

            return new Resize(parts[1], int.Parse(parts[2]), int.Parse(parts[3]));
        }

        public override object DoRun()
        {
            if (What.ToLower() != "browser")
            {
                throw new NotImplementedException("Resizing other windows than 'browser' is not supported. Please use 'Browser' as a what parameter of Resize.");
            }

            Selenium.WebDriver.Manage().Window.Size = new Size(Width, Height);

            return true;
        }
    }
}