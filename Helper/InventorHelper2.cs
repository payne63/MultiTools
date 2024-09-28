using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Inventor;
using Microsoft.UI.Xaml.Controls;
using MultiTools;

namespace SplittableDataGridSAmple.Helper;

public static class InventorHelper2
{
    private static Inventor.Application _App;
    public static bool IsUse;
    public static event Action AppReadyToRun;
    
    public static void ShowApp()
    {
        if (_App != null) _App.Visible = true;
    }

    public static void HideApp()
    {
        if (_App != null) _App.Visible = false;
    }

    private static void CloseInstance() => _App?.Quit();

    public static async Task<Inventor.Application> GetInventorAppAsync()
    {
        return await Task.Run(()=>
        {
            try
            {
                var invType = Type.GetTypeFromProgID("Inventor.Application");
                var instance = (Inventor.Application)System.Activator.CreateInstance(invType);
                if (instance == null) throw new NullReferenceException();
                instance.Visible = false;
                while (!instance.Ready) { }
                AppReadyToRun?.Invoke();
                return instance;
            }
            catch (Exception e)
            {
                throw new Exception("impossible d'obtenir une app inventor", e);
            } 
        });
    }
    
    public static void SavePdf(Inventor.DrawingDocument drawingDoc, string folderPath)
    {
        var fullName = folderPath + @"\" + System.IO.Path.GetFileNameWithoutExtension(drawingDoc.DisplayName) + ".pdf";
        drawingDoc.SaveAs(fullName, true);
    }

    public static void SaveDxf(Inventor.DrawingDocument drawingDoc, string folderPath)
    {
        var dxfAddin = GetAddIn();

        var context = _App.TransientObjects.CreateTranslationContext();
        context.Type = Inventor.IOMechanismEnum.kFileBrowseIOMechanism;

        var options = _App.TransientObjects.CreateNameValueMap();

        var dataMedium = _App.TransientObjects.CreateDataMedium();
        dataMedium.FileName = folderPath + @"\" + System.IO.Path.GetFileNameWithoutExtension(drawingDoc.DisplayName) + ".dxf";

        //DXF Exporter does NOT overwrite and will hang if previous files found
        if (System.IO.File.Exists(dataMedium.FileName))
        {
            System.IO.File.Delete(dataMedium.FileName);
        }
        dxfAddin.SaveCopyAs(drawingDoc, context, options, dataMedium);
    }

    private static TranslatorAddIn GetAddIn()
    {
        var dxfAddin = (Inventor.TranslatorAddIn)_App.ApplicationAddIns.ItemById["{C24E3AC4-122E-11D5-8E91-0010B541CD80}"];
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
        _ => throw new Exception($"error on paper size converter {nameof( sheetSizeEnum)}")
    };

    public static PrintOrientationEnum PrinterOrientationConverter(PageOrientationTypeEnum orientationTypeEnum) => orientationTypeEnum switch
    {
        PageOrientationTypeEnum.kLandscapePageOrientation => PrintOrientationEnum.kLandscapeOrientation,
        PageOrientationTypeEnum.kPortraitPageOrientation => PrintOrientationEnum.kPortraitOrientation,
        PageOrientationTypeEnum.kDefaultPageOrientation => PrintOrientationEnum.kDefaultOrientation,
        _ => throw new ArgumentOutOfRangeException(nameof(orientationTypeEnum), orientationTypeEnum, null)
    };
}