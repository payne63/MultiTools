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
using SplittableDataGridSAmple.Base;
using System.Collections.ObjectModel;
//using System.Text.Json;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using MsgReader.Outlook;
using SplittableDataGridSAmple.Helper;
using System.Threading.Tasks;
using SplittableDataGridSAmple.Interfaces;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs
{

    public sealed partial class Test3Tab : TabViewItem ,IInitTab
    {
        public ObservableCollection<Character> Characters { get; set; } = new();

        public Test3Tab()
        {
            this.InitializeComponent();

        }
        private void Button_Click_AddElement(object sender, RoutedEventArgs e)
        {
            Characters.Add(new Character { Name = "Florent", Age = "41" });
            Characters.Add(new Character { Name = "Laetitia", Age = "40" });
            Characters.Add(new Character { Name = "Nathan", Age = "12" });
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["popo"] = "testSave";
        }

        private async void SAVE_Click(object sender, RoutedEventArgs e)
        {
            await JsonHelper.SaveArray<Character>(Characters.ToArray(), "test.json");
        }

        private async void LOAD_Click(object sender, RoutedEventArgs e)
        {
            foreach (var charactere in await JsonHelper.LoadArray<Character>("test.json"))
            {
                Characters.Add(charactere);
            }
        }

        private async void TabViewItem_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;
                    if (storageFile != null)
                    {
                        Trace.WriteLine($"storage file: {storageFile.Path}");
                        readMSG(new FileInfo(storageFile.Path));
                    }
                }
            }
        }
        public void readMSG(FileInfo fileInfo)
        {
            try
            {
                if (fileInfo.Extension.ToLower().Equals(".msg"))
                {

                    using (var msg = new MsgReader.Outlook.Storage.Message(fileInfo.FullName))
                    {
                        var contact  = msg.Contact;
                        if (contact != null) 
                        {
                            Trace.WriteLine(contact.SurName);//Prenom
                            Trace.WriteLine(contact.GivenName);//Nom
                            Trace.WriteLine(contact.Function);//Poste

                            Trace.WriteLine(contact.CellularTelephoneNumber);//portable
                            Trace.WriteLine(contact.Email1EmailAddress);
                            Trace.WriteLine(contact.BusinessTelephoneNumber);
                            Trace.WriteLine(contact.BusinessTelephoneNumber2);
                            Trace.WriteLine(contact.HomeTelephoneNumber);
                            Trace.WriteLine(contact.HomeTelephoneNumber2);
                            Trace.WriteLine(contact.PrimaryTelephoneNumber);

                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine($"{fileInfo.Name} Unable to convert  msg file: {ex.Message}.");
            }
        }
        private void TabViewItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
        private void DataGridInstance_Sorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
        {
            if (e.Column.Tag.ToString() == "name")
            {
                if (e.Column.SortDirection == null || e.Column.SortDirection == CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending)
                {
                    dg.ItemsSource = new ObservableCollection<Character>(from item in Characters orderby item.Name ascending select item);
                    e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending;
                }
                else
                {
                    dg.ItemsSource = new ObservableCollection<Character>(from item in Characters orderby item.Name descending select item);
                    e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending;
                }
            }
            if (e.Column.Tag.ToString() == "age")
            {
                if (e.Column.SortDirection == null || e.Column.SortDirection == CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending)
                {
                    dg.ItemsSource = new ObservableCollection<Character>(from item in Characters orderby item.Age ascending select item);
                    e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending;
                }
                else
                {
                    dg.ItemsSource = new ObservableCollection<Character>(from item in Characters orderby item.Age descending select item);
                    e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending;
                }
            }
            foreach (var c in dg.Columns)
            {
                if (c.Tag.ToString() != e.Column.Tag.ToString())
                {
                    c.SortDirection = null;
                }
            }
        }
        private void Button_Click_PrintData(object sender, RoutedEventArgs e)
        {
            if (Characters.Count > 0)
            {
                Trace.WriteLine(Characters.First().Name + $"check{Characters.First().Work}");
            }
        }

        public void InitTab()
        {
            Characters.Add(new Character { Name = "First", Age = "100", Work = true });
            Characters.First().AddChildren(new Character { Name = "Children", Age = "1", Work = false });
            Characters.First().AddChildren(new Character { Name = "Children2", Age = "2", Work = false });
        }
    }
}
