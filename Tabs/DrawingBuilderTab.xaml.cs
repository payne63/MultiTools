using AvitechTools.Models;
using CommunityToolkit.WinUI.UI;
using DocumentFormat.OpenXml.Wordprocessing;
using Inventor;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SplittableDataGridSAmple.Base;
using SplittableDataGridSAmple.Elements;
using SplittableDataGridSAmple.Helper;
using SplittableDataGridSAmple.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs;

public sealed partial class DrawingBuilderTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
{
    #region PropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion

    private StorageFile gabaritFile = null;

    private bool _RingInProgress;

    public bool RingInProgress
    {
        get => _RingInProgress;
        set
        {
            _RingInProgress = value; OnPropertyChanged(); OnPropertyChanged(nameof(InventorHelperReady));
        }
    }

    public bool InventorHelperReady => !RingInProgress;

    private bool _IsInterfaceEnabled = true;
    private InventorHelper InventorHelper;

    public ObservableCollection<DataIQT> LaserCollection = new();

    public bool IsInterfaceEnabled
    {
        get => _IsInterfaceEnabled;
        set
        {
            _IsInterfaceEnabled = value ; OnPropertyChanged();
        }
    }

    public async void InitTab()
    {
        RingInProgress = true;
        ProgressRingLabel.Text = "Chargement d'Inventor";
        InventorHelper.Ready += () => { RingInProgress = false; ProgressRingLabel.Text = "Inventor Prêt"; };
        InventorHelper = await InventorHelper.CreateAsync();
        CloseRequested += (sender, args) =>
        {
            if (InventorHelper != null)
            {
                InventorHelper.App.Quit();
                InventorHelper = null;
            }
        };
    }

    public DrawingBuilderTab()
    {
        this.InitializeComponent();
    }

    private void PanelDataI_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private async void PanelDataI_Drop(object sender, DragEventArgs e)
    {

        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            OpenSimpleMessage("Format non compatible");
            return;
        }
        var items = await e.DataView.GetStorageItemsAsync();

