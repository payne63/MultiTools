using ClosedXML.Excel;
using SplittableDataGridSAmple.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Helper
{
    public class CloseXMLHelper
    {
        public static void ExportData(List<DataIQT> datas)
        {
            string fullFileNameExport = Path.GetDirectoryName(datas[0].FullPathName);
            fullFileNameExport += @"\" + Path.GetFileNameWithoutExtension(datas[0].NameFile) + ".xlsx";
            if (File.Exists(fullFileNameExport)) { }

            using var wb = new XLWorkbook();
            var sheet = wb.AddWorksheet("Fiche Lancement");


            var range = sheet.Range("A1:D1").Merge();
            range.SetValue("Fiche de lancement").
                Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).
                Font.SetFontSize(20).
                Font.SetBold(true);
            range.Style.Border.SetTopBorder(XLBorderStyleValues.Medium)
                .Border.SetBottomBorder(XLBorderStyleValues.Medium)
                .Border.SetRightBorder(XLBorderStyleValues.Medium)
                .Border.SetLeftBorder(XLBorderStyleValues.Medium);
            range.Style.Fill.SetBackgroundColor(XLColor.LightGray);

            sheet.Cell("A2").Value = "Assemblage Maître";
            sheet.Cell("B2").Value = datas[0].NameFile;

            var table = sheet.Cell("A4").InsertTable(datas.Select(x => new DataTable(x.NameFile, x.Description, x.Category.ToString(), x.Qt)));
            table.Theme = XLTableTheme.TableStyleMedium1;

            sheet.Columns().AdjustToContents();

            wb.SaveAs(fullFileNameExport);

        }

        record DataTable (string Name, string Description, string Catergorie, int Qt);
    }
}
