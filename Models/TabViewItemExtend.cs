using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MultiTools.Interfaces;

namespace MultiTools.Models;

public abstract class TabViewItemExtend : TabViewItem
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private bool _isInterfaceEnabled = true;
    public bool IsInterfaceEnabled
    {
        get => _isInterfaceEnabled;
        protected set
        {
            _isInterfaceEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof( IsInterfaceDisabled));
        }
    }
    public bool IsInterfaceDisabled => !IsInterfaceEnabled;

    protected ProgressRing GetProgressRingStatus2(ListView listView, object model)
    {
        var container = listView.ContainerFromItem(model) as ListViewItem;
        return container.FindChild < ProgressRing>();
    }
    

    protected async void OpenSimpleMessage(XamlRoot xamlRoot, string message)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = message,
            PrimaryButtonText = "Ok",
            DefaultButton = ContentDialogButton.Primary,
        };
        _ = await dialog.ShowAsync();
    }


    protected async Task<StorageFile?> GetFileOpenPicker(params string[] filters)
    {
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
        var window = App.m_window;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        if (filters.Length == 0) filters = new[] { "*" };

        foreach (var filter in filters)
        {
            openPicker.FileTypeFilter.Add(filter);
        }

        // Open the picker for the user to pick a file
        return await openPicker.PickSingleFileAsync();
    }

    protected async Task<IReadOnlyList<StorageFile?>> GetFilesOpenPicker(params string[] filters)
    {
        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
        var window = App.m_window;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        openPicker.ViewMode = PickerViewMode.Thumbnail;

        foreach (var filter in filters)
        {
            openPicker.FileTypeFilter.Add(filter);
        }

        if (filters.Length == 0) filters = new[] { "*" };
        return await openPicker.PickMultipleFilesAsync();
    }

    protected async Task<StorageFolder?> GetFolderOpenPicker()
    {
        var folderPicker = new Windows.Storage.Pickers.FolderPicker();
        var window = App.m_window;
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

        folderPicker.ViewMode = PickerViewMode.Thumbnail;
        folderPicker.FileTypeFilter.Add("*");

        return await folderPicker.PickSingleFolderAsync();
    }
}