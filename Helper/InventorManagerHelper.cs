using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Compression;
using System.Diagnostics;
using SplittableDataGridSAmple.Tabs;
using SplittableDataGridSAmple.Tabs.InventorTab;
using SplittableDataGridSAmple.Models;
using System.Runtime.InteropServices;
using SplittableDataGridSAmple.Services;
using System.Collections.ObjectModel;
using ColorCode.Compilation.Languages;
using System.Threading;
using Microsoft.UI.Xaml.Controls;
using SplittableDataGridSAmple.DialogPage;

namespace AvitechTools.Models;

public class InventorManagerHelper
{
    InventorLaserTab instanceLaserTab;
    InventorPrintTab instancePrintTab;

    public Dictionary<Inventor.DrawingSheetSizeEnum, Inventor.PaperSizeEnum> printerSizeConverter
        = new Dictionary<Inventor.DrawingSheetSizeEnum, Inventor.PaperSizeEnum>();
    public Dictionary<Inventor.PageOrientationTypeEnum, Inventor.PrintOrientationEnum> printerOrientationConverter
        = new Dictionary<Inventor.PageOrientationTypeEnum, Inventor.PrintOrientationEnum>();

    public Inventor.Application app = null;
    public static Inventor.Application ActualApp = null;

    /// <summary>
    /// Constructeur
    /// </summary>
    /// <param name="_viewModel"></param>
    public InventorManagerHelper(InventorLaserTab instanceTab)
    {
        this.instanceLaserTab = instanceTab;
    }

    public InventorManagerHelper(InventorPrintTab instanceTab)
    {
        printerSizeConverter.Add(Inventor.DrawingSheetSizeEnum.kA4DrawingSheetSize, Inventor.PaperSizeEnum.kPaperSizeA4);
        printerSizeConverter.Add(Inventor.DrawingSheetSizeEnum.kA3DrawingSheetSize, Inventor.PaperSizeEnum.kPaperSizeA3);
        printerSizeConverter.Add(Inventor.DrawingSheetSizeEnum.kA2DrawingSheetSize, Inventor.PaperSizeEnum.kPaperSizeA2);
        printerSizeConverter.Add(Inventor.DrawingSheetSizeEnum.kA1DrawingSheetSize, Inventor.PaperSizeEnum.kPaperSizeA1);
        printerSizeConverter.Add(Inventor.DrawingSheetSizeEnum.kA0DrawingSheetSize, Inventor.PaperSizeEnum.kPaperSizeA0);
        printerOrientationConverter.Add(Inventor.PageOrientationTypeEnum.kLandscapePageOrientation, Inventor.PrintOrientationEnum.kLandscapeOrientation);
        printerOrientationConverter.Add(Inventor.PageOrientationTypeEnum.kPortraitPageOrientation, Inventor.PrintOrientationEnum.kPortraitOrientation);
        this.instancePrintTab = instanceTab;
    }

    /// <summary>
    /// valide l'ouverture de l'application Inventor
    /// </summary>
    /// <returns></returns>
    public bool AppExist() => app != null ? true : false;


