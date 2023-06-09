﻿using OfficeOpenXml;
using SplittableDataGridSAmple.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SplittableDataGridSAmple.Helper
{
    public static class EPPlusHelper
    {
        public static void ExportData(List<DataIQT> datas)
        {

            string fullFileNameExport = Path.GetDirectoryName(datas[0].FullPathName);
            fullFileNameExport += @"\" + Path.GetFileNameWithoutExtension(datas[0].NameFile)+".xlsx";
            if (File.Exists(fullFileNameExport)) { }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(fullFileNameExport))
            {
                ExcelWorksheet sheet;
                if (package.Workbook.Worksheets["Fiche Lancement"] == null) 
                {
                    sheet = package.Workbook.Worksheets.Add("Fiche Lancement");
                }
                else
                {
                    sheet = package.Workbook.Worksheets["Fiche Lancement"];
                }
                sheet.Cells["A1"].Value = datas[0].FullPathName;

                var rowIndex = 1;
                var mergeTitleRow =sheet.Cells[rowIndex, 1, rowIndex, 5];
                mergeTitleRow.Merge = true;
                mergeTitleRow.Value = "Fiche de Lancement";
                mergeTitleRow.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                mergeTitleRow.Style.Font.Size = 20;
                mergeTitleRow.Style.Font.Bold = true;
                rowIndex+=3;
                var rowOfTitleTable = rowIndex;

                sheet.Cells[rowOfTitleTable, 1].Value = "Nom";
                sheet.Cells[rowOfTitleTable, 2].Value = "Description";
                sheet.Cells[rowOfTitleTable, 3].Value = "Categorie";
                sheet.Cells[rowOfTitleTable, 4].Value = "Qt";
                sheet.Cells[rowOfTitleTable, 1].EntireRow.Style.Font.Bold = true;
                sheet.Cells[rowOfTitleTable, 1].EntireRow.Style.Font.Size = 13;
                sheet.Cells[rowOfTitleTable, 1, rowOfTitleTable, 4].AutoFilter = true;
                rowIndex++;
                foreach (var data in datas)
                {
                    sheet.Cells[rowIndex, 1].Value = data.NameFile;
                    sheet.Cells[rowIndex, 2].Value = data.Description;
                    sheet.Cells[rowIndex, 3].Value = data.Category.ToString();
                    sheet.Cells[rowIndex, 4].Value = data.Qt;
                    rowIndex++;
                }

                sheet.Columns.AutoFit();
                var rangeStyle = sheet.Cells[rowOfTitleTable, 1, sheet.Dimension.Rows, sheet.Dimension.Columns].Style;
                rangeStyle.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                rangeStyle.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                rangeStyle.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                rangeStyle.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;


                package.Save();
            }
        }

    }
}
