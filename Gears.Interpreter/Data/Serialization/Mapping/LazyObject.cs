using System;

namespace Gears.Interpreter.Data.Serialization.Mapping
{
    public class LazyObject
    {
        

        public LazyObject(Func<object> func , Type type)
        {
            LazyInstance = new Lazy<object>(func);
            Type = type;
        }

        public LazyObject(object instance)
        {
            Instance = instance;
            Type = Instance.GetType();
        }

        private object Instance { get; set; }
        private Lazy<object> LazyInstance { get; set; }

        public bool IsValueCreated
        {
            get
            {
                if (Instance != null)
                {
                    return true;
                }
                return LazyInstance.IsValueCreated;
            }
        }

        public override string ToString()
        {
            if (IsValueCreated)
            {
                return Value.ToString();
            }

            return "Lazy Object (" + Type.Name+") ";
        }

        public object Value
        {
            get
            {
                if (Instance != null)
                {
                    return Instance;
                }
                return LazyInstance.Value;
            }
        }

        public Type Type { get; set; }
    }
}