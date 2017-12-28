using System.IO;
using Gears.Interpreter.App;
using Gears.Interpreter.App.Registrations;

namespace Gears.Interpreter.Tests.Pages
{
    public class TestBootstrapper
    {
        public static IInterpreter Setup()
        {
            Bootstrapper.Register();
            return Bootstrapper.ResolveInterpreter();
        }

        public static IInterpreter Setup(string csvContent)
        {
            var fileName = $"{Directory.CreateDirectory("./Input")}\\Test.csv";
            File.WriteAllText(fileName, csvContent);

            Bootstrapper.RegisterArguments(fileName);

            return Bootstrapper.ResolveInterpreter();
        }

        public static void TearDown()
        {
            Bootstrapper.Release();
            if (Directory.Exists("./Input"))
            {
                Directory.Delete("./Input", true);
            }
        }
    }
}