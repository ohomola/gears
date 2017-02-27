using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library.Documentations
{
    public class Documentation
    {

        public const string ConsoleKeywordNote =
            "\n> Console control Keyword - use directly in console to control application (not recommended as part of scenario).\n\n";
        private readonly List<IKeyword> _keywords;

        public Documentation(IEnumerable<IKeyword> keywords)
        {
            _keywords = keywords.ToList();
        }

        public string CreateContentMarkDown()
        {
            var orderedKeywords = _keywords.OrderBy(x=>x.GetType().Name).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("# List of keywords");
            foreach (var keyword in orderedKeywords)
            {
                sb.AppendLine(keyword.CreateDocumentationMarkDown());
            }

            return sb.ToString();
        }

        public string CreateSideMenuMarkDown()
        {
            var orderedKeywords = _keywords.OrderBy(x => x.GetType().Name).ToList();

            var sb = new StringBuilder();

            sb.AppendLine("- [List of Keywords](Documentation#list-of-keywords)  ");

            foreach (var keyword in orderedKeywords)
            {
                sb.AppendLine($"  - [{keyword.GetType().Name}](Documentation#{keyword.GetType().Name.ToLower()})");
            }

            sb.AppendLine();
            return sb.ToString();
        }
    }
}
