using AvitechTools.Models;
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
using Windows.Storage.FileProperties;

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


        private bool _RingInProgress;

        public bool RingInProgress
        {
            get => _RingInProgress;
            set
            {
                _RingInProgress = value; OnPropertyChanged();
            }
        }

        private bool _InventorHelperReady;

        public bool InventorHelperReady
        {
            get => _InventorHelperReady;
            set
            {
                _InventorHelperReady = value; OnPropertyChanged();
            }
        }


        private bool _IsInterfaceEnabled = true;
        private InventorHelper InventorHelper;

        public ObservableCollection<DataIQT> LaserCollection = new();

        public bool IsInterfaceEnabled
        {
            get
            {
                return _IsInterfaceEnabled;
            }
            set
            {
                _IsInterfaceEnabled = value; OnPropertyChanged();
            }
        }

        public async void InitTab()
        {
            InventorHelperReady = false;
            RingInProgress = true;
            InventorHelper.Ready += () => { RingInProgress = false; InventorHelperReady = true; };
            InventorHelper = await InventorHelper.CreateAsync();
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
                //IsInterfaceEnabled = false;


                foreach (var item in GetLaserDatas(storageItemDrop.Path))
                {
                    LaserCollection.Add(item);
                }

                //IsInterfaceEnabled = true;
            }
        }

        public List<DataIQT> GetLaserDatas(string firstPathFullName)
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
            return dic.Select(x => x.Value).Where(x => x.Category == DataIBase.CategoryType.Laser).ToList();
        }

        private void RecursiveLaserDatas(List<DataIQT> lasers, string PathFullName, int qt)
        {
            var data = new DataIQT(PathFullName, qt);
            LaserCollection.Add(data);
            if (data.bom.Count == 0) return;
            foreach (var bomElement in data.bom)
            {
                RecursiveLaserDatas(lasers, bomElement.fullFileName, bomElement.Qt * qt);
            }
        }

        private void Button_Click_RemoveData(object sender, RoutedEventArgs e) => RemoveAllData();

        private async void Button_Click_BuildDrawing(object sender, RoutedEventArgs e)
        {
            while (InventorHelper == null)
            {

            }
            InventorHelper.ShowApp();
            foreach (var laser in LaserCollection)
            {


                var drawingDocument = DXFBuilderHelper.Build(InventorHelper, laser.FullPathName);
                var drawingSavePath = laser.FileInfoData.Directory.FullName + @"\DXF\" + laser.FileInfoData.Name + ".idw";
                ContentDialog dialogValidation = new ContentDialog
                {
                    XamlRoot = XamlRoot,
                    Title = "Validation",
                    Content = $"le plan Generer est-il correct ?\nsi OUI, is sera sauvegarder\n{drawingSavePath}",
                    PrimaryButtonText = "OUI",
                    SecondaryButtonText = "NON",
                    DefaultButton = ContentDialogButton.Primary,
                };
                var dialogResult = await dialogValidation.ShowAsync();
                if (dialogResult == ContentDialogResult.Primary)
                {
                    drawingDocument.SaveAs(drawingSavePath, false);

                }
                drawingDocument.Close();
            }
            InventorHelper.HideApp();

        }
        private void RemoveAllData()
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
    }
}
