using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gears.Interpreter.Core.Adapters.DB
{
    public class DatabaseAdapter : IDatabaseAdapter
    {
        public string SelectValue(string connectionString, string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    var command = connection.CreateCommand();
                    command.CommandText = query;
                    var result = command.ExecuteScalar()?.ToString();
                    return result;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
