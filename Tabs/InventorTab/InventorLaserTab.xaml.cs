using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using AvitechTools.Models;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Net;
using CommunityToolkit.WinUI.UI.Triggers;
using Microsoft.VisualBasic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.UI.Xaml.Media.Imaging;
using MultiTools.Base;
using Windows.Storage.FileProperties;
using CommunityToolkit.WinUI.UI;
using Inventor;
using MultiTools.Helper;
using MultiTools.Models;
using FileAttributes = System.IO.FileAttributes;

namespace MultiTools.Tabs.InventorTab;

public sealed partial class InventorLaserTab : TabViewItemExtend, Interfaces.IInitTab, INotifyPropertyChanged
{
    public readonly ObservableCollection<IdwModel> IdwModels;

    public Visibility DragAndDropVisibility => IdwModels.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    
    private bool _IsZipCompres;

    public bool IsZipCompres
    {
        get
        {
            return _IsZipCompres;
        }
        set
        {
            _IsZipCompres = value;
            OnPropertyChanged();
        }
    }

    public int NbDrawing => IdwModels.Count();

    public int NbPDFDrawing => IdwModels.Where(x => x.MakePDF).Count();

    public int NbDXFDrawing => IdwModels.Where(x => x.MakeDXF).Count();

    public InventorLaserTab()
    {
        IdwModels = new ObservableCollection<IdwModel>();
        this.InitializeComponent();
    }

    public void InitTabAsync()
    {
        IdwModels.CollectionChanged += (sender, e) =>
        {
            OnPropertyChanged(nameof(NbDrawing));
            OnPropertyChanged(nameof(NbPDFDrawing));
            OnPropertyChanged(nameof(NbDXFDrawing));
            OnPropertyChanged(nameof(DragAndDropVisibility));
        };
    }

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var filesInfos = new List<FileInfo>();
            foreach (var storageItem in await e.DataView.GetStorageItemsAsync())
            {
                var fileInfoOrigine= new FileInfo(storageItem.Path);
                if (fileInfoOrigine.Attributes.HasFlag(FileAttributes.Directory))
                {
                    foreach (var fileName in Directory.GetFiles(fileInfoOrigine.FullName))
                    {
                        if (fileName.EndsWith(".idw")) filesInfos.Add(new FileInfo(fileName));
                    }
                }
                else
                {
                    if (fileInfoOrigine.FullName.EndsWith(".idw")) filesInfos.Add(new FileInfo(fileInfoOrigine.FullName));
                }
            }

