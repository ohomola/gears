using JetBrains.Annotations;

namespace Gears.Interpreter.Core
{
    public interface IKeyword : IHaveDocumentation
    {
        // TOP-LEVEL RICH SYNTACTIC PROPERTY
        [CanBeNull]
        string Instruction { set; }

        bool Matches(string textInstruction);

        

        object Execute();
        //TODO strong type
        string Status { get; set; }
        object Expect { get; set; }
        string HelpDescription { get; }
    }
}