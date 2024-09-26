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
using SplittableDataGridSAmple.Tabs.InventorTab;
using SplittableDataGridSAmple.Tabs.TestTab;
using SplittableDataGridSAmple.Tabs.VariousTab;

namespace SplittableDataGridSAmple.Tabs;


public sealed partial class OpenNewTab : TabViewItem, Interfaces.IInitTab
{
    public ObservableCollection<NewTabButton> JobElementsInventor
    {
        get;
        set;
    } = new();

    public ObservableCollection<NewTabButton> JobElements
    {
        get;
        set;
    } = new();

    public OpenNewTab()
    {
        this.InitializeComponent();
        InitTabAsync();
    }

    private void PopulateElements()
    {
        JobElementsInventor.Clear();
        JobElements.Clear();
        //JobElements.Add(new NewTabButton(typeof(Tabs.WelcomeTab), "Page d'Acceuil"));
        JobElementsInventor.Add(new NewTabButton(typeof(ProjectExplorerTab), "Exploration d'un assemblage"));
        JobElements.Add(new NewTabButton(typeof(ParameterTab), "Réglage des options", true));
        JobElements.Add(new NewTabButton(typeof(ContactsTab), "Contacts", true));
        JobElements.Add(new NewTabButton(typeof(Contacts2Tab), "Contacts 2", true));
        JobElementsInventor.Add(new NewTabButton(typeof(InventorLaserTab), "Creation DXF PDF"));
        JobElementsInventor.Add(new NewTabButton(typeof(InventorPrintTab), "Impression des plans Inventor"));
        JobElements.Add(new NewTabButton(typeof(FolderProjectCreationTab), "creation d'un répertoire Projet"));
        JobElementsInventor.Add(new NewTabButton(typeof(InventorQTTab), "Extrait la Nommenclature"));
        JobElementsInventor.Add(new NewTabButton(typeof(CleanProjectTab), "supprime les pièces orphelines"));
        JobElementsInventor.Add(new NewTabButton(typeof(PropertiesRenamerTab), "Renomme les champs", true));
        //JobElements.Add(new NewTabButton(typeof(Test1Tab),"test1"));
        //JobElements.Add(new NewTabButton(typeof(Test2Tab),"Test2"));
        //JobElements.Add(new NewTabButton(typeof(Test3Tab),"Test3"));
        JobElementsInventor.Add(new NewTabButton(typeof(DrawingBuilderTab), "Generation automatique DXF"));
    }

    public void InitTabAsync() => PopulateElements();

    private void Button_Click_AvitechLink(object sender, RoutedEventArgs e) =>
        Process.Start("explorer", @"https://www.avitech-france.fr/avitech/");


    private void Button_Click_RepoLink(object sender, RoutedEventArgs e) =>
        Process.Start("explorer", @"https://github.com/payne63/MultiTools");


    private async void Button_Click_Support(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog
        {
            XamlRoot = XamlRoot,
            Title = "Supporter le développeur",
            Content = "Coder ca donne soif, et je te préviens, j'aime pas le café",
            PrimaryButtonText = "Ok",
            DefaultButton = ContentDialogButton.Primary,
        };
        _ = await dialog.ShowAsync();
    }
}