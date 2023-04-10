// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
using muxc = Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SplittableDataGridSAmple.Base;
using WindowsFormsTest;
using System.Diagnostics;
using CommunityToolkit.WinUI.Helpers;
using Windows.Graphics.Printing;
using SplittableDataGridSAmple.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using I = Inventor;
using Windows.ApplicationModel.DataTransfer;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using System.Text;
using Windows.Storage.FileProperties;
using AvitechTools.Models;
using SplittableDataGridSAmple.Models;
using Inventor;
using Microsoft.VisualBasic.FileIO;
using RtfPipe.Tokens;
using System.Threading;

namespace SplittableDataGridSAmple.Tabs
{
    public sealed partial class ProjectExplorerTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
    {
        public ObservableCollection<DataI> DatasI { get; private set; } = new();

        private FileSystemWatcher Watcher;

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        public ProjectExplorerTab()
        {
            this.InitializeComponent();
        }

        private void ProjectExplorerElements_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.P)
            {
                Trace.WriteLine("print");

                // Define Header, Footer, and Page Numbering.
                //service.Header = new TextBlock() { Text = "Header", Margin = new Thickness(0, 0, 0, 20) };
                //service.Footer = new TextBlock() { Text = "Footer", Margin = new Thickness(0, 20, 0, 0) };
                //service.PageNumbering = PageNumbering.TopRight;

                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
                //renderTargetBitmap.RenderAsync(treeViewPanel);
                //renderTargetBitmap.RenderAsync(treeViewPanel, (int)treeViewPanel.Width, (int) Height);
                DetailStackPanel.Children.Add(new Image { Source = renderTargetBitmap });


                var document = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(20));

