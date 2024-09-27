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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using CommunityToolkit.WinUI.UI.Controls;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using DocumentFormat.OpenXml.Spreadsheet;
using MultiTools.Interfaces;
using System.Reflection;
using I=Inventor;
using System.Diagnostics;
using MultiTools.Base;
using MultiTools.Helper;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiTools.Tabs.InventorTab;

public sealed partial class CleanProjectTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
{

    #region PropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion


    private StorageFile MainAssemblyFile = null;

    private HashSet<DataIClean> AllParts = new();

    private ObservableCollection<DataIClean> _orpheansPart =new();
    public ObservableCollection<DataIClean> OrphansPart { get => _orpheansPart; set { _orpheansPart = value; } }

    public CleanProjectTab()
    {
        this.InitializeComponent();
        //OrphansPart.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(DragAndDropVisibility)); };
        //OrphansPart.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => { OnPropertyChanged(nameof(OrphansPart)); };
    }


    public async void InitTabAsync()
    {
        InventorHelper = await InventorHelper.CreateAsync();
    }


    private bool _IsInterfaceEnabled = true;
    public bool IsInterfaceEnabled
    {
        get => _IsInterfaceEnabled;
        set
        {
            _IsInterfaceEnabled = value; OnPropertyChanged();
        }
    }


    private async Task DoTheJob()
    {
        IsInterfaceEnabled = false;
        var listOfPartAndAssembly = PartsAndAssemblyFindInRoot();
        foreach (var item in listOfPartAndAssembly)
        {
            //await Task.Run(() =>
            //{
            OrphansPart.Add(new DataIClean(item, MainAssemblyFile.Path));
            //});

        }
        Trace.WriteLine("fin");
        try
        {
            await getDataIClean();
        }
        catch (System.Exception ex)
        {
            IsInterfaceEnabled = true;
            OpenSimpleMessage($"Erreur!!{ex.Message} ");
            return;
        }

        foreach (var dataIClean in AllParts)
        {
            if (listOfPartAndAssembly.All(x => x != dataIClean.FullPathName))
            {
                OrphansPart.Add(dataIClean);
            }
            //if (groups.All(x => x.FullPathName != file))
            //{
            //    OrphansPart.Add(new DataIQT(file, 0));
            //}
        }
        IsInterfaceEnabled = true;

    }

    private async Task getDataIClean()
    {
        await RecursiveGetDataIClean(MainAssemblyFile.Path);
    }

    private async Task RecursiveGetDataIClean(string path)
    {
        //var eventCompleted = new TaskCompletionSource<bool>();
        //InventorHelper.Ready += () =>
        //{
        //    eventCompleted.SetResult(true);

        //};
        //await eventCompleted.Task; // ne fonctionne pas !!!!
        while (InventorHelper == null)
        {
            await Task.Delay(500);
            Trace.WriteLine("wait");
        }
        var doc = InventorHelper.App.Documents.Open(path);

        if (doc is I.AssemblyDocument assemblyDoc)
        {
            AllParts.Add(new DataIClean(path, MainAssemblyFile.Path));
            foreach (I.ComponentOccurrence occurrence in assemblyDoc.ComponentDefinition.Occurrences)
            {
                I.ComponentDefinition compDef = occurrence.Definition;
                if (compDef is I.PartComponentDefinition partCompDef)
                {
                    AllParts.Add (new DataIClean(((I.PartDocument)(partCompDef.Document)).FullFileName, MainAssemblyFile.Path));
                }
                else if (compDef is I.AssemblyComponentDefinition assemblyComponent)
                {
                    await RecursiveGetDataIClean(((I.AssemblyDocument)(assemblyComponent.Document)).FullFileName);
                }
            }
        }
        doc.Close();
    }

    public List<string> PartsAndAssemblyFindInRoot()
    {
        if (MainAssemblyFile == null) return null;
        return Directory.GetFiles(Path.GetDirectoryName(MainAssemblyFile.Path), "*.*", System.IO.SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".ipt") || path.EndsWith(".iam"))
            .Where(path => !path.Contains("OldVersions"))
            .ToList();
    }


    private async Task<StorageFile> GetFileOpenPicker(params String[] filters)
    {
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
        var window = App.m_window;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        foreach (var filter in filters)
        {
            openPicker.FileTypeFilter.Add(filter);
        }

        // Open the picker for the user to pick a file
        return await openPicker.PickSingleFileAsync();
    }

    public Visibility DragAndDropVisibility => OrphansPart.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    private InventorHelper InventorHelper;

    private void TabViewItem_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            OpenSimpleMessage("Format non compatible");
            return;
        }
        var items = await e.DataView.GetStorageItemsAsync();
        MainAssemblyFile = items.FirstOrDefault() as StorageFile;

        await DoTheJob();
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

    private void Button_Click_RemoveData(object sender, RoutedEventArgs e) => OrphansPart.Clear();

    private async void Button_Click_SelectFiles(object sender, RoutedEventArgs e)
    {
        var file = await GetFileOpenPicker(".ipt", ".iam");
        if (file == null) return;
        MainAssemblyFile = file;
        await DoTheJob();
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIDWModel = ((FrameworkElement)sender).DataContext as DataIClean;
        OrphansPart.Remove(contextIDWModel);
    }

    private void Button_Click_Clean(object sender, RoutedEventArgs e)
    {
        if (OrphansPart.Count == 0) return;
        var oldPartDirectory = Path.GetDirectoryName(MainAssemblyFile.Path) + @"\AutoOLD\";
        if (!Directory.Exists(oldPartDirectory))
        {
            Directory.CreateDirectory(oldPartDirectory);
        }
        foreach (var file in OrphansPart)
        {
            var newPath = oldPartDirectory + file.NameFile;
            File.Move(file.FullPathName, newPath);
        }
        OrphansPart.Clear();
    }

}


