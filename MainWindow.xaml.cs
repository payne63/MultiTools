using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MultiTools.Base;
using MultiTools.Elements;
using MultiTools.Helper;
using MultiTools.Tabs;
using MultiTools.Tabs.InventorTab;
using WinUIEx;

namespace MultiTools;

public sealed partial class MainWindow : WindowEx, INotifyPropertyChanged
{
    public static TabView tabViewStaticRef;
    public static MainWindow Instance;
    public static string ContactsDataPath;
    public static string CompanyDataPath;
    public static string UsersDataPath;

    public ElementTheme CurrentElementTheme = ElementTheme.Default;
    
    private CancellationTokenSource ctsVisibilityChangeTask = new();

    public ObservableCollection<NewTabButton> JobElementsInventor
    {
        get;
        set;
    } = new();

    public MainWindow()
    {
        this.InitializeComponent();
        Instance = this;
        tabViewStaticRef = TabViewMain;
        PopulateElements();
        App.AppReady += () =>
        {
            ToggleSwitchInventor.Toggled -= toggleSwitchInventor_Toggled;
            ToggleSwitchInventor.IsOn = true;
            ToggleSwitchInventor.Toggled += toggleSwitchInventor_Toggled;
        };
        ToggleSwitchInventor.Toggled += toggleSwitchInventor_Toggled;

        ToggleSwitchShowInventor.Toggled += (sender, e) =>
        {
            if (ToggleSwitchShowInventor.IsOn)
            {
                InventorHelper2.ShowApp();
            }
            else
            {
                InventorHelper2.HideApp();
            }
        };
        
        var token = ctsVisibilityChangeTask.Token;
        Task.Run(() => VisibilityChangedEvent(token),token);
        
        // UsersNameUpdate();
        //ExtendsContentIntoTitleBar = true;
    }
    
    private void PopulateElements()
    {
        
        JobElementsInventor.Clear();
        JobElementsInventor.Add(new NewTabButton(typeof(ProjectExplorerTab), "Exploration d'un assemblage"));
        JobElementsInventor.Add(new NewTabButton(typeof(InventorLaserTab), "Creation DXF PDF"));
        JobElementsInventor.Add(new NewTabButton(typeof(InventorPrintTab), "Impression des plans Inventor"));
        JobElementsInventor.Add(new NewTabButton(typeof(InventorQTTab), "Extrait la Nommenclature"));
        JobElementsInventor.Add(new NewTabButton(typeof(CleanProjectTab), "supprime les pièces orphelines"));
        JobElementsInventor.Add(new NewTabButton(typeof(PropertiesRenamerTab), "Renomme les champs"));
        JobElementsInventor.Add(new NewTabButton(typeof(DrawingBuilderTab), "Generation automatique DXF"));
    }

    private void VisibilityChangedEvent(CancellationToken token)
    {
        var visible = false;
        while (true)
        {
            Task.Delay(1000).Wait();
            if (token.IsCancellationRequested) break;
            if (InventorHelper2.AppIsVisible != visible)
            {
                visible = InventorHelper2.AppIsVisible;
                // Task.Delay(500).Wait();
                DispatcherQueue.TryEnqueue(() =>
                {
                    ToggleSwitchShowInventor.IsOn = visible;
                });
            }
        }
    }


    private void toggleSwitchInventor_Toggled(object sender, RoutedEventArgs e)
    {
        ToggleSwitchInventor.Toggled -= toggleSwitchInventor_Toggled;
        ToggleSwitchInventor.IsOn = !ToggleSwitchInventor.IsOn;
        ToggleSwitchInventor.Toggled += toggleSwitchInventor_Toggled;
    }




    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    
    private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args) =>
        sender.TabItems.Remove(args.Tab);

    private void OnThemeButtonClick(object sender, RoutedEventArgs e)
    {
        CurrentElementTheme = CurrentElementTheme == ElementTheme.Dark ? ElementTheme.Light : ElementTheme.Dark;
        MainPage.RequestedTheme = CurrentElementTheme;
    }

    private void MainWindow_OnClosed(object sender, WindowEventArgs args)
    {
        ctsVisibilityChangeTask.Cancel();
        InventorHelper2.CloseInstance();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}