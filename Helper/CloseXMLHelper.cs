using ClosedXML.Excel;
using SplittableDataGridSAmple.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SplittableDataGridSAmple.Helper;

public class CloseXMLHelper
{
    public static void ExportData(List<DataIQT> datas,StorageFile storageFile, DateTime dateSave)
    {
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
        sheet.Cell("A3").Value = "date d'extraction";
        sheet.Cell("B3").Value = dateSave.ToString("dd/MM/yy hh:mm");

        var table = sheet.Cell("A4").InsertTable(datas.Select(
            x => new DataTable(x.NameFile,null,null, x.Description, x.Category.ToString(), x.Qt,null, null,null,null)));
        
        table.Theme = XLTableTheme.TableStyleMedium1;
        sheet.Cells("A4:J4").ToList().ForEach( cell => cell.Value = cell.Value.GetText().Replace('_',' '));
        var rowOfTable = table.RowCount();
        for (int i = 0; i < rowOfTable-1; i++)
        {
            sheet.Cell("A" + (i + 5).ToString()).SetHyperlink(new XLHyperlink(datas[i].FullPathName));
        }

        sheet.Columns().AdjustToContents();
        try
        {
            wb.SaveAs(storageFile.Path);
        }
        catch (Exception)
        {
            throw new Exception("Fichier en lecture seule");
        }

    }

    record DataTable (string Nom,string Plan_Details, string Plan_Laser, string Description,
        string Categorie, int Qt, string Fournisseur, string Status, string Date_De_Livraison,string Commentaires);
}
