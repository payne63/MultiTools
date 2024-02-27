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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WelcomeTab : TabViewItem , Interfaces.IInitTab
{
    public WelcomeTab()
    {
        this.InitializeComponent();
    }

    public void InitTabAsync()
    {
    }

    private void Button_Click_OpenNewTab(object sender, RoutedEventArgs e)
    {
        var newTab = new Tabs.OpenNewTab();
        ((Interfaces.IInitTab)newTab).InitTabAsync();
        MainWindow.tabViewRef.TabItems.Add(newTab);
        MainWindow.tabViewRef.SelectedItem = newTab;
    }
}
