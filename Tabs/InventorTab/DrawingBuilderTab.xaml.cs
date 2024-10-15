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
using System.Diagnostics;
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

namespace MultiTools.Tabs.InventorTab;

public sealed partial class DrawingBuilderTab : TabViewItemExtend, Interfaces.IInitTab, INotifyPropertyChanged
{
    private StorageFile? _gabaritFile;

    public readonly ObservableCollection<DataIQT> SourceLaserCollection = new();

    public Visibility DragAndDropVisibility => SourceLaserCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    
    public async void InitTabAsync()
    {
        SourceLaserCollection.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(DragAndDropVisibility));
    }

    public DrawingBuilderTab()
    {
        InitializeComponent();
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
                OpenSimpleMessage(XamlRoot, "seul des pièces ou des assemblages sont utilisable");
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
        DataIQT data;
        try
        {
            data = new DataIQT(pathFullName, qt);
        }
        catch (Exception ex)
        {
            OpenSimpleMessage(XamlRoot, $"Erreur!!{ex.Message} \n fichier {pathFullName}");
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
        if (_gabaritFile == null)
        {
            OpenSimpleMessage(XamlRoot, "Selectionner en premier le plan de gabarit");
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
            DrawingDocument? drawingDocument = null;
            (bool status, string message) abordOperation = (false,string.Empty);
            if (dataIqt.IsTrueSheetMetal)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        drawingDocument = InventorHelper2.BuildTrueSheetMetal( dataIqt.FullPathName, _gabaritFile.Path); 
                    }
                    catch (Exception ex)
                    {
                        abordOperation = (true,ex.Message);   
                    }
                });
                if (abordOperation.status == true)
                { 
                    OpenSimpleMessage(XamlRoot, abordOperation.message);
                    dataIqt.Status = abordOperation.message;
                    GetProgressRingStatus(dataIqt).IsActive = false;
                    InventorHelper2.CloseActiveDocument(); //fermeture mise en plan
                    InventorHelper2.CloseActiveDocument(); //fermeture piece
                    continue; 
                }
            }
            else
            {
                await Task.Run(() => drawingDocument = InventorHelper2.BuildNotSheetMetal( dataIqt.FullPathName, _gabaritFile.Path));
            }

            if (drawingDocument == null)
            {
                throw new Exception($"erreur de creation de plan sur le fichier {dataIqt.FullPathName}");
            }
            var drawingSavePath = dataIqt.FileInfoData.Directory?.FullName + @"\Auto DXF\" +
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
                drawingDocument?.SaveAs(drawingSavePath, false);
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
        return container!.FindChild<ProgressRing>()!;
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
            OpenSimpleMessage(XamlRoot, "Format non compatible");
            return;
        }
        var items = await e.DataView.GetStorageItemsAsync();
        AddItemsToCollection(items.ToList());
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIdwModel = ((FrameworkElement)sender).DataContext as DataIQT;
        SourceLaserCollection.Remove(contextIdwModel!);
    }

    private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
    {
        (sender as ToggleSwitch)!.IsOn = false;
    }

    private async void PickAFileButton_Click(object sender, RoutedEventArgs e)
    {
        _gabaritFile = await GetFileOpenPicker(".idw");
        if (_gabaritFile != null)
        {
            OutputTextBlock.Text = "Selection: " + _gabaritFile.Name;
        }
        else
        {
            OutputTextBlock.Text = "";
            _gabaritFile = null;
        }
    }
}
