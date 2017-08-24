namespace Gears.Interpreter.Core
{
    public interface IHaveDocumentation
    {
        string CreateDocumentationMarkDown();
        string CreateDocumentationTypeName();
    }
}