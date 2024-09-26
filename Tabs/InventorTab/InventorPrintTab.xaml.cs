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
using SplittableDataGridSAmple.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using CommunityToolkit.WinUI.UI.Triggers;
using Microsoft.VisualBasic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using SplittableDataGridSAmple.Base;

namespace SplittableDataGridSAmple.Tabs;

public sealed partial class InventorPrintTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
{
    public ObservableCollection<IDWPrintModel> IDWPrintModels = new();

    InventorManagerHelper inventorManager;

    #region PropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion

    private ObservableCollection<PrinterModel> _PrinterA4A3;
    public ObservableCollection<PrinterModel> PrinterA4A3
    {
        get => _PrinterA4A3; set
        {
            var actualUserName = GetSelectedPrinterA4A3;
            if (_PrinterA4A3 != null)
            {
                ComboBoxPrinterA4A3.SelectedItem = actualUserName;
            }
            _PrinterA4A3 = value; OnPropertyChanged();
        }
    }

    private ObservableCollection<PrinterModel> _PrinterA2A1A0;
    public ObservableCollection<PrinterModel> PrinterA2A1A0
    {
        get => _PrinterA2A1A0; set
        {
            var actualUserName = GetSelectedPrinterA4A3;
            if (_PrinterA2A1A0 != null)
            {
                ComboBoxPrinterA4A3.SelectedItem = actualUserName;
            }
            _PrinterA2A1A0 = value; OnPropertyChanged();
        }
    }

    public PrinterModel GetSelectedPrinterA4A3 => ComboBoxPrinterA4A3.SelectedItem as PrinterModel;
    public PrinterModel GetSelectedPrinterA2A1A0 => ComboBoxPrinterA2A1A0.SelectedItem as PrinterModel;

    private bool _IsViewApp;
    public bool IsViewApp
    {
        get => _IsViewApp;
        set { _IsViewApp = value; OnPropertyChanged(); }
    }

    private double _ProgressBarValue;

    public double ProgressBarValue
    {
        get => _ProgressBarValue;
        set { _ProgressBarValue = value; OnPropertyChanged(); }
    }

    private string _ProgressBarStatus = string.Empty;

    public string ProgressBarStatus
    {
        get => _ProgressBarStatus;
        set { _ProgressBarStatus = value; OnPropertyChanged(); }
    }

    private bool _IsInderterminateProgressBar;

    public bool IsInderterminateProgressBar
    {
        get => _IsInderterminateProgressBar;
        set { _IsInderterminateProgressBar = value; OnPropertyChanged(); }
    }

    private bool _IsInterfaceEnabled = true;

    public bool IsInterfaceEnabled
    {
        get => _IsInterfaceEnabled;
        set { _IsInterfaceEnabled = value; OnPropertyChanged(); }
    }


    public int NbDrawing { get => IDWPrintModels.Where(x => x.IsPrint).Count(); }
    public int NbA4Drawing
    {
        get => IDWPrintModels.Where(
        x => x.SheetSize == Inventor.DrawingSheetSizeEnum.kA4DrawingSheetSize && x.IsPrint).Count();
    }
    public int NbA3Drawing
    {
        get => IDWPrintModels.Where(
        x => x.SheetSize == Inventor.DrawingSheetSizeEnum.kA3DrawingSheetSize && x.IsPrint).Count();
    }
    public int NbA2Drawing
    {
        get => IDWPrintModels.Where(
        x => x.SheetSize == Inventor.DrawingSheetSizeEnum.kA2DrawingSheetSize && x.IsPrint).Count();
    }
    public int NbA1Drawing
    {
        get => IDWPrintModels.Where(
        x => x.SheetSize == Inventor.DrawingSheetSizeEnum.kA1DrawingSheetSize && x.IsPrint).Count();
    }
    public int NbA0Drawing
    {
        get => IDWPrintModels.Where(
        x => x.SheetSize == Inventor.DrawingSheetSizeEnum.kA0DrawingSheetSize && x.IsPrint).Count();
    }

    public InventorPrintTab()
    {
        this.InitializeComponent();
    }

