using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorCode.Compilation.Languages;
using Inventor;

namespace SplittableDataGridSAmple.Helper;
internal class InventorHelper
{
    private static readonly List<Application> inventorApplications = new();

    public static void CloseAllInstance() => inventorApplications.ForEach(x => x.Quit());

    public static PaperSizeEnum printerSizeConverter(DrawingSheetSizeEnum sheetSizeEnum) => sheetSizeEnum switch
    {
        DrawingSheetSizeEnum.kA4DrawingSheetSize => PaperSizeEnum.kPaperSizeA4,
        DrawingSheetSizeEnum.kA3DrawingSheetSize => PaperSizeEnum.kPaperSizeA3,
        DrawingSheetSizeEnum.kA2DrawingSheetSize => PaperSizeEnum.kPaperSizeA2,
        DrawingSheetSizeEnum.kA1DrawingSheetSize => PaperSizeEnum.kPaperSizeA1,
        DrawingSheetSizeEnum.kA0DrawingSheetSize => PaperSizeEnum.kPaperSizeA0,
        _ => throw new NotImplementedException()
    };

    public static PrintOrientationEnum printerOrientationConverter(PageOrientationTypeEnum orientationTypeEnum) => orientationTypeEnum switch
    {
        PageOrientationTypeEnum.kLandscapePageOrientation => PrintOrientationEnum.kLandscapeOrientation,
        PageOrientationTypeEnum.kPortraitPageOrientation => PrintOrientationEnum.kPortraitOrientation,
        PageOrientationTypeEnum.kDefaultPageOrientation => PrintOrientationEnum.kDefaultOrientation
    };

    private Application _App;
    public Application App
    {
        get => _App;
        private set => _App = value;
    }

    private ApprenticeServerComponent _AppServerComp;
    public ApprenticeServerComponent AppServerComp
    {
        get => _AppServerComp;
        private set => _AppServerComp = value;
    }

    public static event Action Ready;

    private InventorHelper()
    {
    }

    public static async Task<InventorHelper> CreateAsync()
    {
        var inventorHelper = new InventorHelper();
        inventorHelper.App = await CreateInventorInstance();
        //inventorHelper.App.SilentOperation = true; //supprime les promptes d'inventor
        Ready.Invoke();
        inventorHelper.AppServerComp = new ApprenticeServerComponent();
        return inventorHelper;
    }

    private static async Task<Application> CreateInventorInstance()
    {
        Application application = null;
        await Task.Run(() =>
        {
            try// essaye d'ouvrir Inventor Session
        {
            Type invType = Type.GetTypeFromProgID("Inventor.Application");
            application = (Inventor.Application)System.Activator.CreateInstance(invType);
            application.Visible = false;
            while (!application.Ready) { }
        }
        catch (Exception ex)
        {
            throw new Exception("impossible d'ouvrir une session inventor :" + ex);
        }
        });
        inventorApplications.Add(application);
        return application;
    }

    ~InventorHelper()
    {
        if (App != null)
        {
            App.Quit();
            App = null;
        }
    }

    public void ShowApp() => App.Visible = true;
    public void HideApp() => App.Visible = false;

    public void SavePDF(Inventor.DrawingDocument drawingDoc, string folderPath)
    {
        var fullName = folderPath + @"\" + System.IO.Path.GetFileNameWithoutExtension(drawingDoc.DisplayName) + ".pdf";
        drawingDoc.SaveAs(fullName, true);
    }

    public void SaveDXF(Inventor.DrawingDocument drawingDoc, string folderPath)
    {
        var DXFAddin = (Inventor.TranslatorAddIn)App.ApplicationAddIns.ItemById["{C24E3AC4-122E-11D5-8E91-0010B541CD80}"];
        //active l'addon si il est pas activé
        if (DXFAddin.Activated == false) DXFAddin.Activate();

        //context
        var oContext = App.TransientObjects.CreateTranslationContext();
        oContext.Type = Inventor.IOMechanismEnum.kFileBrowseIOMechanism;

        //créer un NameValueMap object
        var oOptions = App.TransientObjects.CreateNameValueMap();

        //créer un dataMedium object
        var oDataMedium = App.TransientObjects.CreateDataMedium();
        //renseigne la destination
        oDataMedium.FileName = folderPath + @"\" + System.IO.Path.GetFileNameWithoutExtension(drawingDoc.DisplayName) + ".dxf";

        //DXF Exporter does NOT overwrite and will hang if previous files found
        if (System.IO.File.Exists(oDataMedium.FileName))
        {
            System.IO.File.Delete(oDataMedium.FileName);
        }

        DXFAddin.SaveCopyAs(drawingDoc, oContext, oOptions, oDataMedium);

    }

}
