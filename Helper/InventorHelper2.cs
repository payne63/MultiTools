using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventor;
using MultiTools.Models;
using File = System.IO.File;
using Path = System.IO.Path;

namespace MultiTools.Helper;

public static class InventorHelper2
{
    private static Application _App;

    public static bool IsUse; //TODO create an 'in use' system to avoid collision
    public static event Action AppReady;

    public static bool AppIsVisible
    {
        get
        {
            if (_App == null ) return false;
            return _App.Visible;
        }
    }

    public static event Action AppClosed;

    public static void ShowApp()
    {
        if (_App != null)
        {
            _App.Visible = true;
        }
    }
    
    public static void HideApp()
    {
        if (_App != null)
        {
            _App.Visible = false;
        }
    }

    public static void CloseInstance() => _App?.Quit();

    public static async Task GetInventorAppAsync()
    {
        if (_App != null) return;
        await Task.Run(() =>
        {
            try
            {
                var invType = Type.GetTypeFromProgID("Inventor.Application");
                var instance = (Application)Activator.CreateInstance(invType);
                if (instance == null) throw new NullReferenceException();
                instance.Visible = false;
                while (!instance.Ready)
                {
                }

                _App = instance;
            }
            catch (Exception e)
            {
                throw new Exception("impossible d'obtenir une app inventor", e);
            }
        });

        AppReady?.Invoke(); // must be outside the task.run
    }

    public static Document? GetDocument(string fullPathFileName)
    {
        return _App?.Documents.Open(fullPathFileName);
    }

    public static void SavePdf(DrawingDocument drawingDoc, string folderPath)
    {
        var fullName = folderPath + @"\" + Path.GetFileNameWithoutExtension(drawingDoc.DisplayName) + ".pdf";
        drawingDoc.SaveAs(fullName, true);
    }

    public static void SaveDxf(DrawingDocument drawingDoc, string folderPath)
    {
        var dxfAddin = GetAddIn();

        var context = _App.TransientObjects.CreateTranslationContext();
        context.Type = IOMechanismEnum.kFileBrowseIOMechanism;

        var options = _App.TransientObjects.CreateNameValueMap();

        var dataMedium = _App.TransientObjects.CreateDataMedium();
        dataMedium.FileName = folderPath + @"\" + Path.GetFileNameWithoutExtension(drawingDoc.DisplayName) +
                              ".dxf";

        //DXF Exporter does NOT overwrite and will hang if previous files found
        if (File.Exists(dataMedium.FileName))
        {
            File.Delete(dataMedium.FileName);
        }

        dxfAddin.SaveCopyAs(drawingDoc, context, options, dataMedium);
    }

    private static TranslatorAddIn GetAddIn()
    {
        var dxfAddin =
            (TranslatorAddIn)_App.ApplicationAddIns.ItemById["{C24E3AC4-122E-11D5-8E91-0010B541CD80}"];
        if (dxfAddin.Activated == false)
        {
            dxfAddin.Activate();
        }

        return dxfAddin;
    }

    public static PaperSizeEnum PrinterSizeConverter(DrawingSheetSizeEnum sheetSizeEnum) => sheetSizeEnum switch
    {
        DrawingSheetSizeEnum.kA4DrawingSheetSize => PaperSizeEnum.kPaperSizeA4,
        DrawingSheetSizeEnum.kA3DrawingSheetSize => PaperSizeEnum.kPaperSizeA3,
        DrawingSheetSizeEnum.kA2DrawingSheetSize => PaperSizeEnum.kPaperSizeA2,
        DrawingSheetSizeEnum.kA1DrawingSheetSize => PaperSizeEnum.kPaperSizeA1,
        DrawingSheetSizeEnum.kA0DrawingSheetSize => PaperSizeEnum.kPaperSizeA0,
        _ => throw new Exception($"error on paper size converter {nameof(sheetSizeEnum)}")
    };

    public static PrintOrientationEnum PrinterOrientationConverter(PageOrientationTypeEnum orientationTypeEnum) =>
        orientationTypeEnum switch
        {
            PageOrientationTypeEnum.kLandscapePageOrientation => PrintOrientationEnum.kLandscapeOrientation,
            PageOrientationTypeEnum.kPortraitPageOrientation => PrintOrientationEnum.kPortraitOrientation,
            PageOrientationTypeEnum.kDefaultPageOrientation => PrintOrientationEnum.kDefaultOrientation,
            _ => throw new ArgumentOutOfRangeException(nameof(orientationTypeEnum), orientationTypeEnum, null)
        };

    public static async Task<bool> PrintFileAsync(IdwPrintModel plan, int indexPage,
        Dictionary<DrawingSheetSizeEnum, string> printerSettings)
    {
        var doc = _App.Documents;
        await Task.Run(() => doc.Open(plan.FileInfoData.FullName));
        var documentToPrint = (DrawingDocument)_App.ActiveDocument;
        if (await Print(documentToPrint, plan.PageNumber, printerSettings[documentToPrint.Sheets[indexPage].Size]) ==
            false) return false;
        doc.CloseAll();
        return true;
    }

    private static async Task<bool> Print(DrawingDocument documentToPrint, int indexPage, string printerName)
    {
        var size = documentToPrint.Sheets[indexPage].Size;
        var orientation = documentToPrint.Sheets[indexPage].Orientation;

        var printerManager = (DrawingPrintManager)documentToPrint.PrintManager;

        printerManager.Printer = printerName;
        printerManager.NumberOfCopies = 1;
        printerManager.ScaleMode =
            PrintScaleModeEnum.kPrintFullScale; //Inventor.PrintScaleModeEnum.kPrintBestFitScale;
        printerManager.PaperSize = PrinterSizeConverter(size);
        printerManager.Orientation = PrinterOrientationConverter(orientation);
        printerManager.PrintRange = PrintRangeEnum.kPrintSheetRange;
        printerManager.SetSheetRange(indexPage, indexPage);
        await Task.Run(() => printerManager.SubmitPrint());

        return true;
    }
}