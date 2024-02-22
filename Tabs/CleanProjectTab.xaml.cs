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
using SplittableDataGridSAmple.Base;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using CommunityToolkit.WinUI.UI.Controls;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using DocumentFormat.OpenXml.Spreadsheet;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs;

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

    public ObservableCollection<DataIQT> OrphansPart = new();

    public CleanProjectTab()
    {
        this.InitializeComponent();
        OrphansPart.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => { OnPropertyChanged( nameof( DragAndDropVisibility)); };
    }


    public void InitTab()
    {
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


    private void DoTheJob()
    {
        IsInterfaceEnabled = false;
        var listOfPartAndAssembly = PartsAndAssemblyFindInRoot();
        List<DataIQT> groups;
        try
        {
            groups = QtManager.GetQtDatas(MainAssemblyFile.Path);
        }
        catch (System.Exception ex)
        {
            IsInterfaceEnabled = true;
            OpenSimpleMessage($"Erreur!!{ex.Message} ");
            return;
        }

        foreach (var file in listOfPartAndAssembly)
        {
            if (groups.All(x => x.FullPathName != file))
            {
                OrphansPart.Add(new DataIQT(file,0));
            }
        }
        IsInterfaceEnabled = true;

    }

    public IEnumerable<string> PartsAndAssemblyFindInRoot()
    {
        if (MainAssemblyFile == null) return null;
        return Directory.GetFiles(Path.GetDirectoryName(MainAssemblyFile.Path), "*.*", System.IO.SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".ipt") || path.EndsWith(".iam"))
            .Where(path => !path.Contains("OldVersions")); // remove files from oldVersions folder
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

        DoTheJob();
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
        MainAssemblyFile = file;
        DoTheJob();
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIDWModel = ((FrameworkElement)sender).DataContext as DataIQT;
        OrphansPart.Remove(contextIDWModel);
    }

    private void Button_Click_Clean(object sender, RoutedEventArgs e)
    {
        if (OrphansPart.Count == 0) return;
        var oldPartDirectory = Path.GetDirectoryName(MainAssemblyFile.Path) + @"\OLD\";
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
