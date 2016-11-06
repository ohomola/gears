using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gears.Interpreter.Core.Extensions;

namespace Gears.Interpreter.Library
{
    public class Instruction
    {
        private static string QuotedWord = "(\\s+'[^']+'\\s+)";
        private static string NotPrecedingAnyControlWord = "(?!((with)|(under)|(next to)|(above)|(below)|(left from)|(right from)|(near)|(from left)|(from right)|(from top)|(from bottom)))";
        private static string AnythingExceptQuote = "[^']";
        private static string UnquotedWord = $"(({NotPrecedingAnyControlWord}{AnythingExceptQuote})*)";
        private static string DefaultCapturingGroupForValues = $"{UnquotedWord}|{QuotedWord}";
        private static string NumberStrippingOffNthTextSuffix = "\\s?(\\d+))(\\S*\\s?";

        public string With { get; set; }

        public SearchDirection Direction { get; set; }

        public string Locale { get; set; }

        public int Order { get; set; }

        public string SubjectName { get; set; }

        public SubjectType SubjectType { get; set; }

        public List<ITagSelector> TagNames { get; set; }

        public Instruction(string what)
        {
            TagNames = new List<ITagSelector>();
            what = " " + what + " ";
            var regex = new Regex("^"+
                    Optional(CapturingGroup("Order", NumberStrippingOffNthTextSuffix))+
                    CapturingGroup("Subject") +
                    Optional(
                        CapturingGroup("Direction",
                            Or(
                            ControlWord("next to"),
                            ControlWord("under"),
                            ControlWord("above"),
                            ControlWord("below"),
                            ControlWord("left from"),
                            ControlWord("right from"),
                            ControlWord("near"),
                            ControlWord("from left"),
                            ControlWord("from right"),
                            ControlWord("from top"),
                            ControlWord("from bottom")
                            ))) +
                    Optional(CapturingGroup("Locale")) +
                    Optional(ControlWord("with") + CapturingGroup("Text") )
                    +"$");

            var result = regex.Match(what);

            if (result.Success == false)
            {
                SubjectName = what;
            }
            else
            {
                var orderString = GetCapturedValue(result, "Order")??string.Empty;
                Order = string.IsNullOrEmpty(orderString)?0: int.Parse(orderString) - 1;
                Direction = ParseDirection(GetCapturedValue(result, "Direction"));
                Locale = GetCapturedValue(result, "Locale");
                With = GetCapturedValue(result, "Text");

                var subject = GetCapturedValue(result, "Subject");
                SubjectName = CutoffTagNamesAndSubjectName(subject, TagNames);
            }
        }

        private string CutoffTagNamesAndSubjectName(string subject, List<ITagSelector> selectors)
        {
            subject = subject.ToLower();

            if (subject.Contains("button"))
            {
                selectors.Add(new TagNameSelector("button"));
                selectors.Add(new AttributeSelector("type", "button"));
                this.SubjectType = SubjectType.Button;
            }
            else if (subject.Contains("link"))
            {
                selectors.Add(new TagNameSelector("a"));
                this.SubjectType = SubjectType.Link;
            }
            else if(subject.Contains("input") || subject.Contains("textfield") || subject.Contains("textarea"))
            {
                selectors.Add(new TagNameSelector("input"));
                selectors.Add(new TagNameSelector("textArea"));
                this.SubjectType = SubjectType.Input;
            }

            return subject
                .Replace("button", "")
                .Replace("link", "")
                .Replace("input", "")
                .Replace("textarea", "")
                .Replace("textfield", "")
                .Trim();
        }

        private SearchDirection ParseDirection(string direction)
        {
            direction = direction.ToLower().Trim();
            switch (direction)
            {
                case ("left from"):
                    return SearchDirection.LeftFromAnotherElement;
                case ("right from"):
                    return SearchDirection.RightFromAnotherElement;
                case ("from right"):
                    return SearchDirection.LeftFromRightEdge;
                case ("from left"):
                    return SearchDirection.RightFromLeftEdge;
                case ("above"):
                    return SearchDirection.AboveAnotherElement;
                case ("below"):
                    return SearchDirection.BelowAnotherElement;
                case ("from top"):
                    return SearchDirection.DownFromTopEdge;
                case ("from bottom"):
                    return SearchDirection.UpFromBottomEdge;
                default:
                    return SearchDirection.RightFromAnotherElementInclusiveOrAnywhereNextTo;
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

        public string CapturingGroup(string groupName, string subRegex)
        {
            return $"(?<{groupName}>{subRegex})";
        }

        public string CapturingGroup(string groupName)
        {
            return CapturingGroup(groupName, DefaultCapturingGroupForValues);
        }

        public string ControlWord(string keyword)
        {
            return $"\\s?({keyword})\\s?";
        }

        public string GetCapturedValue(Match result, string groupname)
        {
            return result.Groups[groupname].Value.Replace("'", "").Trim();
        }


        public override string ToString()
        {
            return $"{(this.Order+1).ToOrdinalString()} {(SubjectType == default(SubjectType) ? "element" : SubjectType.ToString())} with text '{this.SubjectName}' {(Direction==default(SearchDirection)?"":"looking '"+Direction+"'")} {(string.IsNullOrEmpty(Locale)?"":"'"+Locale+"'")}";
        }
    }
}