using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gears.Interpreter.Core.Data.Serialization.CSV;
using Gears.Interpreter.Core.Data.Serialization.Excel;
using Gears.Interpreter.Core.Data.Serialization.JUnit;
using Gears.Interpreter.Core.Data.Serialization.Mapping;

namespace Gears.Interpreter.Core.Data.Serialization
{
    internal class SerializerFactory
    {
        public ISerializer GetSerializerByPath(string path)
        {
            var extension = Path.GetExtension(path);

            if (IsExcel(extension) && IsExcelInstalled())
            {
                return  new ExcelSerializer(new InteropExcelGateway(path), new ColumnToPropertyMappingStrategy());
            }

            if (IsCSV(extension))
            {
                return new CsvSerializer(path);
            }

            if (IsXML(extension))
            {
                return new JUnitSerializer(path);
            }

            throw new NotSupportedException($"Given file extension {extension} is not supported.");
        }

        private bool IsXML(string extension)
        {
            return extension.ToLower().Equals(".xml");
        }

        private bool IsExcelInstalled()
        {
            return Type.GetTypeFromProgID("Excel.Application") != null;
        }

        private bool IsExcel(string extension)
        {
            return extension.ToLower().Equals(".xls") || extension.ToLower().Equals(".xlsx");
        }

        private bool IsCSV(string extension)
        {
            return extension.ToLower().Equals(".csv");
        }

        protected IEnumerable<Type> GetSubTypes(IEnumerable<Type> knownTypes)
        {
            return knownTypes.SelectMany(x => x.Assembly.GetTypes().Where(x.IsAssignableFrom));
        }
    }

}