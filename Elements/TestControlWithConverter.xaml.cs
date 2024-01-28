// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

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
using System.Diagnostics;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Elements;

public sealed partial class TestControlWithConverter : UserControl ,INotifyPropertyChanged
{
    private string _textBox;
    private bool _VisibilityTextBloc;
    public bool VisibilityTextBloc
    {
        get { return _VisibilityTextBloc; }
        set { _VisibilityTextBloc = value; NotifyPropertyChanged(); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public string TextBox1
    {
        get { return _textBox; }
        set { _textBox = value; NotifyPropertyChanged(); Trace.WriteLine(_textBox); }
    }
    public TestControlWithConverter()
    {
        this.InitializeComponent();
        VisibilityTextBloc = true;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        TextBox1 = "button as been clicked";
        VisibilityTextBloc = !VisibilityTextBloc;
    }
}
public class Converter1 : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
