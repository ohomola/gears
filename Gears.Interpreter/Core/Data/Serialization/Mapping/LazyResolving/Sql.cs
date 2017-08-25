using System;
using Castle.Core.Internal;
using Gears.Interpreter.Core.Adapters.DB;
using Gears.Interpreter.Core.ConfigObjects;
using Gears.Interpreter.Core.Registrations;

namespace Gears.Interpreter.Core.Data.Serialization.Mapping.LazyResolving
{
    public class SQL
    {
        public static string Select(string query)
        {
            if (ServiceLocator.IsInitialised())
            {
                var connectionString = ServiceLocator.Instance.Resolve<IDataContext>().Get<ConnectionString>();

                if (connectionString == null || connectionString.Text.IsNullOrEmpty())
                {
                    throw new ArgumentException("Please provide a ConnectionString config object to the Data Context ('use connectionstring <YourConnectionStringValue>' or include a connectionString object directly to your scenario");
                }

                return ServiceLocator.Instance.Resolve<IDatabaseAdapter>().SelectValue(connectionString.Text, query);
            }

            return null;
        }
    }
}