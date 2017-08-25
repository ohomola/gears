namespace Gears.Interpreter.Core.ConfigObjects
{
    public class SkipAssertions : IHaveDocumentation, IConfig
    {
        public string CreateDocumentationMarkDown()
        {
            return $@"## {this.GetType().Name}

Configuration Type used to turn off assertions executions.

> Note: This is a configuration, not an active keyword. It changes behaviour of your scenario by merely existing in your data context. Use 'Use' keyword to add it to your context and 'Off' keyword to take it away.


#### Scenario usages
| Discriminator | What |
| ------------- | ---- |
| Use           | SkipAssertions |
| IsVisible         | Somthing which is broken and we know it |
| Off           | SkipAssertions|

#### Console usages
    use SkipAssertions
    run
    SkipAssertions off
";
        }

        public string CreateDocumentationTypeName()
        {
            return nameof(SkipAssertions);
        }

        public override string ToString()
        {
            return "SkipAssertions (any assertions keyword is automatically skipped)";
        }
    }
}