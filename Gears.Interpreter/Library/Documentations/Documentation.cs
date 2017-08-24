using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gears.Interpreter.Core;

namespace Gears.Interpreter.Library.Documentations
{
    public class Documentation
    {

        public const string ConsoleKeywordNote =
            "\n> Console control Keyword - use directly in console to control application (not recommended as part of scenario).\n\n";
        private readonly List<IHaveDocumentation> _keywords;

        public Documentation(IEnumerable<IHaveDocumentation> keywords)
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

            sb.Append(RestOfTheDocumentation);
            return sb.ToString();
        }

        public string RestOfTheDocumentation = @"



# Common Keyword properties
### SKIP
Any keyword can be tagged to be always skipped (not executed) by adding column called 'Skip' with value equal to TRUE (case insensitive).

### WaitAfter
You can specify the amount of miliseconds to wait after execution of a keyword with this field. Purpose is to save cluttering your scenario with Wait steps if your website is timeout sensitive.

### WaitBefore
You can specify the amount of miliseconds to wait before execution of a keyword with this field. Purpose is to save cluttering your scenario with Wait steps if your website is timeout sensitive.

### ScreenshotAfter
Use True/False to turn on screensaving after execution of the keyword. You can also use specific string instead to give the screenshot a filename prefix (equivalent to giving a What argument to Savescreenshot keyword)

#### Scenario usages
| Discriminator | What  | WaitAfter |
| ------------- | ----- | ------ |
| Click         | login | 250    |


# Web element instructions
Web element instruction is a text passed to all web-based keywords to specify the query for the keyword. Instruction must follow a simplified english expression syntax. The syntax structure effectivelly breaks the text into several components by it's structural position. There total of 5 different elements:

### Syntax parts

1. **Order** - this indicates you're asking for Nth element. For example the '1st' in '1st button'. If not specified, 1 is used.
1. **Subject type** - Indicates the type of element you are looking for. Default is Any element type. If specified can be one of the following:
  - button
  - link
  - input
  - textArea _(treated same as input)_
  - textfield _(treated same as input)_
1. **Accuracy** - use word 'like' before indicating **Subject name** to allow partial text matches. Skip this to match only entire text of element.
1. **Subject name** - visible name of what you are looking for. This must be full - no partial matches are considered.
1. **Direction** - indicates where you are looking for the element. Default is 'Near' which is generic enough although not reliable when multiple occurences of the seached element exist. This can be also one of the following values:
  - left from - only takes elements left from another element (must provide Locale)
  - right from - only takes elements right from another element (must provide Locale)
  - above - only takes elements above another element (must provide Locale)
  - below - only takes elements below another element (must provide Locale)
  - from right - goes from right edge of the browser i.e. sorts elements by their X coordinates descending.
  - from left - goes from left edge of the browser i.e. sorts elements by their X coordinates ascending.
  - from bottom- goes from left edge of the browser i.e. sorts elements by their Y coordinates descending.
  - from top - goes from left edge of the browser i.e. sorts elements by their Y coordinates ascending.
1. **Locale** specified only if relative direction is specified. This is a visible text of another element.

The query is interpreted in the following manner:

| Whole instruction                  | Order | Subject Type | Subject Name | Direction | Locale | 
| ------                             | ----- | ----         | -------      | --------- | ----   | 
| 1st input Password right from Login| 0     | Input        | Password     | RightFrom | Login  |

Note that none of the parts is mandatory, so the following samples are all valid (if perhaps not all sensible):

| Whole instruction                 | Order | Subject Type | Subject Name | Direction | Locale | Accuracy |
|---                                | ----- | ----         | -------      | --------- | ----   | Exact |
| button                            | 1     | Button       |   _Anything_ | _Anywhere_|        | Exact |
| login                             | 1     | _Anything_   |   Login      | _Anywhere_|        | Exact |
| 1st                               | 1     | _Anything_   |   Login      | _Anywhere_|        | Exact |
| above login                       | 1     | _Anything_   |   _Anything_ | Above     | Login  | Exact |
| 2th login                         | 2     | _Anything_   |   Login      | _Anywhere_|        | Exact |
| 2nd button like log               | 2     | Button       |   Log        | _Anywhere_|        | Partial |
| like 'log'                        | 0     | _Anything_   |   Log        | _Anywhere_|        | Partial |

> Nothing is case-sensitive.

# Expressions
Expressions are parts of user instructions written with special syntax, that cause the system to evaluate them into different values. Those are either used for data parametrisation of tests (via remembered values expressions) or for run-time actions such as randomly generated numbers, dates, or math operations.

### Remembered variables
Remembered variables are referenced by expressions in brackets []. Use these anywhere in text to cause system to evaluate the brackets into the variable's value. See [Remember](remember) for more info.

### Code chunk
TODO

# Command line arguments
### Memory variables
You can instruct the application to automatically add a variable on start time by adding argument with specific syntax

The following example will add a variable _myVariable_ with value ""HelloWorld"".

    Gears.Interpreter.exe -myVariable=HelloWorld

Sample usage is to specify base url for your application tests and control them from outer scope.
";

        public string CreateSideMenuMarkDown()
        {
            var orderedKeywords = _keywords.OrderBy(x => x.GetType().Name).ToList();

            var sb = new StringBuilder();

            sb.AppendLine("- [List of Keywords](Documentation#list-of-keywords)  ");

            foreach (var keyword in orderedKeywords)
            {
                sb.AppendLine($"  - [{keyword.CreateDocumentationTypeName()}](Documentation#{keyword.CreateDocumentationTypeName().ToLower()})");
            }

            sb.AppendLine();

            sb.Append(@"


- [Common Keyword properties](Documentation#common-keyword-properties)  
- [Web element instructions](Documentation#web-element-instructions)  
- [Expressions](Documentation#expressions)  
- [Command-line arguments](Documentation#command-line-arguments)
");
            return sb.ToString();
        }
    }
}
