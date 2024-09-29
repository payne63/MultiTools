// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using ABI.Windows.UI;
using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using SplittableDataGridSAmple.Helper;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiTools;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        m_window = new MainWindow();
        m_window.Activate();
        ((MainWindow)m_window)._currentElementTheme = RequestedTheme == ApplicationTheme.Light? ElementTheme.Light:ElementTheme.Dark;
        LoadIcon("Images\\travail-evolution.ico");
        await InventorHelper2.GetInventorAppAsync();
    }
    

    public static Window m_window;

    /// <summary>
    /// Load the icon for the windowj
    /// </summary>
    /// <param name="iconName"></param>
    private void LoadIcon(string iconName)
    {
        //Get the Window's HWND
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
        var hIcon = PInvoke.User32.LoadImage(System.IntPtr.Zero, iconName,
                  PInvoke.User32.ImageType.IMAGE_ICON, 16, 16, PInvoke.User32.LoadImageFlags.LR_LOADFROMFILE);

        PInvoke.User32.SendMessage(hwnd, PInvoke.User32.WindowMessage.WM_SETICON, (System.IntPtr)0, hIcon);
    }
}
