// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CommunityToolkit.WinUI.UI;
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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static MsgReader.Outlook.Storage;
using SplittableDataGridSAmple.Elements;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs;

public sealed partial class OpenNewTab : TabViewItem, Interfaces.IInitTab
{
    public ObservableCollection<NewTabButton> JobElementsInventor { get; set; } = new();
    public ObservableCollection<NewTabButton> JobElements { get; set; } = new();

    public OpenNewTab()
    {
        this.InitializeComponent();
        InitTab();
    }

    private void PopulateElements()
    {
        JobElementsInventor.Clear();
        JobElements.Clear();
        //JobElements.Add(new NewTabButton(typeof(Tabs.WelcomeTab), "Page d'Acceuil"));
        JobElementsInventor.Add(new NewTabButton(typeof(Tabs.ProjectExplorerTab), "Exploration d'un assemblage"));
        JobElements.Add(new NewTabButton(typeof(Tabs.ParameterTab), "Réglage des options", true));
        JobElements.Add(new NewTabButton(typeof(Tabs.ContactsTab), "Contacts",true));
        JobElements.Add(new NewTabButton(typeof(Tabs.Contacts2Tab), "Contacts 2",true));
        JobElementsInventor.Add(new NewTabButton(typeof(Tabs.InventorLaserTab), "Creation DXF PDF"));
        JobElementsInventor.Add(new NewTabButton(typeof(Tabs.InventorPrintTab), "Impression des plans Inventor"));
        JobElements.Add(new NewTabButton(typeof(Tabs.FolderProjectCreationTab),"creation d'un répertoire Projet"));
        JobElementsInventor.Add(new NewTabButton(typeof(Tabs.InventorQTTab),"Extrait la Nommenclature"));
        //JobElements.Add(new NewTabButton(typeof(Tabs.Test1Tab),"test1"));
        //JobElements.Add(new NewTabButton(typeof(Tabs.Test2Tab),"Test2"));
        //JobElements.Add(new NewTabButton(typeof(Tabs.Test3Tab),"Test3"));
        JobElementsInventor.Add(new NewTabButton(typeof(Tabs.DrawingBuilderTab),"Generation automatique DXF" ));
    }

    public void InitTab()
    {
        PopulateElements();
    }
    private void Button_Click_AvitechLink(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer", @"https://www.avitech-france.fr/avitech/");
    }
    private void Button_Click_RepoLink(object sender, RoutedEventArgs e)
    {
        Process.Start("explorer",@"https://github.com/payne63/MultiTools");
    }

    private async void Button_Click_Support(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Supporter le développeur",
            Content = "réfléchir ca donne soif, et je te préviens, j'aime pas le café",
            PrimaryButtonText = "Ok",
            DefaultButton = ContentDialogButton.Primary,
        };
        _ = await dialog.ShowAsync();
    }
}
