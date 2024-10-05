using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using MultiTools.Base;
using MultiTools.Helper;

namespace MultiTools.Tabs.InventorTab;

public sealed partial class PropertiesRenamerTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
{
    #region PropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion

    public readonly ObservableCollection<DataIProp> SourceFilesCollection = new();
    private bool _isInterfaceEnabled = true;

    public bool IsInterfaceEnabled
    {
        get => _isInterfaceEnabled;
        set
        {
            _isInterfaceEnabled = value;
            OnPropertyChanged();
        }
    }

    public Visibility DragAndDropVisibility =>
        SourceFilesCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public PropertiesRenamerTab()
    {
        this.InitializeComponent();
    }


    public async void InitTabAsync()
    {
        SourceFilesCollection.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(DragAndDropVisibility));
    }

    private async void Button_Click_SelectFiles(object sender, RoutedEventArgs e)
    {
        var file = await GetFileOpenPicker(".ipt", ".iam");
        if (file != null)
        {
            AddItems(file);
        }
    }

    private void AddItems(IStorageItem file)
    {
        if (!file.Name.EndsWith(".ipt") && !file.Name.EndsWith(".iam"))
        {
            _ = OpenSimpleMessage("seul des pièces ou des assemblages Inventor sont utilisable");
        }

        foreach (var dataIProp in GetParts(file.Path))
        {
            dataIProp.Status = DataIProp.StatusEnum.WaitForUpdate;
            SourceFilesCollection.Add(dataIProp);
        }
    }

    private IEnumerable<DataIProp> GetParts(string firstPathFullName)
    {
        var parts = new List<DataIProp>();
        GetPartsRecursive(parts, firstPathFullName);
        var dic = new Dictionary<string, DataIProp>();
        foreach (var data in parts)
        {
            dic.TryAdd(data.FullPathName, data);
        }

        return dic.Select(x => x.Value);
    }

    private void GetPartsRecursive(List<DataIProp> Parts, string PathFullName)
    {
        DataIProp part;
        try
        {
            part = new DataIProp(PathFullName);
        }
        catch (Exception ex)
        {
            _ = OpenSimpleMessage($"Erreur!!{ex.Message} \n fichier {PathFullName}");
            return;
        }

        Parts.Add(part);
        if (part.bom.Count == 0) return;
        foreach (var bomElement in part.bom)
        {
            GetPartsRecursive(Parts, bomElement);
        }
    }

    private void Button_Click_RemoveData(object sender, RoutedEventArgs e) => ClearParts();

    private void ClearParts()
    {
        foreach (var dataIProp in SourceFilesCollection)
        {
            dataIProp.Document.Close();
        }

        SourceFilesCollection.Clear();
    }

    private async void Button_Click_RenameIProperty(object sender, RoutedEventArgs e)
    {
        if (NewAuthorName.Text == string.Empty || NewProjectName.Text == string.Empty ||
            NewCustomerName.Text == string.Empty)
        {
            await OpenSimpleMessage("Veuillez remplir touts les champs avant de renommer");
            return;
        }

        _isInterfaceEnabled = false;
        foreach (var dataIProp in SourceFilesCollection)
        {
            if (dataIProp.Status == DataIProp.StatusEnum.NotUpdateRequired)
            {
                continue;
            }

            dataIProp.Status = DataIProp.StatusEnum.Updating;
            GetProgressRingStatus(dataIProp).IsActive = true;

            dataIProp.CustomerName = NewCustomerName.Text;
            dataIProp.ProjectName = NewProjectName.Text;
            dataIProp.AuthorName = NewAuthorName.Text;
            await Task.Run(() =>
            {
                var inventorFile = InventorHelper2.GetDocument(dataIProp.FullPathName);
                if (inventorFile == null) return;
                
                inventorFile.PropertySets["Design Tracking Properties"].ItemByPropId[9].Value = dataIProp.CustomerName;
                inventorFile.PropertySets["Design Tracking Properties"].ItemByPropId[7].Value = dataIProp.ProjectName;
                inventorFile.PropertySets["Inventor Summary Information"].ItemByPropId[4].Value = dataIProp.AuthorName;

                inventorFile.Save();
                inventorFile.Close();
                // await Task.Delay(200);
            });
            dataIProp.Status = DataIProp.StatusEnum.Updated;
            GetProgressRingStatus(dataIProp).IsActive = false;
        }

        CloseIApprenticeServerDocument();
        _isInterfaceEnabled = true;
    }

    private void CloseIApprenticeServerDocument() => SourceFilesCollection.First().Document.Close();

    private ProgressRing GetProgressRingStatus(DataIProp dataIProp)
    {
        var container = ListViewParts.ContainerFromItem(dataIProp) as ListViewItem;
        return (container ?? throw new Exception("container is null")).FindChild<ProgressRing>();
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e) =>
        e.AcceptedOperation = DataPackageOperation.Move;

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            _ = OpenSimpleMessage("Format non compatible");
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        if (items != null)
        {
            AddItems(items.First());
        }
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIdwModel = ((FrameworkElement)sender).DataContext as DataIProp;
        SourceFilesCollection.Remove(contextIdwModel);
    }

    private async void GetThumbNailAsync(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is DataIBase dataIBaseContext)
        {
            if (TeachingTipThumbNail.IsOpen == true && ThumbNailPartNumber.Text == dataIBaseContext.FileInfoData.Name)
            {
                TeachingTipThumbNail.IsOpen = false;
                return;
            }

            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(dataIBaseContext.FileInfoData.FullName);
            var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(iconThumbnail);
            dataIBaseContext.bitmapImage = bitmapImage;
            ImageThumbNail.Source = bitmapImage;
            ThumbNailPartNumber.Text = dataIBaseContext.FileInfoData.Name;
            ThumbNailDescription.Text = string.Empty;
            ThumbNailCustomer.Text = string.Empty;
            TeachingTipThumbNail.IsOpen = true;
        }
    }

    private async Task OpenSimpleMessage(string message, string content = null)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = message,
            Content = content,
            PrimaryButtonText = "Ok",
            DefaultButton = ContentDialogButton.Primary,
        };
        _ = await dialog.ShowAsync();
    }
    
    private static async Task<StorageFile> GetFileOpenPicker(params string[] filters)
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

    private void _OnTextChanged(object sender, TextChangedEventArgs e)
    {
        foreach (var dataIProp in SourceFilesCollection)
        {
            var asTheGoodProperties = dataIProp.AsTheGoodProperties(NewProjectName.Text, NewCustomerName.Text, NewAuthorName.Text);
            dataIProp.Status = asTheGoodProperties ? DataIProp.StatusEnum.NotUpdateRequired : DataIProp.StatusEnum.WaitForUpdate;
        }
    }
}