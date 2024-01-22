using AvitechTools.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SplittableDataGridSAmple.Base;
using SplittableDataGridSAmple.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs
{
    public sealed partial class DrawingBuilderTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
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

        public void InitTab()
        {
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
                    //newDataGridIQT.MoveData += NewDataGridIQT_MoveData;
                    //newDataGridIQT.Selection += NewDataGridIQT_Selection;
                    StackPanelOfLasers.Children.Add(newDataGridIQT);
                }

                IsInterfaceEnabled = true;
            }
        }

        private void Button_Click_RemoveData(object sender, RoutedEventArgs e) => throw new Exception();

        private async void Button_Click_BuildDrawing(object sender, RoutedEventArgs e)
        {
            
        }
        private void RemoveAllData()
        {
            StackPanelOfLasers.Children.Clear();
        }

        private void ScrollViewer_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private void ScrollViewer_Drop(object sender, DragEventArgs e)
        {
            PanelDataI_Drop(sender, e);
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
    }
}
