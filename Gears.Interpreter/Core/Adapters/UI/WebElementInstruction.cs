using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Gears.Interpreter.Core.Adapters.UI.Lookup;
using Gears.Interpreter.Core.Extensions;
using JetBrains.Annotations;

namespace Gears.Interpreter.Core.Adapters.UI
{
    public class WebElementInstruction
    {
        private const string DEFAULT_VALUE_WHEN_WORD_IS_NOT_FOUND = null;

        private static string QuotedWord = "(\\s?'[^']+'\\s?)";
        private static string NotPrecedingAnyControlWord =  "(?!((inside )|(with )|(under)|(next to)|(above)|(below)|(left from)|(right from)|(near)|(from left)|(from right)|(from top)|(from bottom)))";
        //private static string FollowingAnyControlWord =    "(?<=((in)|(with)|(under)|(next to)|(above)|(below)|(left from)|(right from)|(near)|(from left)|(from right)|(from top)|(from bottom)))";
        private static string AnythingExceptQuote = "[^']";
        private static string UnquotedWord = $"(({NotPrecedingAnyControlWord}{AnythingExceptQuote})*)";
        private static string DefaultCapturingGroupForValues = $"{QuotedWord}|{UnquotedWord}";// NOTE: order defines preference (reverting causes incorrect match of empty string as unquoted word, instead for capturing quoted word)
        private static string NumberStrippingOffNthTextSuffix = "\\s?(\\d+[a-zA-Z\\.]+)\\s?";
        private static string SpaceOrEnd = "(?:\\s|$)";

        
        //private static string NumberStrippingOffNthTextSuffix = "\\s?(\\d+))(\\S*\\s?";

        [CanBeNull]
        public string With { get; set; }

        [CanBeNull]
        public SearchDirection? Direction { get; set; }

        [CanBeNull]
        public string Locale { get; set; }

        [CanBeNull]
        public int? Order { get; set; }

        [CanBeNull]
        public string SubjectName { get; set; }

        [CanBeNull]
        public WebElementType? SubjectType { get; set; }

        [CanBeNull]
        public List<ITagSelector> TagNames { get; set; }

        [CanBeNull]
        public CompareAccuracy? Accuracy { get; set; }

        public string Area { get; set; }

        public WebElementInstruction(string what) : this()
        {
            what = " " + what + " ";
            var regex = new Regex("^"+
                    Optional(CapturingGroup("Order", NumberStrippingOffNthTextSuffix))+
                    Optional(CapturingGroup("SubjectTagName", $"\\s?{NotPrecedingAnyControlWord}(button){SpaceOrEnd}|(link){SpaceOrEnd}|(input){SpaceOrEnd}|(textfield){SpaceOrEnd}|(textarea){SpaceOrEnd}")) +
                    Optional(CapturingGroup("Accuracy", $"\\s?{NotPrecedingAnyControlWord}(like)\\s?")) +
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
                    Optional(ControlWord("inside") + CapturingGroup("Area")) +
                    Optional(ControlWord("with") + CapturingGroup("Text") )
                    +"$", RegexOptions.IgnoreCase);

            var result = regex.Match(what);

            if (result.Success == false)
            {
                SubjectName = what;
            }
            else
            {
                var orderString = GetCapturedValue(result, "Order")??string.Empty;
                orderString = ParseNumber(orderString);
                Order = string.IsNullOrEmpty(orderString)?(int?)null: int.Parse(orderString) - 1;
                Direction = ParseDirection(GetCapturedValue(result, "Direction"));
                Locale = GetCapturedValue(result, "Locale");
                With = GetCapturedValue(result, "Text");
                Accuracy = GetCapturedValue(result, "Accuracy")?.ToLower() == "like"?CompareAccuracy.Partial : CompareAccuracy.Exact;
                Area = GetCapturedValue(result, "Area");
                SubjectName = GetCapturedValue(result, "Subject");
                this.SubjectType= MapToSubjectTypeAndAddTagnamesRange(GetCapturedValue(result, "SubjectTagName"), TagNames);
                if (!TagNames.Any())
                {
                    TagNames = null;
                }
            }
        }

        private string ParseNumber(string orderString)
        {
            var regex = new Regex("^(\\d+).*$");

            var result = regex.Match(orderString);

            return result.Groups[1].Value;
        }

        public WebElementInstruction()
        {
            TagNames = new List<ITagSelector>();
        }

        private WebElementType? MapToSubjectTypeAndAddTagnamesRange(string subject, List<ITagSelector> selectors)
        {
            if (subject == null)
            {
                return null;
            }

            subject = subject.ToLower();

            if (subject.Contains("button"))
            {
                selectors.Add(new TagNameSelector("button"));
                selectors.Add(new AttributeSelector("type", "button"));
                return WebElementType.Button;
            }
            else if (subject.Contains("link"))
            {
                selectors.Add(new TagNameSelector("a"));
                return WebElementType.Link;
            }
            else if(subject.Contains("input") || subject.Contains("textfield") || subject.Contains("textarea"))
            {
                selectors.Add(new TagNameSelector("input"));
                selectors.Add(new TagNameSelector("textArea"));
                return WebElementType.Input;
            }

            return WebElementType.Any;

            //return subject
            //    .Replace("button", "")
            //    .Replace("link", "")
            //    .Replace("input", "")
            //    .Replace("textarea", "")
            //    .Replace("textfield", "")
            //    .Trim();
        }

        private SearchDirection? ParseDirection(string direction)
        {
            if (direction == null)
            {
                return null;
            }
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
            var capturedValue = result.Groups[groupname].Value.Replace("'", "").Trim();

            if (string.IsNullOrEmpty(capturedValue))
            {
                return DEFAULT_VALUE_WHEN_WORD_IS_NOT_FOUND;
            }

            return capturedValue;
        }


        public override string ToString()
        {
            return $"{(!Order.HasValue ? "" :(this.Order.Value+1).ToOrdinalString())} " +
                   $"{(!SubjectType.HasValue? "" : SubjectType.ToString())} " +
                   $"{(string.IsNullOrEmpty(this.SubjectName)? "":$"'{this.SubjectName}'")} " +
                   $"{(!Direction.HasValue ? "" : GetDescription(Direction.Value))} " +
                   $"{(string.IsNullOrEmpty(Locale)?"":"'"+Locale+"'")}";
        }

        public string ToAnalysisString()
        {
            return "Instruction:\n" +
                   $"{nameof(Order)} = {Order}\n" +
                   $"{nameof(SubjectType)} = {SubjectType}\n" +
                   $"{nameof(SubjectName)} = {SubjectName}\n" +
                   $"{nameof(Direction)} = {Direction}\n" +
                   $"{nameof(Locale)} = {Locale}\n" +
                   $"{nameof(With)} = {With}\n" +
                   $"{nameof(TagNames)} = {(TagNames==null?"null":string.Join(", ",TagNames))}\n";
            ;

        }

        public static string GetDescription(SearchDirection enumValue)
        {
            var fi = typeof(SearchDirection).GetField(enumValue.ToString());

            if (null != fi)
            {
                object[] attrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }

            return enumValue.ToString();
        }
    }

    public enum CompareAccuracy
    {
        Exact,
        Partial,
    }
}