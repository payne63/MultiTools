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
                IsInterfaceEnabled = false;
                //DatasIQT = new ObservableCollection<DataIQT>(await LoadData(storageItemDrop.Path)); // get all files informations
                //DatasIQT = new ObservableCollection<DataIQT>(QtManager.GetQtDatas(storageItemDrop.Path));

                var groups = QtManager.GetQtDatas(storageItemDrop.Path).GroupBy(data => data.Category);
                foreach (var group in groups)
                {
                    var dataGridInstance = new Elements.DataGridQT(group.Key) { Datas = new ObservableCollection<DataIQT>(group), Title = group.Key.ToString() };
                    StackPanelOfBom.Children.Add(dataGridInstance);
                    //dataGridCollection.Add(dataGridInstance);
                }



                IsInterfaceEnabled = true;
            }
        }


        private void Button_Click_RemoveData(object sender, RoutedEventArgs e)
        {
            //DatasIQT.Clear();
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

        private void Button_Click_ExportData(object sender, RoutedEventArgs e)
        {
            List<DataIQT> fulldata = DataGridQT.dataGridCollection.Where(x=>x.IsVisible == true).SelectMany(data => data.Datas).ToList();
            if (fulldata.Count == 0) return;
            //ExcelHelper.ExportData(fulldata);
            EPPlusHelper.ExportData(fulldata);
        }

    }
}