            await SortAndAddToList(filesInfos);
        }

        var sortableList = new List<IdwModel>(IdwModels);
        sortableList.Sort((IdwModel A, IdwModel B) => string.Compare(A.Name, B.Name));
        for (int i = 0; i < sortableList.Count; i++)
        {
            IdwModels.Move(IdwModels.IndexOf(sortableList[i]), i);
        }
    }

    private async Task SortAndAddToList(List<FileInfo> filesInfos)
    {
        IsInterfaceEnabled = false;
        ApprenticeHelper.ResetApprenticeServer();
        filesInfos.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        foreach (var fileInfo in filesInfos)
        {
            IdwModels.Add(new IdwModel(fileInfo, nbPDFDXFPropertyChanged));
        }

        IsInterfaceEnabled = true;
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }
    
    private void Button_Click_OpenDrawing(object sender, RoutedEventArgs e)
    {
        var contextIdwModel = ((FrameworkElement)sender).DataContext as IdwModel;
        if (contextIdwModel != null)
        {
            InventorHelper2.GetDocument(contextIdwModel.FileInfoData.FullName);
            InventorHelper2.ShowApp();
        }
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        var contextIDWModel = ((FrameworkElement)sender).DataContext as IdwModel;
        IdwModels.Remove(contextIDWModel);
    }

    private async void Button_Click_GeneratePdfDxf(object sender, RoutedEventArgs e)
    {
        if (IdwModels.Count == 0) return; // si pas de plan on ne fait rien;
        if (IsInterfaceEnabled == false) return;
        IsInterfaceEnabled = false;
        // await GeneratePdfDxfAsync(new List<IDWModel>(IdwModels));
        foreach (var idwModel in IdwModels)
        {
            idwModel.ButtonEnable = false;
        }
        await GenerateAllPdfDxfAsync(IdwModels.Where(m => m.MakePDF || m.MakeDXF).ToList());
        foreach (var idwModel in IdwModels)
        {
            idwModel.ButtonEnable = true;
        }
        IsInterfaceEnabled = true;
    }
    
    // private ProgressRing GetProgressRingStatus(IdwModel idwPrintModel)
    // {
    //     var container = ListViewIDW.ContainerFromItem(idwPrintModel) as ListViewItem;
    //     return container.FindChild<ProgressRing>();
    // }

    private async Task GenerateAllPdfDxfAsync(List<IdwModel> IdwModels)
    {
        var rootPathOfFile = IdwModels.First()?.FileInfoData?.Directory?.FullName;
        if (rootPathOfFile == null) return;

        string PDFFolder = rootPathOfFile + @"\PDF";
        if (IdwModels.Exists(x => x.MakePDF) && !Directory.Exists(PDFFolder)) Directory.CreateDirectory(PDFFolder);

        string DXFFolder = rootPathOfFile + @"\DXF";
        if (IdwModels.Exists(x => x.MakeDXF) && !Directory.Exists(DXFFolder)) Directory.CreateDirectory(DXFFolder);

        foreach (IdwModel plan in IdwModels)
        {
            GetProgressRingStatus2(ListViewIDW, plan).IsActive = true;
            await GeneratePdfDxfAsync(plan, PDFFolder, DXFFolder);
            GetProgressRingStatus2(ListViewIDW, plan).IsActive = false;
        }

        if (IsZipCompres) await GenerateZip(PDFFolder, DXFFolder);
    }

    private static async Task GeneratePdfDxfAsync(IdwModel plan, string PDFFolder, string DXFFolder)
    {
        var drawingDoc = InventorHelper2.GetDocument(plan.FileInfoData.FullName) as DrawingDocument;

        if (drawingDoc == null) return;
        if (plan.MakePDF) await Task.Run(() => InventorHelper2.SavePdf(drawingDoc, PDFFolder));
        if (plan.MakeDXF) await Task.Run(() => InventorHelper2.SaveDxf(drawingDoc, DXFFolder));

        drawingDoc.Close();
    }

    private static async Task GenerateZip(string PDFFolder, string DXFFolder)
    {
        if (Directory.Exists(PDFFolder))
        {
            if (Directory.GetFiles(PDFFolder).Length != 0)
            {
                if (System.IO.File.Exists(PDFFolder + ".zip")) System.IO.File.Delete(PDFFolder + ".zip");
                await Task.Run(() => ZipFile.CreateFromDirectory(PDFFolder, PDFFolder + ".zip"));
            }
        }

        if (Directory.Exists(DXFFolder))
        {
            if (Directory.GetFiles(DXFFolder).Length != 0)
            {
                if (System.IO.File.Exists(DXFFolder + ".zip")) System.IO.File.Delete(DXFFolder + ".zip");
                await Task.Run(() => ZipFile.CreateFromDirectory(DXFFolder, DXFFolder + ".zip"));
            }
        }
    }

    private void Button_Click_ClearAllList(object sender, RoutedEventArgs e) => IdwModels.Clear();

    private void CheckBox_Checked_PDFChange(object sender, RoutedEventArgs e) =>
        OnPropertyChanged(nameof(NbPDFDrawing));

    private void CheckBox_Checked_DXFChange(object sender, RoutedEventArgs e) =>
        OnPropertyChanged(nameof(NbDXFDrawing));

    private void nbPDFDXFPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(NbPDFDrawing));
        OnPropertyChanged(nameof(NbDXFDrawing));
    }

    private void MenuFlyoutItem_Click_AllPDF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.MakePDF = true;
    });

    private void MenuFlyoutItem_Click_NonePDF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.MakePDF = false;
    });

    private void MenuFlyoutItem_Click_AutoPDF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.AutoSelectPDFStatus();
    });

    private void MenuFlyoutItem_Click_AllDXF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.MakeDXF = true;
    });

    private void MenuFlyoutItem_Click_NoneDXF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.MakeDXF = false;
    });

    private void MenuFlyoutItem_Click_AutoDXF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.AutoSelectDXFStatus();
    });

    private async void ButtonPickFile(object sender, RoutedEventArgs e)
    {
        var storageFiles = await GetFilesOpenPicker(".idw");
        if (storageFiles.Count == 0) return;
        await SortAndAddToList(storageFiles.Select(s => new FileInfo(s.Path)).ToList());
    }

    private async void ButtonPickFolder(object sender, RoutedEventArgs e)
    {
        var storageFolder = await GetFolderOpenPicker();
        if (storageFolder == null) return;
        var storageFiles = await storageFolder.GetFilesAsync();
        await SortAndAddToList(storageFiles.Where(f => f.Name.EndsWith(".idw"))
            .Select(f => new FileInfo(f.Path))
            .ToList());
    }
}