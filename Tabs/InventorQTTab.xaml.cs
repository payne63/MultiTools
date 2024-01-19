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
using SplittableDataGridSAmple.Base;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using ctWinUI = CommunityToolkit.WinUI.UI.Controls;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Office.Interop.Outlook;
using System.Globalization;
using SplittableDataGridSAmple.Helper;
using CommunityToolkit.WinUI.Helpers;
using Inventor;
using Windows.Graphics.Printing;
using SplittableDataGridSAmple.Elements;
using Windows.Storage.Pickers;
using Windows.Storage;
using CommunityToolkit.WinUI.UI;

namespace SplittableDataGridSAmple.Tabs
{
    public sealed partial class InventorQTTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
    {

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private bool _IsInterfaceEnabled = true;
        public bool IsInterfaceEnabled
        {
            get { return _IsInterfaceEnabled; }
            set { _IsInterfaceEnabled = value; OnPropertyChanged(); }
        }

        public InventorQTTab()
        {
            this.InitializeComponent();
        }
        public void InitTab()
        {
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
            if (items.Count != 1)
            {
                OpenSimpleMessage("Déposer 1 seul fichier à la fois");
                return;
            }
            var storageItemDrop = items[0];
            if (storageItemDrop.Name.EndsWith(".idw"))
            {
                OpenSimpleMessage("Déposer un assemblage ou une pièce, mais pas de plan");
                return;
            }
            if (storageItemDrop.Name.EndsWith(".ipt") || storageItemDrop.Name.EndsWith(".iam"))
            {
                RemoveAllData();
                IsInterfaceEnabled = false;

                var groups = QtManager.GetQtDatas(storageItemDrop.Path).GroupBy(data => data.Category);
                foreach (DataIBase.CategoryType enumVal in Enum.GetValues(typeof(DataIBase.CategoryType)))
                {
                    var group = groups.Where(x => x.Key == enumVal).FirstOrDefault();
                    var newDataGridIQT = new DataGridQT(enumVal, group == null ? new() : new(group));
                    newDataGridIQT.MoveData += NewDataGridIQT_MoveData;
                    newDataGridIQT.Selection += NewDataGridIQT_Selection;
                    StackPanelOfBom.Children.Add( newDataGridIQT);
                }
                
                IsInterfaceEnabled = true;
            }
        }

        private void NewDataGridIQT_Selection(DataIBase.CategoryType FromCategoryType)
        {
            foreach (var dataGridQT in StackPanelOfBom.Children.Cast<DataGridQT>().Where(x => x.category != FromCategoryType))
            {
                dataGridQT.RemoveSelection();
            }
        }

        private void NewDataGridIQT_MoveData(DataIQT dataIQT, DataIBase.CategoryType fromCategoryType, DataIBase.CategoryType toCategoryType)
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
            StackPanelOfBom.Children.Clear();
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

        private async void Button_Click_ExportData(object sender, RoutedEventArgs e)
        {
            var fulldata = StackPanelOfBom.Children.Cast<DataGridQT>().Where(x => x.IsVisible == true).SelectMany(d=>d.Datas).ToList();
            if (fulldata.Count == 0) return;

            FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Instance);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hWnd);
            savePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder; //System.IO.Path.GetDirectoryName(fulldata[0].FullPathName);
            savePicker.FileTypeChoices.Add("Fichier Excel", new List<string>() { ".xlsx" });
            var dateSave = DateTime.Now;
            var fileName = System.IO.Path.GetFileNameWithoutExtension(fulldata[0].NameFile);
            savePicker.SuggestedFileName = "Extraction de " + fileName + " le " + dateSave.ToString("yy-MM-dd à HH\\hmm") + ".xlsx";
            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file == null) return;

            CloseXMLHelper.ExportData(fulldata, file,dateSave);
        }

        private void ScrollViewer_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private void ScrollViewer_Drop(object sender, DragEventArgs e)
        {
            PanelDataI_Drop(sender, e);
        }
    }
}
