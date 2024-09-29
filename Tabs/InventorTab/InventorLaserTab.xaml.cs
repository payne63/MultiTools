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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.IO.Compression;
using System.Net;
using CommunityToolkit.WinUI.UI.Triggers;
using Microsoft.VisualBasic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.UI.Xaml.Media.Imaging;
using MultiTools.Base;
using Windows.Storage.FileProperties;
using Inventor;
using MultiTools.Helper;
using MultiTools.Models;

namespace MultiTools.Tabs.InventorTab;

public sealed partial class InventorLaserTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
{
    public readonly ObservableCollection<IDWModel> IdwModels = new();

    #region PropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion

    private bool _IsZipCompres;

    public bool IsZipCompres
    {
        get
        {
            return _IsZipCompres;
        }
        set
        {
            _IsZipCompres = value;
            OnPropertyChanged();
        }
    }

    private bool _IsInterfaceEnabled = true;

    public bool IsInterfaceEnabled
    {
        get
        {
            return _IsInterfaceEnabled;
        }
        set
        {
            _IsInterfaceEnabled = value;
            OnPropertyChanged();
        }
    }

    public int NbDrawing
    {
        get => IdwModels.Count();
    }

    public int NbPDFDrawing
    {
        get => IdwModels.Where(x => x.MakePDF).Count();
    }

    public int NbDXFDrawing
    {
        get => IdwModels.Where(x => x.MakeDXF).Count();
    }

    public InventorLaserTab()
    {
        this.InitializeComponent();
        IdwModels.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
        {
            OnPropertyChanged(nameof(NbDrawing));
            OnPropertyChanged(nameof(NbPDFDrawing));
            OnPropertyChanged(nameof(NbDXFDrawing));
        };
    }

    public void InitTabAsync()
    {
    }

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            if (items.Count > 0)
            {
                foreach (var file in items)
                {
                    Trace.WriteLine(file.Path);
                    if (!Directory.Exists(file.Path))
                    {
                        if (file.Name.EndsWith(".idw"))
                        {
                            FileInfo fileInfo = new FileInfo(file.Path);
                            IdwModels.Add(new IDWModel(fileInfo, nbPDFDXFPropertyChanged));
                        }
                    }

                    if (!Directory.Exists(file.Path)) continue; // si ce n'est un répertoire
                    var filesInDirectory = Directory.GetFiles(file.Path);
                    foreach (var f in filesInDirectory)
                    {
                        if (f.EndsWith(".idw"))
                        {
                            FileInfo fileInfo = new FileInfo(f);
                            IdwModels.Add(new IDWModel(fileInfo, nbPDFDXFPropertyChanged));
                        }
                    }
                }
            }
        }

        var sortableList = new List<IDWModel>(IdwModels);
        sortableList.Sort((IDWModel A, IDWModel B) => string.Compare(A.Name, B.Name));
        for (int i = 0; i < sortableList.Count; i++)
        {
            IdwModels.Move(IdwModels.IndexOf(sortableList[i]), i);
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
        var contextIdwModel = ((FrameworkElement)sender).DataContext as IDWModel;
        if (contextIdwModel != null)
        {
            InventorHelper2.GetDocument(contextIdwModel.FileInfoData.FullName);
            InventorHelper2.ShowApp();
        }
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        var contextIDWModel = ((FrameworkElement)sender).DataContext as IDWModel;
        IdwModels.Remove(contextIDWModel);
    }

    private async void Button_Click_GeneratePdfDxf(object sender, RoutedEventArgs e)
    {
        if (IdwModels.Count == 0) return; // si pas de plan on ne fait rien;
        if (IsInterfaceEnabled == false) return;
        IsInterfaceEnabled = false;
        // await GeneratePdfDxfAsync(new List<IDWModel>(IdwModels));
        await GeneratePdfDxfAsync(IdwModels.Where(m => m.MakePDF || m.MakeDXF).ToList());
        IsInterfaceEnabled = true;
    }

    private async Task GeneratePdfDxfAsync(List<IDWModel> IdwModels)
    {
        var rootPathOfFile = IdwModels.First()?.FileInfoData?.Directory?.FullName;
        if (rootPathOfFile == null) return;

        string PDFFolder = rootPathOfFile + @"\PDF";
        if (IdwModels.Exists(x => x.MakePDF) && !Directory.Exists(PDFFolder)) Directory.CreateDirectory(PDFFolder);

        string DXFFolder = rootPathOfFile + @"\DXF";
        if (IdwModels.Exists(x => x.MakeDXF) && !Directory.Exists(DXFFolder)) Directory.CreateDirectory(DXFFolder);

        foreach (IDWModel plan in IdwModels)
        {
            var drawingDoc = InventorHelper2.GetDocument(plan.FileInfoData.FullName) as DrawingDocument;

            if (drawingDoc == null) continue;
            if (plan.MakePDF) await Task.Run(() => InventorHelper2.SavePdf(drawingDoc, PDFFolder));
            if (plan.MakeDXF) await Task.Run(() => InventorHelper2.SaveDxf(drawingDoc, DXFFolder));

            drawingDoc.Close();
        }

        if (IsZipCompres) await GenerateZip(PDFFolder, DXFFolder);
    }

    private static async Task GenerateZip(string PDFFolder, string DXFFolder)
    {
        if (Directory.Exists(PDFFolder))
        {
            if (Directory.GetFiles(PDFFolder).Length != 0)
            {
                if (System.IO.File.Exists(PDFFolder + ".zip")) System.IO.File.Delete(PDFFolder + ".zip");
                await Task.Run(() => ZipFile.CreateFromDirectory(PDFFolder, PDFFolder + ".zip"));
            }
        }

        if (Directory.Exists(DXFFolder))
        {
            if (Directory.GetFiles(DXFFolder).Length != 0)
            {
                if (System.IO.File.Exists(DXFFolder + ".zip")) System.IO.File.Delete(DXFFolder + ".zip");
                await Task.Run(() => ZipFile.CreateFromDirectory(DXFFolder, DXFFolder + ".zip"));
            }
        }
    }

    private void Button_Click_ClearAllList(object sender, RoutedEventArgs e) => IdwModels.Clear();

    private void CheckBox_Checked_PDFChange(object sender, RoutedEventArgs e) =>
        OnPropertyChanged(nameof(NbPDFDrawing));

    private void CheckBox_Checked_DXFChange(object sender, RoutedEventArgs e) =>
        OnPropertyChanged(nameof(NbDXFDrawing));

    private void nbPDFDXFPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(NbPDFDrawing));
        OnPropertyChanged(nameof(NbDXFDrawing));
    }

    private void MenuFlyoutItem_Click_AllPDF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.MakePDF = true;
    });

    private void MenuFlyoutItem_Click_NonePDF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.MakePDF = false;
    });

    private void MenuFlyoutItem_Click_AutoPDF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.AutoSelectPDFStatus();
    });

    private void MenuFlyoutItem_Click_AllDXF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.MakeDXF = true;
    });

    private void MenuFlyoutItem_Click_NoneDXF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.MakeDXF = false;
    });

    private void MenuFlyoutItem_Click_AutoDXF(object sender, RoutedEventArgs e) => IdwModels.ToList().ForEach(x =>
    {
        x.AutoSelectDXFStatus();
    });
}