using System.Linq;
using System.Text.RegularExpressions;
using Gears.Interpreter.Library;

namespace Gears.Interpreter.Data.Serialization.Mapping
{
    public interface IRememberedDataResolver
    {
        bool CanResolve(object obj);
        string Resolve(string s);
    }

    public class RememberedDataResolver : IRememberedDataResolver
    {
        private IDataContext _dataContext;

        public RememberedDataResolver(IDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public bool CanResolve(object obj)
        {
            var s = (obj as string);

            if (s == null)
            {
                return false;
            }

            return (s.Contains("[") && s.Contains("]"));
        }

        public string Resolve(string s)
        {
            var match= new Regex(".*(?<variable>\\[.*\\]).*").Match(s);

            if (!match.Success)
            {
                return s;
            }

            var variableSubstring = match.Groups["variable"].Value;

            var variable = variableSubstring.Substring(1, variableSubstring.Length - 2);

            var rememberedTexts = _dataContext.GetAll<RememberedText>().ToList();
            var data =rememberedTexts.FirstOrDefault(x => x.Variable.ToLower() == variable.ToLower());

            if (data != null)
            {
                return Resolve(s.Replace(variableSubstring, data.What));
            }

            return s;


     ;   }
    }
}