    public void InitTabAsync()
    {
        IDWPrintModels.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
        {
            OnPropertyChanged(nameof(NbDrawing));
            OnPropertyChanged(nameof(NbA4Drawing));
            OnPropertyChanged(nameof(NbA3Drawing));
            OnPropertyChanged(nameof(NbA2Drawing));
            OnPropertyChanged(nameof(NbA1Drawing));
            OnPropertyChanged(nameof(NbA0Drawing));
        };
        PrinterA4A3 = PrinterModel.GetSystemPrinter();
        PrinterA2A1A0 = PrinterModel.GetSystemPrinter();
        inventorManager = new InventorManagerHelper(this);
    }

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0)
            {
                foreach (var file in items)
                {
                    if (!Directory.Exists(file.Path)) //si ce n'est pas un r�pertoire
                    {
                        if (file.Name.EndsWith(".idw"))
                        {
                            AddPrinterAction(file.Path);
                        }
                    }
                    if (Directory.Exists(file.Path)) // si c'est un r�pertoire
                    {
                        var filesInDirectory = Directory.GetFiles(file.Path);
                        foreach (var f in filesInDirectory)
                        {
                            if (f.EndsWith(".idw"))
                            {
                                AddPrinterAction(f);
                            }
                        }
                    }
                }
            }
        }
        var sortableList = new List<IDWPrintModel>(IDWPrintModels);
        sortableList.Sort((IDWPrintModel A, IDWPrintModel B) => string.Compare(A.Name, B.Name));
        for (int i = 0; i < sortableList.Count; i++)
        {
            IDWPrintModels.Move(IDWPrintModels.IndexOf(sortableList[i]), i);
        }
    }

    private void AddPrinterAction(string filePath)
    {
        foreach (var printAction in IDWPrintModel.GetIDWPrintModels(filePath,nbPDFDXFPropertyChanged))
        {
            IDWPrintModels.Add(printAction);
        }
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private async void GetThumbNailAsync(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is IDWPrintModel IDWPrintModelContext)
        {
            if (IDWPrintModelContext.PageNumber >1)
            {
                return;
            }
            if (TeachingTipThumbNail.IsOpen == true && ThumbNailPartNumber.Text == IDWPrintModelContext.FileInfoData.Name)
            {
                TeachingTipThumbNail.IsOpen = false;
                return;
            }
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(IDWPrintModelContext.FileInfoData.FullName);
            var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(iconThumbnail);
            if (bitmapImage != null)
            {
                var sheetSizeConverter = new SheetSizeEnumToStringConverter();
                var sheetOrientationConverter = new OrientationTypeToStringConverter();
                IDWPrintModelContext.bitmapImage = bitmapImage;
                ImageThumbNail.Source = bitmapImage;
                ThumbNailPartNumber.Text = IDWPrintModelContext.FileInfoData.Name;
                ThumbNailDescription.Text = sheetSizeConverter.Convert(IDWPrintModelContext.SheetSize, typeof(string), null, "").ToString();
                ThumbNailCustomer.Text = sheetOrientationConverter.Convert(IDWPrintModelContext.Orientation, typeof(string), null, "").ToString();
                TeachingTipThumbNail.IsOpen = true;
            }
        }
    }

    private void Button_Click_OpenDrawing(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is IDWPrintModel contextIDWModel)
        {
            InventorManagerHelper.GetActualInventorApp()?.Documents.Open(contextIDWModel.FileInfoData.FullName);
            if (InventorManagerHelper.GetActualInventorApp()?.ActiveDocument is Inventor.DrawingDocument drawing)
            {
                var sheetToOpen = drawing.Sheets[contextIDWModel.PageNumber];
                sheetToOpen.Activate(); 
            }
        }
    }
    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        var contextIDWModel = ((FrameworkElement)sender).DataContext as IDWPrintModel;
        IDWPrintModels.Remove(contextIDWModel);
    }

    private async void Button_Click_Print(object sender, RoutedEventArgs e)
    {

        if (IDWPrintModels.Count == 0 || NbDrawing == 0)// si pas de plan on ne fait rien;
        {
            OpenSimpleMessage("Pas d'impressions selectionn�es");
            return;
        }
        if ((NbA4Drawing > 0 || NbA3Drawing > 0) && GetSelectedPrinterA4A3 == null)
        {
            OpenSimpleMessage("Selectioner une imprimante pour la A4 et A3");
            return;
        }
        if ((NbA2Drawing > 0 || NbA1Drawing > 0 || NbA0Drawing > 0) && GetSelectedPrinterA2A1A0 == null)
        {
            OpenSimpleMessage("Selectioner une imprimante pour la A2 et A1 et A0");
            return;
        }
        if (IsInterfaceEnabled == false) return;
        IsInterfaceEnabled = false;
        await inventorManager.PrintList(new List<IDWPrintModel>(IDWPrintModels));
        IsInterfaceEnabled = true;
    }

    private async void OpenSimpleMessage(string Message)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = Message,
            PrimaryButtonText = "Ok",
            DefaultButton = ContentDialogButton.Primary,
        };
        _ = await dialog.ShowAsync();
    }

    private void Button_Click_ClearAllList(object sender, RoutedEventArgs e) => IDWPrintModels.Clear();

    private void CheckBox_Checked_PDFChange(object sender, RoutedEventArgs e) => OnPropertyChanged(nameof(NbA4Drawing));
    private void CheckBox_Checked_DXFChange(object sender, RoutedEventArgs e) => OnPropertyChanged(nameof(NbA3Drawing));

    private void nbPDFDXFPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(NbDrawing));
        OnPropertyChanged(nameof(NbA4Drawing));
        OnPropertyChanged(nameof(NbA3Drawing));
        OnPropertyChanged(nameof(NbA2Drawing));
        OnPropertyChanged(nameof(NbA1Drawing));
        OnPropertyChanged(nameof(NbA0Drawing));
    }

    private void MenuFlyoutItem_Click_AllPrint(object sender, RoutedEventArgs e)
        => IDWPrintModels.ToList().ForEach(x => { x.IsPrint = true; });
    private void MenuFlyoutItem_Click_NonePrint(object sender, RoutedEventArgs e)
        => IDWPrintModels.ToList().ForEach(x => { x.IsPrint = false; });
    private void MenuFlyoutItem_Click_AutoPrintA4A3(object sender, RoutedEventArgs e)
    {
        foreach (var x in IDWPrintModels)
        {
            x.IsPrint = false;
            if (x.Name.EndsWith("L.idw")) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA2DrawingSheetSize) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA1DrawingSheetSize) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA0DrawingSheetSize) continue;
            x.IsPrint = true;
        }
    }
    private void MenuFlyoutItem_Click_AutoPrintA2A1A0(object sender, RoutedEventArgs e)
    {
        foreach (var x in IDWPrintModels)
        {
            x.IsPrint = false;
            if (x.Name.EndsWith("L.idw")) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA3DrawingSheetSize) continue;
            if (x.SheetSize == Inventor.DrawingSheetSizeEnum.kA4DrawingSheetSize) continue;
            x.IsPrint = true;
        }
    }

    private void VisualButtonNonPossible_Click(object sender, RoutedEventArgs e)
    {
        TeachingTipThumbNail.IsOpen = false;
    }
}
