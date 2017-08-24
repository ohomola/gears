namespace Gears.Interpreter.Core
{
    /// <summary>
    /// Marker interface used by Gears core when Opening external .dll files.
    /// Use this interface when creating a custom Keyword subclass, then run the Open keyword against your pre-built .dll to register your custom keywords as a plugin.
    /// </summary>
    public interface IGearsPlugin
    {
    }
}