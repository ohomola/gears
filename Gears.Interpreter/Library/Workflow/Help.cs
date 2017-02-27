using System;
using System.Collections.Generic;
using Gears.Interpreter.Applications;

namespace Gears.Interpreter.Library.Workflow
{
    [NotLogged]
    [UserDescription("help \t\t-\t Displays this help.")]
    public class Help : Keyword
    {
        private readonly Lazy<ILanguage> _language;

        public Help(Func<IInterpreter> interpreterProvider)
        {
            _language = new Lazy<ILanguage>(()=>interpreterProvider.Invoke().Language);
        }

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
            foreach (var languageKeyword in _language.Value.Keywords)
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