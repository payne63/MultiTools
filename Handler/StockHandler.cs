using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using MultiTools.Models;

namespace MultiTools.Handler;

public static class StockHandler
{
    public static List<StockItem> StockItems = new List<StockItem>();
    public static List<StockMovement> StockMovements = new List<StockMovement>();
    private static XLWorkbook _workbook;
    private static IXLWorksheet _worksheet;

    public static void OpenExcelData(string path)
    {
        if (File.Exists(path))
        {
            _workbook = new XLWorkbook(path);
        }
        else
        {
            _workbook = new XLWorkbook();
        }

        if (_workbook.Worksheets != null)
        {
            _worksheet = _workbook.Worksheet(1);
        }

        var maxLine = _worksheet.LastRowUsed().RowNumber();
        if (maxLine == 0) return;
        var RowsOfData = _worksheet.Rows(1, maxLine);
        foreach (var row in RowsOfData)
        {
            var action = row.Cell(1).GetValue<StockMovement.ActionStock>();
            if (action == StockMovement.ActionStock.Init) ;
            {
                StockItems.Add(new StockItem(
                    row.Cell(3).GetValue<string>(),
                    row.Cell(4).GetValue<string>(),
                    row.Cell(5).GetValue<bool>()));
            }
        }

        foreach (var row in RowsOfData)
        {
            
        }
    }
}