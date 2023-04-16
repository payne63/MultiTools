using Microsoft.Office.Interop.Excel;
using SplittableDataGridSAmple.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace SplittableDataGridSAmple.Helper
{
    internal static class ExcelHelper
    {
        public static void ExportData( List<DataIQT> datas )
        {
            string fullFileNameExport = Path.GetDirectoryName(datas[0].FullPathName);
            fullFileNameExport += @"\"+Path.GetFileNameWithoutExtension(datas[0].NameFile);
            Excel.Application excel;
            try
            {
                excel = new Excel.Application(); //{ Visible = true };
            }
            catch (Exception)
            {
                throw new Exception("ouverture d'excel crash");
            }
            Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
            Excel.Worksheet sheet = (Excel.Worksheet)workbook.ActiveSheet;
            var rowIndex = 1;

            ((Excel.Range)sheet.Cells[rowIndex, 1]).Value = "Nom";
            ((Excel.Range)sheet.Cells[rowIndex, 2]).Value = "Description";
            ((Excel.Range)sheet.Cells[rowIndex, 3]).Value = "Categorie";
            ((Excel.Range)sheet.Cells[rowIndex, 4]).Value = "Qt";
            ((Excel.Range)sheet.Cells[rowIndex, 1]).EntireRow.Font.Bold = true;
            ((Excel.Range)sheet.Cells[rowIndex, 1]).EntireRow.Font.Size = 13;
            ((Excel.Range)sheet.Cells[rowIndex, 1]).EntireRow.AutoFilter2(1);
            rowIndex++;
            foreach (var data in datas)
            {
                ((Excel.Range)sheet.Cells[ rowIndex,1]).Value = data.NameFile;
                ((Excel.Range)sheet.Cells[ rowIndex,2]).Value = data.Description;
                ((Excel.Range)sheet.Cells[ rowIndex,3]).Value = data.Category.ToString();
                ((Excel.Range)sheet.Cells[ rowIndex,4]).Value = data.Qt;
                rowIndex++;
            }

            sheet.Columns.AutoFit();
            sheet.Rows.AutoFit();

            Excel.Range tRange = sheet.UsedRange;
            tRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            tRange.Borders.Weight = Excel.XlBorderWeight.xlThin;
            try
            {
                workbook.SaveAs(fullFileNameExport);
            }
            catch (Exception)
            {
            }
            workbook.Close();
            excel.Quit();
        }
    }
}
