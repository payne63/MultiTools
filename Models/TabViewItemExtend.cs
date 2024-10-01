using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Controls;

namespace MultiTools.Models;

public abstract class TabViewItemExtend : TabViewItem
{
    
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

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

    public abstract void InitTabAsync();

}