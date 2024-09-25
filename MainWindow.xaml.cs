using Microsoft.Office.Interop.Outlook;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Versioning;
using System.Security;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Outlook = Microsoft.Office.Interop.Outlook;
using MsgReader.Outlook;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using System.Reflection;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI;
using System.Text;
using SplittableDataGridSAmple.Helper;
using SplittableDataGridSAmple.Base;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net.Mail;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Application = Microsoft.UI.Xaml.Application;

namespace SplittableDataGridSAmple;

public sealed partial class MainWindow : Window, INotifyPropertyChanged
{
    public static TabView tabViewStaticRef;
    public static MainWindow Instance;
    public static string ContactsDataPath;
    public static string CompanyDataPath;
    public static string UsersDataPath;

    private ObservableCollection<Base.User> _Users = new();
    public ElementTheme _currentElementTheme = ElementTheme.Default;

    public ObservableCollection<Base.User> UsersName
    {
        get => _Users;
        set
        {
            var actualUserName = GetSelectedUser;
            if (_Users != null)
            {
                ComboBoxUsers.SelectedItem = actualUserName;
            }
            _Users = value;
            OnPropertyChanged();
        }
    }

    public User GetSelectedUser => ComboBoxUsers.SelectedItem as User;

    public MainWindow()
    {
        this.InitializeComponent();
        LoadPaths();
        Instance = this;
        tabViewStaticRef = TabViewMain;
        ResizeWindows(1800, 1000);
        UsersNameUpdate();
        //ExtendsContentIntoTitleBar = true;
        window.Title = "MultiTools";
    }

    private static void LoadPaths()
    {
        ContactsDataPath =
            System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "JsonData\\contacts.json");
        CompanyDataPath =
            System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "JsonData\\companys.json");
        UsersDataPath =
            System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "JsonData\\users.json");
    }

    private void ResizeWindows(int width, int height)
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public async void UsersNameUpdate()
    {
        UsersName.Clear();
        var data = await JsonHelper.LoadArray<Base.User>(UsersDataPath);
        foreach (var user in data)
        {
            UsersName.Add(user);
        }

        foreach (var user in ComboBoxUsers.Items)
        {
            Trace.WriteLine(user);
        }

        ComboBoxUsers.UpdateLayout();
    }

    private void TabView_AddTabButtonClick(TabView tabViewSender, object args)
    {
        var tabViewInstance = new Tabs.OpenNewTab();
        ((Interfaces.IInitTab)tabViewInstance).InitTabAsync();
        tabViewStaticRef.TabItems.Add(tabViewInstance);
        tabViewStaticRef.SelectedItem = tabViewInstance;
    }

    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args) =>
        sender.TabItems.Remove(args.Tab);


    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listBox = sender as ListBox;
        if (listBox != null)
        {
            Trace.WriteLine(listBox.SelectedItem);
        }
    }

    private void OnThemeButtonClick(object sender, RoutedEventArgs e)
    {
        _currentElementTheme = _currentElementTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        MainPage.RequestedTheme = _currentElementTheme;
    }


    private void MainWindow_OnClosed(object sender, WindowEventArgs args)
    {
        InventorHelper.CloseAllInstance();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        Console.WriteLine("fermeture du program");
        ;
    }
}