        if (items.Count > 0)
        {
            ClearLaserData();
            foreach (var file in items)
            {
                if (file.Name.EndsWith(".ipt") || file.Name.EndsWith(".iam"))
                {
                    foreach (var dataIQT in GetLaserDatas(file.Path))
                    {
                        if (dataIQT.IsTrueSheetMetal || dataIQT.IsLaserType)
                        {
                            dataIQT.Status = "en attente";
                            LaserCollection.Add(dataIQT);
                        }
                    }
                }
                else
                {
                    OpenSimpleMessage("seul des pièces ou des assemblages sont utilisable");
                }
            }
        }


    }

    public IEnumerable<DataIQT> GetLaserDatas(string firstPathFullName)
    {
        var Lasers = new List<DataIQT>();
        RecursiveLaserDatas(Lasers, firstPathFullName, 1);
        var dic = new Dictionary<string, DataIQT>();
        foreach (DataIQT data in Lasers)
        {
            if (dic.ContainsKey(data.FullPathName))
            {
                dic[data.FullPathName].Qt += data.Qt;
                continue;
            }
            dic.Add(data.FullPathName, data);
        }
        return dic.Select(x => x.Value);
    }

    private void RecursiveLaserDatas(List<DataIQT> lasers, string PathFullName, int qt)
    {
        DataIQT data = null;
        try
        {
            data = new DataIQT(PathFullName, qt);
        }
        catch (Exception ex)
        {
            OpenSimpleMessage($"Erreur!!{ex.Message} \n fichier { PathFullName}");
            return ;
        }
        lasers.Add(data);
        if (data.bom.Count == 0) return;
        foreach (var bomElement in data.bom)
        {
            RecursiveLaserDatas(lasers, bomElement.fullFileName, bomElement.Qt * qt);
        }
    }

    private void Button_Click_RemoveData(object sender, RoutedEventArgs e) => ClearLaserData();

    private async void Button_Click_BuildDrawing(object sender, RoutedEventArgs e)
    {
        if (LaserCollection.Count == 0) return;
        if (gabaritFile ==  null)
        {
            OpenSimpleMessage("Selectionner en premier le plan de gabarit");
            return;
        }
        if (!IsInterfaceEnabled) return;
        IsInterfaceEnabled = false;
        InventorHelper.ShowApp();
        foreach (var dataIQT in LaserCollection)
        {
            if (!dataIQT.IsTrueSheetMetal)
            {
                dataIQT.Status = "A faire Manuellement";
                continue;
            }
            dataIQT.Status = "en Cours";
            GetProgressRingStatus(dataIQT).IsActive = true;
            DrawingDocument drawingDocument = null;
            await Task.Run(() => drawingDocument = DXFBuilderHelper.Build(InventorHelper, dataIQT.FullPathName,gabaritFile.Path));
            var drawingSavePath = dataIQT.FileInfoData.Directory.FullName + @"\Auto DXF\" + 
                    System.IO.Path.GetFileNameWithoutExtension(dataIQT.FileInfoData.Name) + ".idw";
            ContentDialog dialogValidation = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Validation",
                Content = $"le plan est-il correct ?\nsi OUI, il sera sauvegardé\n{drawingSavePath}",
                PrimaryButtonText = "OUI",
                SecondaryButtonText = "NON",
                DefaultButton = ContentDialogButton.Primary,
            };
            var dialogResult = await dialogValidation.ShowAsync();
            if (dialogResult == ContentDialogResult.Primary)
            {
                if (System.IO.File.Exists(drawingSavePath))
                {
                    System.IO.File.Delete(drawingSavePath);
                }
                drawingDocument.SaveAs(drawingSavePath, false);
                InventorHelper.SaveDXF(drawingDocument, System.IO.Path.GetDirectoryName(drawingSavePath));
                dataIQT.Status = "Fait";
            }
            else if (dialogResult == ContentDialogResult.Secondary)
            {
                dataIQT.Status = "non sauvegardé";
            }
            GetProgressRingStatus(dataIQT).IsActive = false;
            drawingDocument.Close(true);
        }
        InventorHelper.HideApp();
        IsInterfaceEnabled = true;

    }

    private ProgressRing GetProgressRingStatus(DataIQT dataIQT)
    {
        var container = ListViewLaser.ContainerFromItem(dataIQT) as ListViewItem;
        return container.FindChild<ProgressRing>();
    }

    private void ClearLaserData()
    {
        LaserCollection.Clear();
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        PanelDataI_Drop(sender, e);
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIDWModel = ((FrameworkElement)sender).DataContext as DataIQT;
        LaserCollection.Remove(contextIDWModel);
    }

    private async void GetThumbNailAsync(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is DataIQT DataIQTContext)
        {
            if (TeachingTipThumbNail.IsOpen == true && ThumbNailPartNumber.Text == DataIQTContext.FileInfoData.Name)
            {
                TeachingTipThumbNail.IsOpen = false;
                return;
            }
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(DataIQTContext.FileInfoData.FullName);
            var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(iconThumbnail);
            if (bitmapImage != null)
            {
                DataIQTContext.bitmapImage = bitmapImage;
                ImageThumbNail.Source = bitmapImage;
                ThumbNailPartNumber.Text = DataIQTContext.FileInfoData.Name;
                ThumbNailDescription.Text = string.Empty;
                ThumbNailCustomer.Text = string.Empty;
                TeachingTipThumbNail.IsOpen = true;
            }
        }
    }

    private async void OpenSimpleMessage(string Message, string content = null)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = Message,
            Content = content,
            PrimaryButtonText = "Ok",
            DefaultButton = ContentDialogButton.Primary,
        };
        _ = await dialog.ShowAsync();
    }



    private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
    {
        (sender as ToggleSwitch).IsOn = false;
    }

    private async void PickAFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
        var window = App.m_window;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.FileTypeFilter.Add(".idw");

        // Open the picker for the user to pick a file
        gabaritFile = await openPicker.PickSingleFileAsync();
        if (gabaritFile != null)
        {
            OutputTextBlock.Text = "Selection: "+gabaritFile.Name;
        }
        else
        {
            OutputTextBlock.Text = "";
            gabaritFile = null;
        }
    }
}
