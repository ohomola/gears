using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gears.Interpreter.Library
{
    public enum SearchDirection
    {
        RightFromAnotherElementInclusive =0,
        RightFromAnotherElement,
        LeftFromAnotherElement,
        Up,
        LeftFromRightEdge,
        RightFromLeftEdge,
        Down,
        DownFromTopEdge,
        UpFromBottomEdge
    }

    public class Instruction
    {
        //private const string DefaultCapturingGroupForValues = "(\\s+[\\S]+\\s+)|(\\s+'[^']+'\\s+)";
        
        private static string QuotedWord = "(\\s+'[^']+'\\s+)";
        private static string NotPrecedingAnyControlWord = "(?!under|next to|above|below|left from|right from|near|with|from left|from right|from top)";
        private static string AnythingExceptQuote = "[^']";
        private static string UnquotedWord = $"((?:{AnythingExceptQuote}{NotPrecedingAnyControlWord})*)";
        private static string DefaultCapturingGroupForValues = $"{UnquotedWord}|{QuotedWord}";
        private static string NumberStrippingOffNthTextSuffix = "\\s?(\\d+))(\\S*\\s?";

        public string With { get; set; }

        public SearchDirection Direction { get; set; }

        public string Locale { get; set; }

        public int Order { get; set; }

        public string SubjectName { get; set; }
        public SubjectType SubjectType { get; set; }
        public List<string> TagNames { get; set; }

        public Instruction(string what)
        {
            TagNames = new List<string>();
            what = " " + what + " ";
            var regex = new Regex("^"+
                    Optional(CapturingGroup("Order", NumberStrippingOffNthTextSuffix))+
                    CapturingGroup("Tag") +
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
                            ControlWord("from top")
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

                var subject = GetCapturedValue(result, "Tag");
                SubjectName = CutoffTagNamesAndSubjectName(subject, TagNames);
            }
        }

        private string CutoffTagNamesAndSubjectName(string subject, List<string> tagNames)
        {
            subject = subject.ToLower();
            if (subject.Contains("button"))
            {
                tagNames.Add("button");
                this.SubjectType = SubjectType.Button;
            }
            else if (subject.Contains("link"))
            {
                tagNames.Add("a");
                this.SubjectType = SubjectType.Link;
            }
            else if(subject.Contains("input"))
            {
                tagNames.Add("input");
                tagNames.Add("textArea");
                this.SubjectType = SubjectType.Input;
            }

            return subject.Replace("button", "").Replace("link", "").Replace("input", "").Trim();
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

        // TODO fully implement
        private SearchDirection ParseDirection(string direction)
        {
            if (direction.ToLower().Contains("left from"))
            {
                return SearchDirection.LeftFromAnotherElement;
            }

            if (direction.ToLower().Contains("right from"))
            {
                return SearchDirection.RightFromAnotherElement;
            }

            if (direction.ToLower().Contains("from right"))
            {
                return SearchDirection.LeftFromRightEdge;
            }

            if (direction.ToLower().Contains("from left"))
            {
                return SearchDirection.RightFromLeftEdge;
            }

            if (direction.ToLower().Contains("above"))
            {
                return SearchDirection.Up;
            }
            
            if (direction.ToLower().Contains("below"))
            {
                return SearchDirection.Down;
            }

            if (direction.ToLower().Contains("from top"))
            {
                return SearchDirection.DownFromTopEdge;
            }

            return SearchDirection.RightFromAnotherElementInclusive;
        }

    }

    public enum SubjectType
    {
        Any = 0,
        Button,
        Input,
        Link
    }
}