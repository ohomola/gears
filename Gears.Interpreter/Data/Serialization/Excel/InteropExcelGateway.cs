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
using System.Data;
using System.IO;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;

namespace Gears.Interpreter.Data.Serialization.Excel
{
    public class InteropExcelGateway : IDataSetGateway
    {
        private string _path;

        public InteropExcelGateway(string path)
        {
            _path = path;
        }

        public void Write(DataTable serializedData)
        {
            var excelApp = new Application { DisplayAlerts = false };
            var workbooks = excelApp.Workbooks;
            workbooks.Add();

            Worksheet sheet = excelApp.ActiveSheet;

            WriteData(sheet, serializedData);

            try
            {
                sheet.SaveAs(_path);
            }
            catch (Exception e)
            {
                throw new Exception("ExcelSerializer: Excel file could not be saved.", e);
            }
            finally
            {
                excelApp.Quit();
            }
        }

        public DataSet Read()
        {
            var dataSet = new DataSet();

            
            var excelApp = new Application {Visible = false, DisplayFullScreen = false, DisplayAlerts = false};
            var workBooks = excelApp.Workbooks;
            Workbook book = null;

            try
            {
                book = workBooks.Open(_path, ReadOnly: true);
                foreach (Worksheet sheet in book.Sheets)
                {
                    dataSet.Tables.Add(ReadTable(sheet));
                }

            }
            catch (Exception e)
            {
                throw new IOException("Failed to load excel document", e);
            }
            finally
            {
                if (book != null)
                {
                    book.Close(false, null, null);
                }
                excelApp.Quit();
            }
            
            return dataSet;
        }

        private DataTable ReadTable(Worksheet sheet)
        {
            var lastCell = sheet.UsedRange.SpecialCells(XlCellType.xlCellTypeLastCell);
            var columns = lastCell.Column;
            var rows = lastCell.Row;

            var wholeSheet = sheet.Range["A1", lastCell].Value;

            var resultTable = CreateDataTable(columns, sheet.Name);

            for (int i = 1; i <= rows; i++)
            {
                var row = resultTable.NewRow();

                for (int j = 1; j <= columns; j++)
                {
                    row[j - 1] = (GetValue(wholeSheet[i, j]));
                }

                resultTable.Rows.Add(row);
            }

            return resultTable;
        }

        private static dynamic GetValue(object value)
        {
            if (value is string)
            {
                value = ((string)value).Trim();
            }
            return value;
        }

        private DataTable CreateDataTable(int columns, string sheetName)
        {
            var resultTable = new DataTable(sheetName);
            for (int i = 0; i < columns; i++)
            {
                resultTable.Columns.Add();
            }
            return resultTable;
        }

        private void WriteData(Worksheet sheet, DataTable data)
        {
            for (int i = 0; i < data.Rows.Count; i++)
            {
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    sheet.Cells[(i + 2), j + 1] = data.Rows[i][j];
                }
            }
        }
    }
}