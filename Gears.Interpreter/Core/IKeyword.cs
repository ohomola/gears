namespace Gears.Interpreter.Core
{
    public interface IKeyword : IHaveDocumentation
    {
        bool Matches(string textInstruction);
        void FromString(string textInstruction);
        object Execute();
        //TODO strong type
        string Status { get; set; }
        object Expect { get; set; }
        string HelpDescription { get; }
        string Specification { set; }
    }
}