                        page.Header()
                            .Text("Hello PDF!")
                            .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(x =>
                            {
                                x.Spacing(20);
                                x.Item().Text(Placeholders.LoremIpsum());
                                x.Item().Image(Placeholders.Image(200, 100));
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ");
                                x.CurrentPageNumber();
                            });
                    });
                });
                document.GeneratePdf(@"E:/popo.pdf");
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(@"E:/popo.pdf")
                    {
                        UseShellExecute = true
                    }
                };

                process.Start();
            }
        }

        public void UpdateSelectionElement(Data data)
        {
            DetailStackPanel.Children.Clear();
            DetailStackPanel.Children.Add(new TextBlock { Text = data.Code });
            var s = new SymbolIcon();
        }
        private bool _IsInterfaceEnabled = true;
        public bool IsInterfaceEnabled
        {
            get { return _IsInterfaceEnabled; }
            set { _IsInterfaceEnabled = value; OnPropertyChanged(); }
        }

        private bool _IsAutoUpdate = true;
        public bool IsAutoUpdate
        {
            get { return _IsAutoUpdate; }
            set
            {
                _IsAutoUpdate = value;
                OnPropertyChanged();
            }
        }


        public void InitTab()
        {
            DataI.instanceProjectExplorer = this;
            KeyDown += ProjectExplorerElements_KeyDown;
        }

        private int _DeepMax;
        public int DeepMax
        {
            get { return _DeepMax; }
            set { _DeepMax = value; OnPropertyChanged(); }
        }

        private IEnumerable<DataI> _DrawingsFindInRoot;

        public IEnumerable<DataI> DrawingsFindInRoot
        {
            get
            {
                if (DatasI == null) return null;
                if (_DrawingsFindInRoot == null)
                {
                    var folderPathRoot = System.IO.Path.GetDirectoryName(DatasI.First().FullPathName);
                    _DrawingsFindInRoot = Directory.GetFiles(folderPathRoot, "*.idw", System.IO.SearchOption.AllDirectories)
                        .ToList()
                        .Where(pathDrawing => !pathDrawing.Contains("OldVersions")) // remove drawing from oldVersions folder
                        .Select(pathDrawing => new DataI(pathDrawing, null, DataI.RecursiveType.OneTime));
                }
                return _DrawingsFindInRoot;
            }
            set { _DrawingsFindInRoot = value; }
        }


        public List<string> GetListValidationManager()
        {
            var list = ValidationManager.Instance.ValidationItems.Select(x => x.Description).ToList();
            list.Add("Aucun Filtre");
            return list;
        }

        public List<string> GetListCategory()
        {
            var list = Enum.GetNames(typeof(DataI.CategoryType)).ToList();
            list.Add("Aucun Filtre");
            return list;
        }

        private async void treeViewPanelDataI_Drop(object sender, DragEventArgs e)
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
                InitWatcher(System.IO.Path.GetDirectoryName(storageItemDrop.Path));
                await LoadData(storageItemDrop.Path);
                IsInterfaceEnabled = true;
            }
        }

        private void InitWatcher(string folderPathToWatch)
        {
            Watcher = new FileSystemWatcher(folderPathToWatch)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite,
            };
            Watcher.Changed += Watcher_Changed;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {

            Trace.WriteLine(e.FullPath);
            Trace.WriteLine(e.Name);
            Trace.WriteLine(e.ChangeType);
            Trace.WriteLine(DateTime.Now.Millisecond.ToString());
            if (!_IsAutoUpdate) return;
            if (e.FullPath.Contains("OldVersions")) return;
            if (e.Name.Contains("newVer")) return; // TRICK INVENTOR ON SAVE FILE
            var extensionOfChangeFile = System.IO.Path.GetExtension(e.FullPath);
            if (extensionOfChangeFile == ".ipt" || extensionOfChangeFile == ".iam" || extensionOfChangeFile == ".idw")
                DispatcherQueue.TryEnqueue(delegate ()
                {
                    if(!DataI.linkFullPathToData.ContainsKey(e.FullPath)) return;
                    Task.Delay(100).Wait();
                    UpdateLocalDocument(DataI.linkFullPathToData[e.FullPath]);
                });
        }

        private Task LoadData(string PrimaryFullPath)
        {
            DataI.linkFullPathToData.Clear();
            DatasI.FirstOrDefault()?.RecursiveClearData();
            DatasI.Clear();
            DeepMax = 0;

            DatasI.Add(new DataI(PrimaryFullPath, null));
            CheckLinkDraw();
            RecursiveRemoveSpecificChildren(DatasI.First()); // remove children of part and children inside composants folder
            return Task.CompletedTask;
        }

        private void CheckLinkDraw()
        {
            foreach (var dataIDrawing in DrawingsFindInRoot)
            {
                _ = RecursiveCheckLinkDraw(DatasI[0], dataIDrawing);
            }
        }


        private bool RecursiveCheckLinkDraw(DataI dataISource, DataI linkDraw)
        {
            if (linkDraw == null || dataISource == null) throw new ArgumentNullException("gros bug");
            foreach (var linkPart in linkDraw.ReferencedDataI)
            {
                if (dataISource.FullPathName == linkPart.FullPathName)
                {
                    dataISource.drawingDocuments.Add(new DataI(linkDraw.FullPathName, null, DataI.RecursiveType.False));
                    return true;
                }
            }
            foreach (var child in dataISource.ReferencedDataI)
            {
                RecursiveCheckLinkDraw(child, linkDraw);
            }
            return false;
        }
        private void RecursiveRemoveSpecificChildren(DataI dataISource)
        {
            // les pièces ou assemblages du commerce n'ont pas d'enfant
            if (dataISource.Category == DataI.CategoryType.Commerce) dataISource.ReferencedDataI.Clear();
            // les pièces ou assemblages du commerce n'ont pas d'enfant
            if (dataISource.Category == DataI.CategoryType.Commerce) dataISource.ReferencedDataI.Clear();
            // les pièces n'ont pas d'enfant (pièce mirroire et dérivée)
            if (dataISource.DocumentType == DocumentTypeEnum.kPartDocumentObject) dataISource.ReferencedDataI.Clear();

            dataISource.ReferencedDataI.ToList().ForEach(RecursiveRemoveSpecificChildren);
        }

        private void treeViewPanelDataI_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
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

        private void Button_Click_RemoveData(object sender, RoutedEventArgs e)
        {
            DatasI.Clear();
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            if (DatasI.Count == 0) return;
            RecursiveFilterChanged(DatasI.First());
        }
        private void RecursiveFilterChanged(DataI dataISource)
        {
            var filterPartNumber = FilterByPartNumber.Text;
            var filterDescription = FilterByDescription.Text;
            bool FilterCategory = ComboBoxCategory.SelectedItem == null ? true :
                ((string)ComboBoxCategory.SelectedItem) == "Aucun Filtre" ? true :
                ((string)ComboBoxCategory.SelectedItem) == dataISource.Category.ToString() ? true : false;

            bool FilterError = ComboBoxValidationItem.SelectedItem == null ? true :
                ((string)ComboBoxValidationItem.SelectedItem) == "Aucun Filtre" ? true :
                dataISource.GetErrorsMessage().ToList().Contains((string)ComboBoxValidationItem.SelectedItem) ? true : false;


            if (dataISource.PartNumber.IndexOf(filterPartNumber, StringComparison.OrdinalIgnoreCase) >= 0 &&
                dataISource.Description.IndexOf(filterDescription, StringComparison.OrdinalIgnoreCase) >= 0 &&
                (FilterCategory) &&
                (FilterError))
            {
                dataISource.IsVisibility = Visibility.Visible;
            }
            else
            {
                dataISource.IsVisibility = Visibility.Collapsed;
            }
            Trace.WriteLine("--------------------------------------------");
            foreach (var child in dataISource.ReferencedDataI)
            {
                RecursiveFilterChanged(child);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnFilterChanged(sender, null);
        }

        private void Button_Click_ResetFilter(object sender, RoutedEventArgs e)
        {
            FilterByPartNumber.Text = string.Empty;
            FilterByDescription.Text = string.Empty;
            ComboBoxValidationItem.SelectedItem = null;
            ComboBoxCategory.SelectedItem = null;
        }

        private async void GetThumbNailAsync(object sender, RoutedEventArgs e)
        {
            var dataIContext = ((FrameworkElement)sender).DataContext as DataI;
            if (dataIContext != null)
            {
                if (TeachingTipThumbNail.IsOpen == true && ThumbNailPartNumber.Text == dataIContext.PartNumber)
                {
                    TeachingTipThumbNail.IsOpen = false;
                    return;
                }
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(dataIContext.FullPathName);
                var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
                var bitmapImage = new BitmapImage();
                bitmapImage.SetSource(iconThumbnail);
                if (bitmapImage != null)
                {
                    dataIContext.bitmapImage = bitmapImage;
                    ImageThumbNail.Source = bitmapImage;
                    ThumbNailPartNumber.Text = dataIContext.PartNumber;
                    ThumbNailDescription.Text = dataIContext.Description;
                    ThumbNailCustomer.Text = dataIContext.CostCenter;
                    TeachingTipThumbNail.IsOpen = true;
                }
            }
        }

        private void Button_Click_OpenDocument(object sender, RoutedEventArgs e)
        {
            var contextDataI = ((FrameworkElement)sender).DataContext as DataI;
            if (contextDataI != null)
            {
                InventorManagerHelper.GetActualInventorApp()?.Documents.Open(contextDataI.FullPathName);
            }
        }

        private void Button_Click_UpdateAllData(object sender, RoutedEventArgs e)
        {
            if (DatasI.Count == 0) return;
            var PrimaryFullPath = DatasI.First().FullPathName;
            DatasI.Clear();
            LoadData(PrimaryFullPath);
            OnFilterChanged(null, null);

        }
        private void Button_Click_UpdateLocalDocument(object sender, RoutedEventArgs e)
        {
            var data = ((FrameworkElement)sender).DataContext as DataI;
            if (data == null) return;
            UpdateLocalDocument(data);
        }

        private void UpdateLocalDocument(DataI data)
        {
            if (data.Parent == null)
            {
                Button_Click_UpdateAllData(null,null);
                return;
            }
            var index = data.Parent.ReferencedDataI.IndexOf(data);
            data.Parent.ReferencedDataI.Remove(data);
            DataI.linkFullPathToData.Remove(data.FullPathName);
            data.Parent.ReferencedDataI.Insert(index, new DataI(data.FullPathName, data.Parent, DataI.RecursiveType.True, data.Deep));
            CheckLinkDraw();
            RecursiveRemoveSpecificChildren(DatasI.First());
        }
    }
}
