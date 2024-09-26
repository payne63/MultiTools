using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
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
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using SplittableDataGridSAmple.Base;
using SplittableDataGridSAmple.Helper;

namespace SplittableDataGridSAmple.Tabs;

public sealed partial class PropertiesRenamerTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
{
    #region PropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #endregion

    public ObservableCollection<DataIProp> SourceFilesCollection = new();
    private bool _IsInterfaceEnabled = true;

    public bool IsInterfaceEnabled
    {
        get => _IsInterfaceEnabled;
        set
        {
            _IsInterfaceEnabled = value;
            OnPropertyChanged();
        }
    }

    private InventorHelper InventorHelper;
    private bool _RingInProgress;

    public bool RingInProgress
    {
        get => _RingInProgress;
        set
        {
            _RingInProgress = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(InventorHelperReady));
        }
    }

    public bool InventorHelperReady => !RingInProgress;

    public Visibility DragAndDropVisibility =>
        SourceFilesCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public PropertiesRenamerTab()
    {
        this.InitializeComponent();
    }

   

    public async void InitTabAsync()
    {
        SourceFilesCollection.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(DragAndDropVisibility));
        RingInProgress = true;
        ProgressRingLabel.Text = "Chargement d'Inventor";
        InventorHelper.Ready += () =>
        {
            RingInProgress = false;
            ProgressRingLabel.Text = "Inventor Prêt";
        };
        InventorHelper = await InventorHelper.CreateAsync();
        CloseRequested += (sender, args) =>
        {
            if (InventorHelper != null)
            {
                InventorHelper.App?.Quit();
                InventorHelper = null;
            }
        };
    }

    private async void Button_Click_SelectFiles(object sender, RoutedEventArgs e)
    {
        var file = await GetFileOpenPicker(".ipt", ".iam");
        if (file == null) return;
        AddItems(file);
    }

    private void AddItems(IStorageItem file)
    {
        if (!file.Name.EndsWith(".ipt") && !file.Name.EndsWith(".iam"))
            OpenSimpleMessage("seul des pièces ou des assemblages Inventor sont utilisable");

        foreach (var dataIProp in GetParts(file.Path))
        {
            // if (dataIQT.Category != DataIBase.CategoryType.Laser) continue;
            // if (dataIQT.IsTrueSheetMetal || dataIQT.IsLaserType)
            // {
            dataIProp.Status = "en attente";
            SourceFilesCollection.Add(dataIProp);
            // }
        }
    }

    public IEnumerable<DataIProp> GetParts(string firstPathFullName)
    {
        var Parts = new List<DataIProp>();
        GetPartsRecursive(Parts, firstPathFullName);
        var dic = new Dictionary<string, DataIProp>();
        foreach (DataIProp data in Parts)
        {
            if (!dic.ContainsKey(data.FullPathName))
                dic.Add(data.FullPathName, data);
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
            OpenSimpleMessage($"Erreur!!{ex.Message} \n fichier {PathFullName}");
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
        if (NewAuthorName.Text == string.Empty || NewProjectName.Text == string.Empty || NewCustomerName.Text == string.Empty)
        {
            OpenSimpleMessage("Veuillez remplir touts les champs avant de renommer");
            return;
        }
        _IsInterfaceEnabled = false;
        foreach (var dataIProp in SourceFilesCollection)
        {
            GetProgressRingStatus(dataIProp).IsActive = true;
            var isAlreadyGoodIProperties = dataIProp.CustomerName == NewCustomerName.Text && dataIProp.ProjectName == NewProjectName.Text && dataIProp.AuthorName== NewAuthorName.Text;
            if (isAlreadyGoodIProperties)
            {
                dataIProp.Status = "non modifié";
                continue;
            }
            dataIProp.CustomerName = NewCustomerName.Text; 
            dataIProp.ProjectName = NewProjectName.Text;
            dataIProp.AuthorName = NewAuthorName.Text;
            await Task.Run(() =>
            {
                var inventorFile = InventorHelper.App.Documents.Open(dataIProp.FullPathName);
                inventorFile.PropertySets["Design Tracking Properties"].ItemByPropId[9].Value = dataIProp.CustomerName;
                inventorFile.PropertySets["Design Tracking Properties"].ItemByPropId[7].Value = dataIProp.ProjectName;
                inventorFile.PropertySets["Inventor Summary Information"].ItemByPropId[4].Value = dataIProp.AuthorName;

                inventorFile.Save();
                inventorFile.Close();
                // await Task.Delay(200);
            });
            dataIProp.Status = "Renommer";
            GetProgressRingStatus(dataIProp).IsActive = false;
        }
        CloseIApprenticeServerDocument(); 
        _IsInterfaceEnabled = true;
    }

    private void CloseIApprenticeServerDocument() => SourceFilesCollection.First().Document.Close();

    private ProgressRing GetProgressRingStatus(DataIProp dataIProp)
    {
        var container = ListViewParts.ContainerFromItem(dataIProp) as ListViewItem;
        return container.FindChild<ProgressRing>();
    }

    private void TabViewItem_DragOver(object sender, DragEventArgs e) =>
        e.AcceptedOperation = DataPackageOperation.Move;

    private async void TabViewItem_Drop(object sender, DragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            OpenSimpleMessage("Format non compatible");
            return;
        }

        var items = await e.DataView.GetStorageItemsAsync();
        AddItems(items.First());
    }

    private void Button_Click_Remove(object sender, RoutedEventArgs e)
    {
        if (!IsInterfaceEnabled) return;
        var contextIDWModel = ((FrameworkElement)sender).DataContext as DataIProp;
        SourceFilesCollection.Remove(contextIDWModel);
    }

    private async void GetThumbNailAsync(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).DataContext is DataIBase DataIBaseContext)
        {
            if (TeachingTipThumbNail.IsOpen == true && ThumbNailPartNumber.Text == DataIBaseContext.FileInfoData.Name)
            {
                TeachingTipThumbNail.IsOpen = false;
                return;
            }

            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(DataIBaseContext.FileInfoData.FullName);
            var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(iconThumbnail);
            if (bitmapImage != null)
            {
                DataIBaseContext.bitmapImage = bitmapImage;
                ImageThumbNail.Source = bitmapImage;
                ThumbNailPartNumber.Text = DataIBaseContext.FileInfoData.Name;
                ThumbNailDescription.Text = string.Empty;
                ThumbNailCustomer.Text = string.Empty;
                TeachingTipThumbNail.IsOpen = true;
            }
        }
    }

    private async Task OpenSimpleMessage(string Message, string content = null)
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
    
}