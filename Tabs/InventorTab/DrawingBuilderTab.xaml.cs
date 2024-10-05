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
using MultiTools.Elements;
using MultiTools.Models;
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
using MultiTools.Base;
using MultiTools.Helper;
using ContentDialog = Microsoft.UI.Xaml.Controls.ContentDialog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiTools.Tabs.InventorTab;

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

    // private bool _RingInProgress;
    //
    // public bool RingInProgress
    // {
    //     get => _RingInProgress;
    //     set
    //     {
    //         _RingInProgress = value; OnPropertyChanged(); OnPropertyChanged(nameof(InventorHelperReady));
    //     }
    // }
    //
    // public bool InventorHelperReady => !RingInProgress;

    // private InventorHelper _inventorHelper;

    public readonly ObservableCollection<DataIQT> SourceLaserCollection = new();

    public Visibility DragAndDropVisibility => SourceLaserCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    
    private bool _isInterfaceEnabled = true;
    public bool IsInterfaceEnabled
    {
        get => _isInterfaceEnabled;
        private set
        {
            _isInterfaceEnabled = value; OnPropertyChanged();
        }
    }

    public async void InitTabAsync()
    {
        SourceLaserCollection.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(DragAndDropVisibility));
        // RingInProgress = true;
        ProgressRingLabel.Text = "Chargement d'Inventor";
        // InventorHelper.Ready += () => { RingInProgress = false; ProgressRingLabel.Text = "Inventor Prêt"; };
        // _inventorHelper = await InventorHelper.CreateAsync();
        // CloseRequested += (sender, args) =>
        // {
        //     if (_inventorHelper != null)
        //     {
        //         _inventorHelper.App.Quit();
        //         _inventorHelper = null;
        //     }
        // };
    }

    public DrawingBuilderTab()
    {
        this.InitializeComponent();
    }
    private async void Button_Click_SelectFiles(object sender, RoutedEventArgs e)
    {
        var storageFile = await GetFileOpenPicker(".ipt", ".iam");
        if (storageFile == null) return;
        AddItemsToCollection(new() { storageFile });
    }


    private void AddItemsToCollection(List<IStorageItem> items)
    {
        foreach (var file in items)
        {
            if (file.Name.EndsWith(".ipt") || file.Name.EndsWith(".iam"))
            {
                foreach (var dataIQT in GetLaserDatas(file.Path))
                {
                    if (dataIQT.Category != DataIBase.CategoryType.Laser) continue;
                    if (dataIQT.IsTrueSheetMetal || dataIQT.IsLaserType)
                    {
                        dataIQT.Status = "en attente";
                        SourceLaserCollection.Add(dataIQT);
                    }
                }
            }
            else
            {
                OpenSimpleMessage("seul des pièces ou des assemblages sont utilisable");
            }
        }
    }

    public IEnumerable<DataIQT> GetLaserDatas(string firstPathFullName)
    {
        var lasers = new List<DataIQT>();
        RecursiveLaserDatas(lasers, firstPathFullName, 1);
        var dic = new Dictionary<string, DataIQT>();
        foreach (DataIQT data in lasers)
        {
            if (dic.TryGetValue(data.FullPathName, out var value))
            {
                value.Qt += data.Qt;
                continue;
            }
            dic.Add(data.FullPathName, data);
        }
        return dic.Select(x => x.Value);
    }

    private void RecursiveLaserDatas(List<DataIQT> lasers, string pathFullName, int qt)
    {
        DataIQT data = null;
        try
        {
            data = new DataIQT(pathFullName, qt);
        }
        catch (Exception ex)
        {
            _ = OpenSimpleMessage($"Erreur!!{ex.Message} \n fichier {pathFullName}");
            return;
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
        if (SourceLaserCollection.Count == 0) return;
        if (gabaritFile == null)
        {
            _ = OpenSimpleMessage("Selectionner en premier le plan de gabarit");
            return;
        }
        if (!IsInterfaceEnabled) return;
        IsInterfaceEnabled = false;
        InventorHelper2.ShowApp();
        // _inventorHelper.ShowApp();
        foreach (var dataIqt in SourceLaserCollection)
        {
            dataIqt.Status = "en Cours";
            GetProgressRingStatus(dataIqt).IsActive = true;
            DrawingDocument drawingDocument = null;
            (bool status, string message) abordOperation = (false,string.Empty);
            if (dataIqt.IsTrueSheetMetal)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        drawingDocument = InventorHelper2.BuildTrueSheetMetal( dataIqt.FullPathName, gabaritFile.Path); 
                    }
                    catch (Exception ex)
                    {
                        abordOperation = (true,ex.Message);   
                    }
                });
                if (abordOperation.status == true)
                { 
                    await OpenSimpleMessage(abordOperation.message);
                    dataIqt.Status = abordOperation.message;
                    GetProgressRingStatus(dataIqt).IsActive = false;
                    InventorHelper2.CloseActiveDocument();
                    InventorHelper2.CloseActiveDocument();
                    // _inventorHelper.App.ActiveDocument.Close(true);
                    // _inventorHelper.App.ActiveDocument.Close(true);
                    continue; 
                }
            }
            else
            {
                await Task.Run(() => drawingDocument = InventorHelper2.BuildNotSheetMetal( dataIqt.FullPathName, gabaritFile.Path));
            }
            var drawingSavePath = dataIqt.FileInfoData.Directory.FullName + @"\Auto DXF\" +
                    System.IO.Path.GetFileNameWithoutExtension(dataIqt.FileInfoData.Name) + ".idw";
            var dialogValidation = new ContentDialog
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
                InventorHelper2.SaveDxf(drawingDocument, System.IO.Path.GetDirectoryName(drawingSavePath));
                // _inventorHelper.SaveDXF(drawingDocument, System.IO.Path.GetDirectoryName(drawingSavePath));
                dataIqt.Status = "Fait";
            }
            else if (dialogResult == ContentDialogResult.Secondary)
            {
                dataIqt.Status = "non sauvegardé";
            }
            GetProgressRingStatus(dataIqt).IsActive = false;
            drawingDocument.Close(true);
        }
        InventorHelper2.HideApp();
        // _inventorHelper.HideApp();
        IsInterfaceEnabled = true;

    }

    private ProgressRing GetProgressRingStatus(DataIQT dataIQT)
    {
        var container = ListViewLaser.ContainerFromItem(dataIQT) as ListViewItem;
        return container.FindChild<ProgressRing>();
    }

    private void ClearLaserData()
    {
        SourceLaserCollection.Clear();
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            OpenSimpleMessage("Format non compatible");
            return;
        }
        var items = await e.DataView.GetStorageItemsAsync();
        AddItemsToCollection(items.ToList());
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIdwModel = ((FrameworkElement)sender).DataContext as DataIQT;
        SourceLaserCollection.Remove(contextIdwModel);
    }

    private async void GetThumbNailAsync(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is DataIQT dataIqtContext)
        {
            if (TeachingTipThumbNail.IsOpen == true && ThumbNailPartNumber.Text == dataIqtContext.FileInfoData.Name)
            {
                TeachingTipThumbNail.IsOpen = false;
                return;
            }
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(dataIqtContext.FileInfoData.FullName);
            var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(iconThumbnail);
            dataIqtContext.bitmapImage = bitmapImage;
            ImageThumbNail.Source = bitmapImage;
            ThumbNailPartNumber.Text = dataIqtContext.FileInfoData.Name;
            ThumbNailDescription.Text = string.Empty;
            ThumbNailCustomer.Text = string.Empty;
            TeachingTipThumbNail.IsOpen = true;
        }
    }

    private async Task OpenSimpleMessage(string Message, string content = null)
    {
        var dialog = new ContentDialog
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
        gabaritFile = await GetFileOpenPicker(".idw");
        if (gabaritFile != null)
        {
            OutputTextBlock.Text = "Selection: " + gabaritFile.Name;
        }
        else
        {
            OutputTextBlock.Text = "";
            gabaritFile = null;
        }
    }

    private async Task<StorageFile?> GetFileOpenPicker(params String[] filters)
    {
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
        var window = App.m_window;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        foreach (var filter in filters)
        {
            openPicker.FileTypeFilter.Add(filter);
        }

        // Open the picker for the user to pick a file
        return await openPicker.PickSingleFileAsync();
    }

    
}
