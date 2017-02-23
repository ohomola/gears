using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Library.Documentations
{
    public class Documentation
    {
        private readonly List<IKeyword> _keywords;

        public Documentation(IEnumerable<IKeyword> keywords)
        {
            _keywords = keywords.ToList();
        }

        public string CreateContentMarkDown()
        {
            _keywords.OrderBy(x=>x.GetType().Name).ToList();

            var sb = new StringBuilder();

            foreach (var keyword in _keywords)
            {
                sb.AppendLine(keyword.CreateDocumentationMarkDown());
            }

            return sb.ToString();
        }
    }
}
