namespace Gears.Interpreter.Core.ConfigObjects
{
    public class RunFirstStep : IHaveDocumentation
    {
        public string CreateDocumentationMarkDown()
        {
            return $@"
## RunFirstStep

Used in conjunction with DebugMode, forcing the interpreter to perform first step of the scenario (note that this can be anything, for instance a Run keyword)
> Note: To add this to any scenario, you can also use a command-line argument when executing Gears Interpreter -{nameof(RunFirstStep)}
";
        }

        public string CreateDocumentationTypeName()
        {
            return "RunFirstStep";
        }
    }
}