using ClosedXML.Excel;
using OfficeOpenXml.FormulaParsing.ExcelUtilities;
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
    public static void ExportData(List<DataIQT> datas,StorageFile storageFile, DateTime dateSave, DataIQT masterData)
    {
        using var wb = new XLWorkbook();
        var sheet = wb.AddWorksheet("Fiche Lancement");

        var range = sheet.Range("A1:K1").Merge();
        range.SetValue("Fiche de lancement " + masterData.NameFile[..^4]).
            Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center).
            Font.SetFontSize(20).
            Font.SetBold(true);
        range.Style.Border.SetTopBorder(XLBorderStyleValues.Medium)
            .Border.SetBottomBorder(XLBorderStyleValues.Medium)
            .Border.SetRightBorder(XLBorderStyleValues.Medium)
            .Border.SetLeftBorder(XLBorderStyleValues.Medium);
        range.Style.Fill.SetBackgroundColor(XLColor.LightGray);

        SetLine(sheet.Cell("A2"), "Assemblage Maitre", masterData.NameFile);
        SetLine(sheet.Cell("A3"), "Emplacement", masterData.FullPathName);
        SetLine(sheet.Cell("A4"), "date d'extraction", dateSave.ToString("dd/MM/yy HH:mm"));
        SetLine(sheet.Cell("A5"), "Qt plan laser", datas.Where(x => x.Category == DataIBase.CategoryType.Laser).Count().ToString());

        var table = sheet.Cell("A6").InsertTable(datas.Select(
            x => new DataTable(x.NameFile,null,null, x.Description, x.Category.ToString(), x.Qt,null, null,null,null)));
        
        table.Theme = XLTableTheme.TableStyleMedium1;
        sheet.Cells("A6:K6").ToList().ForEach( cell => cell.Value = cell.Value.GetText().Replace('_',' '));
        var rowOfTable = table.RowCount();
        for (var i = 0; i < rowOfTable-1; i++)
        {
            var startCellule = 7; // attention à mettre à jour lord d'un rajout de ligne au dessus.
            var cell = sheet.Cell("A" + (i + startCellule).ToString());
            cell.SetHyperlink(new XLHyperlink(datas[i].FullPathName));
            cell.Style.Font.FontColor = XLColor.Black;
            cell.Style.Font.Underline = XLFontUnderlineValues.None;
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
        string Categorie, int Qt, string Fournisseur, string Status, string Date_De_Livraison,string Commentaires, float Prix_Unitaire =0f);

    private static void SetLine(IXLCell cellStart, string title, string value)
    {
        cellStart.Value = title +" :";
        cellStart.Style.Font.SetBold(true);
        var rangeExtend = cellStart.Worksheet.Range(cellStart.CellRight(),cellStart.CellRight(9)).Merge();
        rangeExtend.Value = value;
        var rangeBox = cellStart.Worksheet.Range(cellStart,cellStart.CellRight(10));
        rangeBox.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
        
    }
}
