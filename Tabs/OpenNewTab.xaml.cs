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

namespace SplittableDataGridSAmple.Tabs
{
    public sealed partial class OpenNewTab : TabViewItem, Interfaces.IInitTab
    {
        public ObservableCollection<NewTabButton> JobElements { get; set; } = new();

        public OpenNewTab()
        {
            this.InitializeComponent();
        }

        private void PopulateElements()
        {
            JobElements.Add(new NewTabButton(typeof(Tabs.WelcomeTab), "Page d'Acceuil"));
            JobElements.Add(new NewTabButton(typeof(Tabs.ProjectExplorerTab), "Exploration d'un assemblage"));
            JobElements.Add(new NewTabButton(typeof(Tabs.ParameterTab), "Réglage des options", true));
            JobElements.Add(new NewTabButton(typeof(Tabs.ContactsTab), "Contacts",true));
            JobElements.Add(new NewTabButton(typeof(Tabs.Contacts2Tab), "Contacts 2",true));
            JobElements.Add(new NewTabButton(typeof(Tabs.InventorLaserTab), "Creation DXF PDF"));
            JobElements.Add(new NewTabButton(typeof(Tabs.InventorPrintTab), "Impression des plans Inventor"));
            JobElements.Add(new NewTabButton(typeof(Tabs.FolderProjectCreationTab),"Permet la creation d'un nouveau répertoire Projet"));
            JobElements.Add(new NewTabButton(typeof(Tabs.InventorQTTab),"Extrait la Nommenclature pour les pièces",true));
            //JobElements.Add(new NewTabButton(typeof(Tabs.Test1Tab),"test1"));
            //JobElements.Add(new NewTabButton(typeof(Tabs.Test2Tab),"Test2"));
            JobElements.Add(new NewTabButton(typeof(Tabs.Test3Tab),"Test3"));
        }

        public void InitTab()
        {
            PopulateElements();
        }
     
    }
}
