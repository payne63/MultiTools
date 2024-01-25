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

    private Application _app;

    public Application App
    {
        get => _app;
        private set => _app = value;
    }

    public static event Action Ready;

    private InventorHelper()
    {
    }

    public static async Task<InventorHelper> CreateAsync()
    {
        var inventorHelper = new InventorHelper();
        inventorHelper.App = await CreateInventorInstance();
        inventorHelper.App.SilentOperation = true; //suprime les promptes d'inventor
        Ready.Invoke();
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



}
