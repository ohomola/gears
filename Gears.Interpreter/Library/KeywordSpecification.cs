using System.Text.RegularExpressions;

namespace Gears.Interpreter.Library
{
    public enum SearchDirection
    {
        Right =0,
        Left,
    }

    public class KeywordSpecification: IRegularExpressionFluentBuilder
    {
        public string Text { get; set; }

        public SearchDirection Direction { get; set; }

        public string LabelText { get; set; }
        
        public KeywordSpecification(string what)
        {
            var regex = new Regex(
                    Optional(CapturingGroup("Tag")) +
                    Optional(
                        CapturingGroup("Direction",
                        Or(
                            ControlWord("next to"),
                            ControlWord("under"),
                            ControlWord("above"),
                            ControlWord("left from"),
                            ControlWord("right from"),
                            ControlWord("near")
                            ))) +
                    CapturingGroup("LabelText") +
                    ControlWord("with") +
                    CapturingGroup("Text"));

            var result = regex.Match(what);

            if (result.Success == false)
            {
                LabelText = what;
            }
            else
            { 
                var tagName = GetCapturedValue(result, "Tag");
                Direction = ParseDirection(GetCapturedValue(result, "Direction"));
                LabelText = GetCapturedValue(result, "LabelText");
                Text = GetCapturedValue(result, "Text");
            }
        }

        public string Optional(string s)
        {
            return $"({s})?";
        }

        public string Or(params string[] strings)
        {
            return string.Join("|", strings);
        }

        public string CapturingGroup(string groupName, string subRegex = "([\\S]+)|('[^']+')")
        {
            return $"(?<{groupName}>{subRegex})";
        }

        public string ControlWord(string keyword)
        {
            return $"\\s?({keyword})\\s";
        }

        public string GetCapturedValue(Match result, string groupname)
        {
            return result.Groups[groupname].Value.Replace("'", "").Trim();
        }

        // TODO fully implement
        private SearchDirection ParseDirection(string direction)
        {
            if (direction.ToLower().Contains("left"))
            {
                return SearchDirection.Left;
            }

            return SearchDirection.Right;
        }

    }
}