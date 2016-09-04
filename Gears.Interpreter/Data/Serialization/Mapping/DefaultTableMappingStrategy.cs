#region LICENSE
/*
Copyright 2016 Ondrej Homola <ondra.homola@gmail.com>

This file is part of Gears, a software automation and assistance framework.

Gears is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Gears is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Gears.Interpreter.Core.Registrations;
using Gears.Interpreter.Data.Core;

namespace Gears.Interpreter.Data.Serialization.Mapping
{
    public interface IDataSetMappingStrategy
    {
        DataTable MapToTable(IEnumerable<object> data);

        IEnumerable<object> MapToObjects(DataTable dataTable);

        IEnumerable<object> MapToObjects(DataSet dataSet);
    }

    public class DefaultTableMappingStrategy : IDataSetMappingStrategy
    {
        private readonly IDictionaryToObjectMapper _mapper = new DictionaryToObjectMapper();
        private readonly ITypeRegistry _typeRegistry;

        public const string DiscriminatorColumn = "Discriminator";

        public DefaultTableMappingStrategy()
        {
            if (ServiceLocator.IsInitialised())
            {
                _typeRegistry = ServiceLocator.Instance.Resolve<ITypeRegistry>();
            }
        }

        public DataTable MapToTable(IEnumerable<object> objects)
        {
            objects = objects.ToList();

            var properties = GetAllProperties(objects.Select(x => x.GetType()).Distinct()).ToList();

            var resultTable = CreateResultTable(properties);

            foreach (var obj in objects)
            {
                var row = resultTable.NewRow();

                row[DiscriminatorColumn] = obj.GetType().Name;

                foreach (var property in _mapper.GetTypeProperties(obj.GetType()))
                {
                    row[property.Name] = property.GetValue(obj, null);
                }

                resultTable.Rows.Add(row);
            }

            return resultTable;
        }

        private static DataTable CreateResultTable(List<string> properties)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn(DiscriminatorColumn));

            foreach (var p in properties)
            {
                table.Columns.Add(new DataColumn(p));
            }
            var resultTable = table;

            var headerRow = (from DataColumn column in resultTable.Columns select column.Caption).ToArray();

            resultTable.Rows.Add(headerRow);
            return resultTable;
        }

        public IEnumerable<object> MapToObjects(DataTable dataTable)
        {
            if (IsDiscriminatorPresent(dataTable))
            {
                var tableWithColumnNames = ConvertToNamedColumnTable(dataTable);

                return MapTableToObjects(tableWithColumnNames).ToList();
            }

            return new List<object>() { dataTable };
        }

        public IEnumerable<object> MapToObjects(DataSet dataSet)
        {
            if (IsHeaderPresent(dataSet))
            {
                foreach (DataTable table in dataSet.Tables)
                {
                    var objects = MapToObjects(table);
                    foreach (var o in objects)
                    {
                        yield return o;
                    }
                }
            }
            else
            {
                throw new NotSupportedException("Given table is not in correct format");
            }
        }

        private IEnumerable<object> MapTableToObjects(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                if (string.IsNullOrEmpty(row[DiscriminatorColumn].ToString()))
                {
                    continue;
                }

                object obj;

                try
                {
                    var dictionary = MapToDictionary(row);
                    obj = _mapper.CreateObject(_typeRegistry.GetFirstDTOType(row[DiscriminatorColumn].ToString()), dictionary);
                }
                catch (Exception ex)
                {
                    obj = new CorruptObject { Exception = ex };
                }

                yield return obj;
            }
        }

        private IDictionary<string, string> MapToDictionary(DataRow data)
        {
            var dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (DataColumn column in data.Table.Columns)
            {
                var columnName = (column.ColumnName ?? string.Empty).Trim();
                var field = column.ColumnName == null
                    ? string.Empty
                    : (data.Field<string>(column.ColumnName) ?? string.Empty).Trim();
                dictionary.Add(columnName, field);
            }

            return dictionary;
        }

        private bool IsHeaderPresent(DataSet dataSet)
        {
            return dataSet.Tables.Cast<DataTable>().Any(dataTable => dataTable.Rows[0][0].ToString() == DiscriminatorColumn);
        }

        private bool IsDiscriminatorPresent(DataTable table)
        {
            return string.Equals(table.Rows[0][0].ToString(), DiscriminatorColumn);
        }

        private DataTable ConvertToNamedColumnTable(DataTable dataTable)
        {
            var decoratedDataTable = CreateEmptyDataTableWithHeaders(dataTable.Rows[0]);

            for (int i = 1; i < dataTable.Rows.Count; i++)
            {
                decoratedDataTable.Rows.Add(dataTable.Rows[i].ItemArray);
            }

            return decoratedDataTable;
        }

        private DataTable CreateEmptyDataTableWithHeaders(DataRow header)
        {
            var table = new DataTable();

            foreach (var item in header.ItemArray.Select(i => i.ToString()))
            {
                table.Columns.Add(item);
            }

            return table;
        }

        private IEnumerable<string> GetAllProperties(IEnumerable<Type> types)
        {
            List<PropertyInfo> allProperties = new List<PropertyInfo>();

            foreach (var type in types)
            {
                allProperties.AddRange(_mapper.GetTypeProperties(type));
            }

            return allProperties.Select(p => p.Name).Distinct();
        }
    }
}
