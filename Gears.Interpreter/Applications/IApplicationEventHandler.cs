namespace Gears.Interpreter.Applications
{
    public interface IApplicationEventHandler
    {
        void Register(ApplicationLoop applicationLoop);
    }
}