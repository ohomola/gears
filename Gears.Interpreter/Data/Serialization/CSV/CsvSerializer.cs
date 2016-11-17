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
using System.IO;
using System.Linq;
using System.Text;
using Gears.Interpreter.Data.Serialization.Mapping;

namespace Gears.Interpreter.Data.Serialization.CSV
{
    internal class CsvSerializer : ISerializer
    {
        private readonly string _path;
        private readonly IDataSetMappingStrategy _mapper = new DefaultTableMappingStrategy();

        public CsvSerializer(String path)
        {
            _path = path;
        }

        public IEnumerable<object> Deserialize()
        {
            var rows = LoadSplittedRows();

            if (!rows.Any())
            {
                return new List<object>();
            }

            var dataTable = CreateDataTableWithColumns(rows.Select(row => row.Count()).Max(), "Sheet1");

            foreach (var row in rows)
            {
                dataTable.Rows.Add(row);
            }

            var dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            return _mapper.MapToObjects(dataSet);
        }

        private DataTable CreateDataTableWithColumns(int count, string tableName)
        {
            var dataTable = new DataTable(tableName);
            for (int i = 0; i < count; i++)
            {
                dataTable.Columns.Add();
            }

            return dataTable;
        }

        private IEnumerable<string[]> LoadSplittedRows()
        {
            string line;
            var rows = new List<string[]>();

            var fileStream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            var textReader = new StreamReader(fileStream, Encoding.GetEncoding(1250));

            while ((line = textReader.ReadLine()) != null)
            {
                rows.Add(line.Split(',').Select(x=>x.Trim()).ToArray());
            }

            textReader.Close();
            return rows;
        }

        public void Serialize(IEnumerable<object> dataObjects)
        {
            if (!dataObjects.Any()) return;

            var serializedData = _mapper.MapToTable(dataObjects);

            WriteData(serializedData);
        }

        private void WriteData(DataTable serializedData)
        {
            var fileStream = new FileStream(_path, FileMode.Create);
            var textWriter = new StreamWriter(fileStream, Encoding.GetEncoding(1250));

            foreach (DataRow row in serializedData.Rows)
            {
                textWriter.WriteLine(string.Join(",", row.ItemArray.Select(o => o.ToString())));
            }

            textWriter.Close();
        }

        public void Dispose()
        {
        }
    }
}
