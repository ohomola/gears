using System;
using System.Linq;
using System.Reflection;

namespace Gears.Interpreter.Library
{
    public class StringResolver
    {
        public static void Resolve(Keyword keyword)
        {
            var properties = keyword.GetType().GetProperties();

            var stringProperties = properties.Where(x => x.PropertyType == typeof(string));

            foreach (var stringProperty in stringProperties)
            {
                var value = stringProperty.GetValue(keyword) as string;
                if (value != null && value.Contains("{Random.Word()}"))
                {
                    value = value.Replace("{Random.Word()}", Random.Word());
                    stringProperty.SetValue(keyword, value);
                }
            }
        }
    }

    public class Random
    {
        public static string Word()
        {
            var random = new System.Random();

            string result = null;
            var alphabet = "abcdefghijklmnopqrst";

            for (int i = 0; i < 3; i++)
            {
                result = result + alphabet[random.Next(alphabet.Length)];
            }

            return result;
        }
    }
}