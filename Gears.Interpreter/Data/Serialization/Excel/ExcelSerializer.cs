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
using Gears.Interpreter.Data.Serialization.Mapping;

namespace Gears.Interpreter.Data.Serialization.Excel
{
    public class ExcelSerializer : ISerializer
    {
        private readonly IDataSetGateway _dataSetGateway;
        private readonly IDataSetMappingStrategy _mappingStrategy;

        public ExcelSerializer(IDataSetGateway dataSetGateway, IDataSetMappingStrategy mappingStrategy)
        {
            _dataSetGateway = dataSetGateway;
            _mappingStrategy = mappingStrategy;
        }

        public void Dispose()
        {
        }

        public void Serialize(IEnumerable<object> dataObjects)
        {
            var serializedData = _mappingStrategy.MapToTable(dataObjects);

            _dataSetGateway.Write(serializedData);
        }

        public IEnumerable<object> Deserialize()
        {
            var data = _dataSetGateway.Read();

            var results = _mappingStrategy.MapToObjects(data);

            return results;
        }   
    }
}