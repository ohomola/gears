namespace Gears.Interpreter.Applications
{
    public interface IApplicationEventHandler
    {
        void Register(IInterpreter applicationLoop);
    }
}