    /// <summary>
    /// Réalise l'ouverture des fichiers et les exports
    /// </summary>
    /// <param name="NbExportationToDo"></param>
    public async Task GenerateFile(List<IDWModel> listIDWModels)
    {
        instanceLaserTab.IsInderterminateProgressBar = true;
        instanceLaserTab.ProgressBarStatus = "Démarrage Inventor";

        await Task.Run(() => StartInventorNewInstance(instanceLaserTab.IsViewApp));

        instanceLaserTab.ProgressBarStatus = "Creation des PDF/DXF";
        instanceLaserTab.IsInderterminateProgressBar = false;
        instanceLaserTab.ProgressBarValue = 0;

        int NbExportationToDo = instanceLaserTab.NbPDFDrawing + instanceLaserTab.NbDXFDrawing;
        int NbJobDone = 0;

        var rootPathOfFile = listIDWModels.First().FileInfoData.Directory.FullName;

        string PDFFolder = rootPathOfFile + @"\PDF";
        if (listIDWModels.Exists(x => x.MakePDF) && !Directory.Exists(PDFFolder)) Directory.CreateDirectory(PDFFolder);

        string DXFFolder = rootPathOfFile + @"\DXF";
        if (listIDWModels.Exists(x => x.MakeDXF) && !Directory.Exists(DXFFolder)) Directory.CreateDirectory(DXFFolder);


        foreach (IDWModel plan in listIDWModels)
        {
            if (!plan.MakePDF && !plan.MakeDXF) continue; // si pas de pdf ni de dxf on passe au fichier suivant

            Inventor.Documents doc = app.Documents;

            doc.Open(plan.FileInfoData.FullName);
            Inventor.DrawingDocument drawingDoc = (Inventor.DrawingDocument)app.ActiveDocument;

            if (plan.MakePDF)
            {
                NbJobDone++;
                instanceLaserTab.ProgressBarValue = (NbJobDone * 100) / NbExportationToDo;
                await Task.Run(() => SavePDF(drawingDoc, PDFFolder));
            }
            if (plan.MakeDXF)
            {
                NbJobDone++;
                instanceLaserTab.ProgressBarValue = (NbJobDone * 100) / NbExportationToDo;
                await Task.Run(() => SaveDXF(drawingDoc, DXFFolder));
            }
            drawingDoc.Close();
        }
        app.Quit();
        app = null;


        if (instanceLaserTab.IsZipCompres)
        {
            instanceLaserTab.ProgressBarStatus = "Compression Zip";
            if (Directory.Exists(PDFFolder))
            {
                if (Directory.GetFiles(PDFFolder).Length != 0)
                {
                    if (File.Exists(PDFFolder + ".zip")) File.Delete(PDFFolder + ".zip");
                    await Task.Run(() => ZipFile.CreateFromDirectory(PDFFolder, PDFFolder + ".zip"));

                }
            }
            if (Directory.Exists(DXFFolder))
            {
                if (Directory.GetFiles(DXFFolder).Length != 0)
                {
                    if (File.Exists(DXFFolder + ".zip")) File.Delete(DXFFolder + ".zip");
                    await Task.Run(() => ZipFile.CreateFromDirectory(DXFFolder, DXFFolder + ".zip"));
                }
            }
        }
        await Task.Delay(1000);
        instanceLaserTab.ProgressBarStatus = @"Creation Terminée";
        await Task.Delay(1500);
        instanceLaserTab.ProgressBarValue = 0;
        instanceLaserTab.ProgressBarStatus = string.Empty;
    }

    /// <summary>
    /// sauvegarde au format dxf le ficher
    /// </summary>
    /// <param name="drawingDoc">document plan Inventor</param>
    /// <param name="folderPath">arborescence du répertoire</param>
    private void SaveDXF(Inventor.DrawingDocument drawingDoc, string folderPath)
    {
        Inventor.TranslatorAddIn DXFAddin;
        DXFAddin = (Inventor.TranslatorAddIn)app.ApplicationAddIns.ItemById["{C24E3AC4-122E-11D5-8E91-0010B541CD80}"];

        //active l'addon si il est pas activé
        if (DXFAddin.Activated == false) DXFAddin.Activate();

        //context
        Inventor.TranslationContext oContext;
        oContext = app.TransientObjects.CreateTranslationContext();
        oContext.Type = Inventor.IOMechanismEnum.kFileBrowseIOMechanism;

        //créer un NameValueMap object
        //Inventor.NameValueMap oOptions;
        var oOptions = app.TransientObjects.CreateNameValueMap();

        //Vérifier si le traducteur a les options 'SaveCopyAs'
        //if (DXFAddin.HasSaveCopyAsOptions[drawingDoc, oContext, oOptions])
        //{
        //string strIniFile = Path.Combine(Environment.CurrentDirectory, @"DXFFormat\", viewModel.SelectedFormatDXF.FileName);
        //oOptions.Value["Export_Acad_IniFile"] = strIniFile;
        //}

        //créer un dataMedium object
        //Inventor.DataMedium oDataMedium;
        var oDataMedium = app.TransientObjects.CreateDataMedium();
        //renseigne la destination
        oDataMedium.FileName = folderPath + @"\" + System.IO.Path.GetFileNameWithoutExtension(drawingDoc.DisplayName) + ".dxf";

        //DXF Exporter does NOT overwrite and will hang if previous files found
        if (File.Exists(oDataMedium.FileName))
        {
            File.Delete(oDataMedium.FileName);
        }

        DXFAddin.SaveCopyAs(drawingDoc, oContext, oOptions, oDataMedium);

    }

    /// <summary>
    /// sauvegarde au format pdf le ficher
    /// </summary>
    /// <param name="drawingDoc">document plan Inventor</param>
    /// <param name="folderPath">arborescence du répertoire</param>
    private static void SavePDF(Inventor.DrawingDocument drawingDoc, string folderPath)
    {
        var fullName = folderPath + @"\" + System.IO.Path.GetFileNameWithoutExtension(drawingDoc.DisplayName) + ".pdf";
        drawingDoc.SaveAs(fullName, true);
    }

