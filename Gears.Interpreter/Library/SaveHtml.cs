using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gears.Interpreter.Library
{
    public class SaveHtml:Keyword
    {
        public override object Run()
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
