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
using SplittableDataGridSAmple.Base;
using Windows.Storage.FileProperties;

namespace SplittableDataGridSAmple.Tabs
{
    public sealed partial class InventorLaserTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
    {
        public ObservableCollection<IDWModel> IDWModels = new();

        InventorManagerHelper inventorManager;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool _IsViewApp;
        public bool IsViewApp
        {
            get { return _IsViewApp; }
            set { _IsViewApp = value; OnPropertyChanged(); }
        }

        private bool _IsZipCompres;
        public bool IsZipCompres
        {
            get { return _IsZipCompres; }
            set { _IsZipCompres = value; OnPropertyChanged(); }
        }

        private double _ProgressBarValue;

        public double ProgressBarValue
        {
            get { return _ProgressBarValue; }
            set { _ProgressBarValue = value; OnPropertyChanged(); }
        }

        private string _ProgressBarStatus = string.Empty;

        public string ProgressBarStatus
        {
            get { return _ProgressBarStatus; }
            set { _ProgressBarStatus = value; OnPropertyChanged(); }
        }


        private bool _IsInderterminateProgressBar;

        public bool IsInderterminateProgressBar
        {
            get { return _IsInderterminateProgressBar; }
            set { _IsInderterminateProgressBar = value; OnPropertyChanged(); }
        }

        private bool _IsInterfaceEnabled = true;

        public bool IsInterfaceEnabled
        {
            get { return _IsInterfaceEnabled; }
            set { _IsInterfaceEnabled = value; OnPropertyChanged(); }
        }


        public int NbDrawing { get => IDWModels.Count(); }
        public int NbPDFDrawing { get => IDWModels.Where(x => x.MakePDF).Count(); }
        public int NbDXFDrawing { get => IDWModels.Where(x=>x.MakeDXF).Count(); }

        public InventorLaserTab()
        {
            this.InitializeComponent();
            IDWModels.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => 
            {
                OnPropertyChanged(nameof(NbDrawing)); 
                OnPropertyChanged(nameof(NbPDFDrawing)); 
                OnPropertyChanged(nameof(NbDXFDrawing)); 
            }; 
        }

        public void InitTab()
        {
            inventorManager = new InventorManagerHelper(this);
        }

        private async void TabViewItem_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var files = items.ToList();
                    foreach (var file in files)
                    {
                        Trace.WriteLine(file.Path);
                        if (!Directory.Exists(file.Path))
                        {
                            if (file.Name.EndsWith(".idw"))
                            {
                                FileInfo fileInfo = new FileInfo(file.Path);
                                IDWModels.Add(new IDWModel(fileInfo, nbPDFDXFPropertyChanged));
                            }
                        }
                        if (Directory.Exists(file.Path)) // si c'est un répertoire
                        {
                            var filesInDirectory = Directory.GetFiles(file.Path);
                            foreach (var f in filesInDirectory)
                            {
                                if (f.EndsWith(".idw"))
                                {
                                    FileInfo fileInfo = new FileInfo(f);
                                    IDWModels.Add(new IDWModel(fileInfo, nbPDFDXFPropertyChanged));
                                }
                            }
                        }
                    }
                }
            }
            var sortableList = new List<IDWModel>(IDWModels);
            sortableList.Sort((IDWModel A, IDWModel B)=>string.Compare(A.Name, B.Name));
            for (int i = 0; i < sortableList.Count; i++)
            {
                IDWModels.Move(IDWModels.IndexOf(sortableList[i]), i);
            }
        }

        private void TabViewItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }
        private async void GetThumbNailAsync(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is IDWModel IDWModelContext)
            {
                if (TeachingTipThumbNail.IsOpen == true && ThumbNailPartNumber.Text == IDWModelContext.FileInfoData.Name)
                {
                    TeachingTipThumbNail.IsOpen = false;
                    return;
                }
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(IDWModelContext.FileInfoData.FullName);
                var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(iconThumbnail);
                if (bitmapImage != null)
                {
                    IDWModelContext.bitmapImage = bitmapImage;
                    ImageThumbNail.Source = bitmapImage;
                    ThumbNailPartNumber.Text = IDWModelContext.FileInfoData.Name;
                    ThumbNailDescription.Text = string.Empty;
                    ThumbNailCustomer.Text = string.Empty;
                    TeachingTipThumbNail.IsOpen = true;
                }
            }
        }

        private void Button_Click_OpenDrawing(object sender, RoutedEventArgs e)
        {
            var contextIDWModel = ((FrameworkElement)sender).DataContext as IDWModel;
            if (contextIDWModel != null)
            {
                InventorManagerHelper.GetActualInventorApp()?.Documents.Open(contextIDWModel.FileInfoData.FullName);
            }
        }
        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            var contextIDWModel = ((FrameworkElement)sender).DataContext as IDWModel;
            IDWModels.Remove(contextIDWModel);
        }

        private async void Button_Click_GeneratePDFDXF(object sender, RoutedEventArgs e)
        {
            if (IDWModels.Count == 0) return; // si pas de plan on ne fait rien;
            if (IsInterfaceEnabled == false) return;
            IsInterfaceEnabled = false;
            await inventorManager.GenerateFile(new List<IDWModel>(IDWModels));
            IsInterfaceEnabled = true;
        }
   

        private void Button_Click_ClearAllList(object sender, RoutedEventArgs e) => IDWModels.Clear();

        private void CheckBox_Checked_PDFChange(object sender, RoutedEventArgs e) => OnPropertyChanged(nameof(NbPDFDrawing));
        private void CheckBox_Checked_DXFChange(object sender, RoutedEventArgs e) => OnPropertyChanged(nameof(NbDXFDrawing));

        private void nbPDFDXFPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(NbPDFDrawing));
            OnPropertyChanged(nameof(NbDXFDrawing));
        }

        private void MenuFlyoutItem_Click_AllPDF(object sender, RoutedEventArgs e) => IDWModels.ToList().ForEach(x => { x.MakePDF = true; });
        private void MenuFlyoutItem_Click_NonePDF(object sender, RoutedEventArgs e) => IDWModels.ToList().ForEach(x => { x.MakePDF = false; });
        private void MenuFlyoutItem_Click_AutoPDF(object sender, RoutedEventArgs e) => IDWModels.ToList().ForEach(x => { x.AutoSelectPDFStatus(); });
        private void MenuFlyoutItem_Click_AllDXF(object sender, RoutedEventArgs e) => IDWModels.ToList().ForEach(x => { x.MakeDXF = true; });
        private void MenuFlyoutItem_Click_NoneDXF(object sender, RoutedEventArgs e) => IDWModels.ToList().ForEach(x => { x.MakeDXF = false; });
        private void MenuFlyoutItem_Click_AutoDXF(object sender, RoutedEventArgs e) => IDWModels.ToList().ForEach(x => { x.AutoSelectDXFStatus(); });
    }
}
