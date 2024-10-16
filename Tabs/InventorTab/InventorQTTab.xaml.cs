// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using ctWinUI = CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Office.Interop.Outlook;
using System.Globalization;
using CommunityToolkit.WinUI.Helpers;
using Inventor;
using Windows.Graphics.Printing;
using Windows.Storage.Pickers;
using Windows.Storage;
using CommunityToolkit.WinUI.UI;
using System.Windows.Input;
using System.Diagnostics;
using MultiTools.Base;
using MultiTools.Elements;
using MultiTools.Helper;
using MultiTools.Models;

namespace MultiTools.Tabs;

public sealed partial class InventorQTTab : TabViewItemExtend, Interfaces.IInitTab, INotifyPropertyChanged
{
    private DataIQT _masterDataIqt;

    private Visibility _DragAndDropVisibility;

    public Visibility DragAndDropVisibility
    {
        get => _DragAndDropVisibility;
        set
        {
            _DragAndDropVisibility = value;
            OnPropertyChanged(nameof(DragAndDropVisibility));
        }
    }

    public InventorQTTab()
    {
        InitializeComponent();
    }

    public void InitTabAsync()
    {
        DragAndDropVisibility = Visibility.Visible;
    }

    private void PanelDataI_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private async void PanelDataI_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            OpenSimpleMessage(XamlRoot, "Format non compatible");
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        if (items.Count != 1)
        {
            OpenSimpleMessage(XamlRoot, "Déposer 1 seul fichier à la fois");
            return;
        }

        var storageItemDrop = items[0];
        if (storageItemDrop.Name.EndsWith(".idw"))
        {
            OpenSimpleMessage(XamlRoot, "Déposer un assemblage ou une pièce, mais pas de plan");
            return;
        }

        if (storageItemDrop.Path.EndsWith(".iam"))
        {
            CreateBoms(storageItemDrop.Path);
        }
    }

    private async void CreateBoms(string pathAssembly)
    {
        RemoveAllData();
        DragAndDropVisibility = Visibility.Collapsed;
        IsInterfaceEnabled = false;
        _masterDataIqt = new DataIQT(pathAssembly, 1);
        var chidlrens = new List<DataGridQT>();
        
        await Task.Run(() =>
        {
            IEnumerable<IGrouping<DataIBase.CategoryType, DataIQT>> groups;
            try
            {
                groups = QtManager.GetQtDatas(pathAssembly).GroupBy(data => data.Category);
            }
            catch (System.Exception ex)
            {
                IsInterfaceEnabled = true;
                OpenSimpleMessage(XamlRoot, $"Erreur!!{ex.Message} ");
                return;
            }

            foreach (DataIBase.CategoryType enumVal in Enum.GetValues(typeof(DataIBase.CategoryType)))
            {
                var group = groups.Where(x => x.Key == enumVal).FirstOrDefault();
                DispatcherQueue.TryEnqueue(() =>
                {
                    var newDataGridIQT = new DataGridQT(enumVal, group == null ? new() : new(group));
                newDataGridIQT.MoveData += NewDataGridIQT_MoveData;
                newDataGridIQT.Selection += NewDataGridIQT_Selection;
                chidlrens.Add(newDataGridIQT);
                });
                // StackPanelOfBom.Children.Add(newDataGridIQT);
            }
        });
        chidlrens.ForEach(x => StackPanelOfBom.Children.Add(x));
        IsInterfaceEnabled = true;
    }

    private void NewDataGridIQT_Selection(DataIBase.CategoryType FromCategoryType)
    {
        foreach (var dataGridQT in StackPanelOfBom.Children.Cast<DataGridQT>()
                     .Where(x => x.category != FromCategoryType))
        {
            dataGridQT.RemoveSelection();
        }
    }

    private void NewDataGridIQT_MoveData(DataIQT dataIQT, DataIBase.CategoryType fromCategoryType,
        DataIBase.CategoryType toCategoryType)
    {
        var dataGrids = StackPanelOfBom.Children.Cast<DataGridQT>().ToList();
        var dataGridFrom = dataGrids.First(x => x.category == fromCategoryType);
        var dataGridTo = dataGrids.First(x => x.category == toCategoryType);
        dataGridFrom.Datas.Remove(dataIQT);
        dataGridTo.Datas.Add(dataIQT);
        dataIQT.Category = toCategoryType;
    }

    private void Button_Click_RemoveData(object sender, RoutedEventArgs e) => RemoveAllData();

    private void RemoveAllData()
    {
        _masterDataIqt = null;
        StackPanelOfBom.Children.Clear();
    }

    private async void Button_Click_ExportData(object sender, RoutedEventArgs e)
    {
        IsInterfaceEnabled = false;
        var fulldata = StackPanelOfBom.Children.Cast<DataGridQT>().Where(x => x.IsVisible == true)
            .SelectMany(d => d.Datas).ToList();
        if (fulldata.Count == 0) return;

        FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Instance);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);
        savePicker.SuggestedStartLocation =
            PickerLocationId.ComputerFolder; //System.IO.Path.GetDirectoryName(fulldata[0].FullPathName);
        savePicker.FileTypeChoices.Add("Fichier Excel", new List<string>() { ".xlsx" });
        var dateSave = DateTime.Now;
        var masterFileName = System.IO.Path.GetFileNameWithoutExtension(_masterDataIqt.NameFile);
        savePicker.SuggestedFileName = "Extraction de " + masterFileName + " le " +
                                       dateSave.ToString("yy-MM-dd à HH\\hmm") + ".xlsx";
        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file == null) return;

        await Task.Run(() =>
        {
            CloseXMLHelper.ExportData(fulldata, file, dateSave, _masterDataIqt);
        });
        
        IsInterfaceEnabled = true;
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Extraction Terminée",
            Content = $"Emplacement : \n {file.Path}",
            PrimaryButtonText = "Fermer",
            SecondaryButtonText = "Ouvrir l'extraction",
            SecondaryButtonCommand = new OpenExtractionCommmand(),
            SecondaryButtonCommandParameter = file.Path,
            DefaultButton = ContentDialogButton.Primary,
        };
        _ = await dialog.ShowAsync();
    }

    private void ScrollViewer_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private void ScrollViewer_Drop(object sender, DragEventArgs e)
    {
        PanelDataI_Drop(sender, e);
    }

    private class OpenExtractionCommmand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            Process.Start(new ProcessStartInfo { FileName = parameter as string, UseShellExecute = true });
        }
    }

    private async void ButtonBase_OnClick_OpenFile(object sender, RoutedEventArgs e)
    {
        var file = await GetFileOpenPicker(".iam");
        if (file == null)
        {
            return;
        }

        if (file.Name.EndsWith(".iam"))
        {
            CreateBoms(file.Path);
        }
    }
}