using Gears.Interpreter.Core.ConfigObjects;

namespace Gears.Interpreter.Core.Adapters.DB
{
    public interface IDatabaseAdapter
    {
        string SelectValue(string connectionString, string query);
    }
}