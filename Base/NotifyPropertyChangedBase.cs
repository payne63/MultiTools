using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MultiTools.Base;

public class NotifyPropertyChangedBase: INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}