    /// <summary>
    /// Demarre un instance de l'application Inventor
    /// </summary>
    /// <param name="viewModel">object viewModel</param>
    /// <returns></returns>
    /// 
    public bool StartInventorNewInstance(bool IsViewApp)
    {
        //BarManager.ResetLevel();
        try// essaye d'ouvrir Inventor Session
        {
            Type invType = Type.GetTypeFromProgID("Inventor.Application");
            app = (Inventor.Application)System.Activator.CreateInstance(invType);
            app.Visible = IsViewApp;
            while (!app.Ready) { }
        }
        catch (Exception ex)
        {
            throw new Exception("impossible d'ouvrir une session inventor :" + ex);
        }

        app.SilentOperation = true; //suprime les promptes d'inventor

        //BarManager.ResetLevel();
        return app != null ? true : false;
    }

    public static Inventor.Application GetActualInventorApp()
    {
        if (ActualApp != null) return ActualApp;
        var processInventor = Process.GetProcessesByName("Inventor");
        if (processInventor.Length == 0) return null;
        ActualApp = (Inventor.Application)MarshalService.GetActiveObject("Inventor.Application");
        if (ActualApp == null) return null;
        return ActualApp;
    }

    public async Task PrintList(List<IDWPrintModel> listIDWPrinterModel)
    {
        instancePrintTab.ProgressBarStatus = "Démarrage Inventor";
        await Task.Run(() => StartInventorNewInstance(instancePrintTab.IsViewApp));
        instancePrintTab.ProgressBarStatus = "Impression en cours";
        var NbPrintDone = 0;
        var NbPrintToDo = listIDWPrinterModel.Where(x => x.IsPrint).Count();
        foreach (IDWPrintModel plan in listIDWPrinterModel)
        {
            if (plan.IsPrint)
            {
                Inventor.Documents doc = app.Documents;
                await Task.Run(() => doc.Open(plan.FileInfoData.FullName));
                Inventor.DrawingDocument documentToPrint = (Inventor.DrawingDocument)app.ActiveDocument;
                if (await Print(documentToPrint, plan.PageNumber)) NbPrintDone++;
                instancePrintTab.ProgressBarValue = (NbPrintDone * 100) / NbPrintToDo;
                doc.CloseAll();
            }
        }

        app.Quit();
        app = null;

        await Task.Delay(1000);
        instancePrintTab.ProgressBarStatus = @"Impression terminée";
        await Task.Delay(1500);
        instancePrintTab.ProgressBarValue = 0;
        instancePrintTab.ProgressBarStatus = string.Empty;
    }

    /// <summary>
    /// Imprime le document avec les paramètres du cartouche
    /// </summary>
    /// <param name="documentToPrint"></param>
    public async Task<bool> Print(Inventor.DrawingDocument documentToPrint,int indexPage)
    {
        Inventor.DrawingSheetSizeEnum size = documentToPrint.Sheets[indexPage].Size;
        Inventor.PageOrientationTypeEnum orientation = documentToPrint.Sheets[indexPage].Orientation;

        Inventor.DrawingPrintManager printerManager = (Inventor.DrawingPrintManager)documentToPrint.PrintManager;

        printerManager.Printer = size switch
        {
            Inventor.DrawingSheetSizeEnum.kA0DrawingSheetSize => instancePrintTab.GetSelectedPrinterA2A1A0.Name,
            Inventor.DrawingSheetSizeEnum.kA1DrawingSheetSize => instancePrintTab.GetSelectedPrinterA2A1A0.Name,
            Inventor.DrawingSheetSizeEnum.kA2DrawingSheetSize => instancePrintTab.GetSelectedPrinterA2A1A0.Name,
            Inventor.DrawingSheetSizeEnum.kA3DrawingSheetSize => instancePrintTab.GetSelectedPrinterA4A3.Name,
            Inventor.DrawingSheetSizeEnum.kA4DrawingSheetSize => instancePrintTab.GetSelectedPrinterA4A3.Name,
            _ => throw new NotImplementedException(),
        };

        printerManager.NumberOfCopies = 1;
        printerManager.ScaleMode = Inventor.PrintScaleModeEnum.kPrintFullScale;//Inventor.PrintScaleModeEnum.kPrintBestFitScale;
        printerManager.PaperSize = printerSizeConverter[size];
        printerManager.Orientation = printerOrientationConverter[orientation];
        printerManager.PrintRange = Inventor.PrintRangeEnum.kPrintSheetRange;
        printerManager.SetSheetRange(indexPage, indexPage);
        await Task.Run(() => printerManager.SubmitPrint());

        return true;

    }
    
    public static void TestDialogPrinter()
    {
        //PageSetupDialog pageSetupDialog = new PageSetupDialog();
        //pageSetupDialog.PageSettings = new System.Drawing.Printing.PageSettings();
        //pageSetupDialog.PrinterSettings = new System.Drawing.Printing.PrinterSettings();
        //pageSetupDialog.ShowNetwork = false;
        //DialogResult result = pageSetupDialog.ShowDialog();
        //Debug.WriteLine(result);
    }

}
