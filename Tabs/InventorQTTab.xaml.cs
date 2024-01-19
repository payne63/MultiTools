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
                    var group = groups.Where(x => x.Key == enumVal);
                    DataGridQT dataGridQT;
                    if (group.Count() != 0)
                    {
                        dataGridQT = new DataGridQT(enumVal, new(group.First()));
                    }
                    else
                    {
                        dataGridQT = new DataGridQT(enumVal, new() ,false);
                    }
                    StackPanelOfBom.Children.Add(dataGridQT);
                }
                IsInterfaceEnabled = true;
            }
        }


        private void Button_Click_RemoveData(object sender, RoutedEventArgs e) => RemoveAllData();

        private void RemoveAllData()
        {
            DataGridQT.ClearAllData();
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

            List<DataIQT> fulldata = DataGridQT.dataGridCollection.Where(x => x.IsVisible == true).SelectMany(data => data.Datas).ToList();
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

        
    }
}
