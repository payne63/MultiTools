using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.FileProperties;
using CommunityToolkit.WinUI.UI;
using Inventor;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using MultiTools.Base;
using MultiTools.Helper;
using MultiTools.Models;
using FileInfo = System.IO.FileInfo;

namespace MultiTools.Tabs.InventorTab;

public sealed partial class InventorPrintTab : TabViewItemExtend, Interfaces.IInitTab, INotifyPropertyChanged
{
    public ObservableCollection<IdwPrintModel> IdwPrintModels;

    public Visibility DragAndDropVisibility => IdwPrintModels.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    
    private ObservableCollection<PrinterModel> _printerA4A3;

    public ObservableCollection<PrinterModel> PrinterA4A3
    {
        get => _printerA4A3;
        set
        {
            var actualUserName = GetSelectedPrinterA4A3;
            if (_printerA4A3 != null)
            {
                ComboBoxPrinterA4A3.SelectedItem = actualUserName;
            }

            _printerA4A3 = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<PrinterModel> _printerA2A1A0;

    public ObservableCollection<PrinterModel> PrinterA2A1A0
    {
        get => _printerA2A1A0;
        set
        {
            var actualUserName = GetSelectedPrinterA4A3;
            if (_printerA2A1A0 != null)
            {
                ComboBoxPrinterA4A3.SelectedItem = actualUserName;
            }

            _printerA2A1A0 = value;
            OnPropertyChanged();
        }
    }

    public PrinterModel GetSelectedPrinterA4A3 => ComboBoxPrinterA4A3.SelectedItem as PrinterModel;
    public PrinterModel GetSelectedPrinterA2A1A0 => ComboBoxPrinterA2A1A0.SelectedItem as PrinterModel;


    public int NbDrawing => IdwPrintModels.Where(x => x.MustBePrint).Count();

    public int NbA4Drawing =>
        IdwPrintModels.Count(x => x.SheetSize == DrawingSheetSizeEnum.kA4DrawingSheetSize && x.MustBePrint);

    public int NbA3Drawing =>
        IdwPrintModels.Count(x => x.SheetSize == DrawingSheetSizeEnum.kA3DrawingSheetSize && x.MustBePrint);

    public int NbA2Drawing =>
        IdwPrintModels.Count(x => x.SheetSize == DrawingSheetSizeEnum.kA2DrawingSheetSize && x.MustBePrint);

    public int NbA1Drawing =>
        IdwPrintModels.Count(x => x.SheetSize == DrawingSheetSizeEnum.kA1DrawingSheetSize && x.MustBePrint);

    public int NbA0Drawing =>
        IdwPrintModels.Count(x => x.SheetSize == DrawingSheetSizeEnum.kA0DrawingSheetSize && x.MustBePrint);

    public InventorPrintTab()
    {
        IdwPrintModels = new ObservableCollection<IdwPrintModel>();
        this.InitializeComponent();
    }

    public void InitTabAsync()
    {
        IdwPrintModels.CollectionChanged += (sender, e) =>
        {
            OnPropertyChanged(nameof(NbDrawing));
            OnPropertyChanged(nameof(NbA4Drawing));
            OnPropertyChanged(nameof(NbA3Drawing));
            OnPropertyChanged(nameof(NbA2Drawing));
            OnPropertyChanged(nameof(NbA1Drawing));
            OnPropertyChanged(nameof(NbA0Drawing));
            OnPropertyChanged(nameof(DragAndDropVisibility));
        };
        PrinterA4A3 = PrinterModel.GetSystemPrinter();
        PrinterA2A1A0 = PrinterModel.GetSystemPrinter();
    }

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems )== false )return;
        var filesInfos = new List<FileInfo>();
        foreach (var storageItem in await e.DataView.GetStorageItemsAsync())
        {
            var fileInfoOrigin = new FileInfo(storageItem.Path);
            if (fileInfoOrigin.Attributes.HasFlag(FileAttributes.Directory))
            {
                foreach (var fileInfo in Directory.GetFiles(fileInfoOrigin.FullName))
                {
                    if (fileInfo.EndsWith(".idw")) filesInfos.Add(new FileInfo(fileInfo));
                }
            }
            else
            {
                if (fileInfoOrigin.FullName.EndsWith(".idw")) filesInfos.Add(fileInfoOrigin);
            }
        }
        await SortAndAddToList(filesInfos);
    }

    private async Task SortAndAddToList(List<FileInfo> filesInfos)
    {
        filesInfos.Sort((a,b) => a.Name.CompareTo(b.Name));
        foreach (var fileInfo in filesInfos)
        {
            await foreach (var idwPrintModel in IdwPrintModel.GetIdwPrintModels(fileInfo.FullName,NbPdfdxfPropertyChanged))
            {
                IdwPrintModels.Add(idwPrintModel);
            }
        }
        
        //IdwPrintModels = new ObservableCollection<IdwPrintModel>( IdwPrintModels.Distinct());
        
        foreach (var idwPrintModel in IdwPrintModels)
        {
            if (!idwPrintModel.Name.EndsWith("L.idw")) idwPrintModel.MustBePrint = true;
        }
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e) => e.AcceptedOperation = DataPackageOperation.Move;

    private void Button_Click_OpenDrawing(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is IdwPrintModel contextIDWModel)
        {
            var contextDrawingDocument = InventorHelper2.GetDocument(contextIDWModel.FileInfoData.FullName);
            if (contextDrawingDocument == null) return;
            if (contextDrawingDocument is DrawingDocument drawingDocument)
            {
                InventorHelper2.ShowApp();
                drawingDocument.Sheets[contextIDWModel.PageNumber].Activate();
            }
        }
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        var contextIdwModel = ((FrameworkElement)sender).DataContext as IdwPrintModel;
        if (contextIdwModel == null) return;
        IdwPrintModels.Remove(contextIdwModel);
    }

    private async void Button_Click_Print(object sender, RoutedEventArgs e)
    {
        if (IsInterfaceEnabled == false) return;
        if (NbDrawing == 0) // si pas de plan on ne fait rien;
        {
            OpenSimpleMessage(XamlRoot, "Pas d'impressions sélectionnées");
            return;
        }

        if ((NbA4Drawing > 0 || NbA3Drawing > 0) && GetSelectedPrinterA4A3 == null)
        {
            OpenSimpleMessage(XamlRoot, "Sélectionner une imprimante pour la A4 et A3");
            return;
        }

        if ((NbA2Drawing > 0 || NbA1Drawing > 0 || NbA0Drawing > 0) && GetSelectedPrinterA2A1A0 == null)
        {
            OpenSimpleMessage(XamlRoot, "Sélectionner une imprimante pour la A2 et A1 et A0");
            return;
        }

        IsInterfaceEnabled = false;

        var printerSettings = new Dictionary<Inventor.DrawingSheetSizeEnum, string>()
        {
            {
                DrawingSheetSizeEnum.kA0DrawingSheetSize,
                GetSelectedPrinterA2A1A0?.Name ?? PrinterModel.NullPrinterModel.Name
            },
            {
                DrawingSheetSizeEnum.kA1DrawingSheetSize,
                GetSelectedPrinterA2A1A0?.Name ?? PrinterModel.NullPrinterModel.Name
            },
            {
                DrawingSheetSizeEnum.kA2DrawingSheetSize,
                GetSelectedPrinterA2A1A0?.Name ?? PrinterModel.NullPrinterModel.Name
            },
            {
                DrawingSheetSizeEnum.kA3DrawingSheetSize,
                GetSelectedPrinterA4A3?.Name ?? PrinterModel.NullPrinterModel.Name
            },
            {
                DrawingSheetSizeEnum.kA4DrawingSheetSize,
                GetSelectedPrinterA4A3?.Name ?? PrinterModel.NullPrinterModel.Name
            }
        };
        foreach (var idwPrintModel in IdwPrintModels)
        {
            idwPrintModel.ButtonEnable = false;
        }
        
        foreach (var printModel in IdwPrintModels)
        {
            if (printModel.MustBePrint == false) continue;
            GetProgressRingStatus(printModel).IsActive = true;
            await InventorHelper2.PrintFileAsync(printModel, printModel.PageNumber, printerSettings);
            GetProgressRingStatus(printModel).IsActive = false;
            printModel.MustBePrint = false;
        }

        foreach (var idwPrintModel in IdwPrintModels)
        {
            idwPrintModel.ButtonEnable = true;
        }

        IsInterfaceEnabled = true;
    }
    
    private ProgressRing GetProgressRingStatus(IdwPrintModel idwPrintModel)
    {
        var container = ListViewIDW.ContainerFromItem(idwPrintModel) as ListViewItem;
        return container.FindChild<ProgressRing>();
    }

    private void Button_Click_ClearAllList(object sender, RoutedEventArgs e) => IdwPrintModels.Clear();

    private void CheckBox_Checked_PDFChange(object sender, RoutedEventArgs e) => OnPropertyChanged(nameof(NbA4Drawing));
    private void CheckBox_Checked_DXFChange(object sender, RoutedEventArgs e) => OnPropertyChanged(nameof(NbA3Drawing));

    private void NbPdfdxfPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(NbDrawing));
        OnPropertyChanged(nameof(NbA4Drawing));
        OnPropertyChanged(nameof(NbA3Drawing));
        OnPropertyChanged(nameof(NbA2Drawing));
        OnPropertyChanged(nameof(NbA1Drawing));
        OnPropertyChanged(nameof(NbA0Drawing));
    }

    private void MenuFlyoutItem_Click_AllPrint(object sender, RoutedEventArgs e)
        => IdwPrintModels.ToList().ForEach(x =>
        {
            x.MustBePrint = true;
        });

    private void MenuFlyoutItem_Click_NonePrint(object sender, RoutedEventArgs e)
        => IdwPrintModels.ToList().ForEach(x =>
        {
            x.MustBePrint = false;
        });

    private void MenuFlyoutItem_Click_AutoPrintA4A3(object sender, RoutedEventArgs e)
    {
        foreach (var x in IdwPrintModels)
        {
            x.MustBePrint = false;
            if (x.Name.EndsWith("L.idw")) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA2DrawingSheetSize) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA1DrawingSheetSize) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA0DrawingSheetSize) continue;
            x.MustBePrint = true;
        }
    }

    private void MenuFlyoutItem_Click_AutoPrintA2A1A0(object sender, RoutedEventArgs e)
    {
        foreach (var x in IdwPrintModels)
        {
            x.MustBePrint = false;
            if (x.Name.EndsWith("L.idw")) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA3DrawingSheetSize) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA4DrawingSheetSize) continue;
            x.MustBePrint = true;
        }
    }

    private async void ButtonPickFiles(object sender, RoutedEventArgs e)
    {
        var storageFiles = await GetFilesOpenPicker(".idw");
        if (storageFiles.Count == 0) return;
        await SortAndAddToList( storageFiles.Select(s => new FileInfo(s.Path)).ToList());
    }

    private async void ButtonPickFolder(object sender, RoutedEventArgs e)
    {
        var storageFolder = await GetFolderOpenPicker();
        var storageFiles = await storageFolder.GetFilesAsync();
        await SortAndAddToList(storageFiles.
            Where(f => f.Name.EndsWith(".idw"))
            .Select(f=> new FileInfo(f.Path))
            .ToList());
}
}