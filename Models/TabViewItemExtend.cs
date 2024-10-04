using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MultiTools.Interfaces;

namespace MultiTools.Models;

public abstract class TabViewItemExtend : TabViewItem
{
    public event PropertyChangedEventHandler PropertyChanged;

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
        }
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
}