namespace Gears.Interpreter.Core.ConfigObjects
{
    public class ConnectionString : IConfig, IHaveDocumentation
    {
        public ConnectionString(string text)
        {
            Text = text;
        }

        public ConnectionString()
        {
        }

        public virtual string Text { get; set; }
        public string CreateDocumentationMarkDown()
        {
            return @"## {this.GetType().Name}

Configuration Type used to configure all SQL queries in your scenario.

> Note: This is a configuration, not an active keyword. It changes behaviour of your scenario by merely existing in your data context. Use 'Use' keyword to add it to your context and 'Off' keyword to take it away.


#### Scenario usages
| Discriminator | Text |
| ------------- | ---- |
| ConnectionString  | user id=myself;password=myPass;Data Source=.\sqlexpress;Database=MyDb.CustomerData; |
| Comment         | {SQL.Select(""Select top 1 name from Customers"")} |

#### Console usages
    use ConnectionString user id=myself;password=myPass;Data Source=.\sqlexpress;Database=MyDb.CustomerData;    
";
        }

        public string CreateDocumentationTypeName()
        {
            return nameof(ConnectionString);
        }

        public override string ToString() => $"Connection string : {Text}";
    }
}