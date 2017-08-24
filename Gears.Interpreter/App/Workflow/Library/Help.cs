using System;
using System.Collections.Generic;
using Gears.Interpreter.Core;
using Gears.Interpreter.Core.Interpretation;

namespace Gears.Interpreter.App.Workflow.Library
{
    [NotLogged]
    [UserDescription("help \t\t-\t Displays this help.")]
    public class Help : Keyword
    {
        public override string CreateDocumentationMarkDown()
        {
            return base.CreateDocumentationMarkDown() +
                   $@"Displays console help with the list of common keywords.
#### Console usages
    help";
        }

        public override object DoRun()
        {
            var consoleCommands = "\n--- Help ---\nConsole commands: \n";
            var info = new InformativeAnswer(consoleCommands);
            
            var descriptions = new List<string>();
            foreach (var languageKeyword in Interpreter.Language.Keywords)
            {
                var userDescription = languageKeyword.GetUserDescription();
                
                if (!string.IsNullOrEmpty(userDescription))
                {
                    descriptions.Add(userDescription);
                }
            }

            descriptions.Sort(StringComparer.CurrentCultureIgnoreCase);

            foreach (var description in descriptions)
            {
                info = info.With(new InformativeDetailAnswer(description));
            }

            return info;
        }
    }
}