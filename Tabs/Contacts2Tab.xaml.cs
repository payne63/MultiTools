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
using System.Collections.ObjectModel;
using SplittableDataGridSAmple.Base;
using SplittableDataGridSAmple.Helper;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.DataTransfer;
using System.Text;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs
{
    public sealed partial class Contacts2Tab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ObservableCollection<Contact> ContactsFiltered = new ObservableCollection<Contact>();
        public ObservableCollection<Company> CompanyFiltered = new ObservableCollection<Company>();

        public Contacts2Tab()
        {
            this.InitializeComponent();
            EditTipTextBox.KeyDown += delegate (object sender, KeyRoutedEventArgs e)
            {
                Trace.WriteLine("sender; "+sender);
                var EditTipTextBox =  sender as TextBox;
                var doubleTappedTextBlock = EditTipTextBox.DataContext as TextBlock;
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    if (doubleTappedTextBlock.Text != EditTipTextBox.Text)
                    {
                        doubleTappedTextBlock.Text = EditTipTextBox.Text;
                        //var be = doubleTappedTextBlock.GetBindingExpression(TextBox.TextProperty);
                        //Trace.WriteLine(be.ToString());
                        //if (be != null) be.UpdateSource();
                        StaticDataHelper.SaveContacts();
                    }
                    EditTip.IsOpen = false;
                    EditTip.IsEnabled = false;
                    return;
                }
            };
        }

        public async void InitTab()
        {
            foreach (var contact in await StaticDataHelper.GetAllcontacts())
            {
                ContactsFiltered.Add(contact);
            }
            foreach (Company company in await StaticDataHelper.GetAllCompany())
            {
                CompanyFiltered.Add(company);
            }
        }

        private async void Button_Click_MailQuotation(object sender, RoutedEventArgs e)
            => await MailManager.CreateMailQuotation(sender, e);
        private async void Button_Click_MailOrder(object sender, RoutedEventArgs e)
            => await MailManager.CreateMailOrder(sender, e);
        private async void Button_Click_Mail(object sender, RoutedEventArgs e)
            => await MailManager.CreateMail(sender, e);

        private void Button_Click_ContactAskToDelete(object sender, RoutedEventArgs e)
        {
        }
        private void Button_Click_ContactAskToUpdate(object sender, RoutedEventArgs e)
        {
        }
        private void Button_Click_Navigate(object sender, RoutedEventArgs e)
        {
            var address = ((Company)((Button)sender).DataContext).Address;
            var name = ((Company)((Button)sender).DataContext).Name;
            Process myProcess = new Process();
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = $"https://www.google.fr/maps/dir/Avitech/{name} {address}";
            myProcess.Start();
        }

        private async void Button_Click_EditCompany(object sender, RoutedEventArgs e)
        {
            var CompanySelect = ((Button)sender).DataContext as Company;
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Ajouter une Entreprise";
            dialog.PrimaryButtonText = "Sauvegarder";
            dialog.CloseButtonText = "Annuler";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var updateCompagnyDialogPage = new DialogPage.NewCompanyDialogPage(new() { Name = CompanySelect.Name as string, Address = CompanySelect.Address, Type = CompanySelect.Type });
            dialog.Content = updateCompagnyDialogPage;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                CompanySelect.CopyData(updateCompagnyDialogPage.CompanyContextCopy);
                //UpdateCVS();
                //SaveCompanys();
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("test");
            var list = await StaticDataHelper.GetAllCompany();
            foreach (var compagny in list)
            {
                Trace.WriteLine($"{compagny.Name}");
                CompanyFiltered.Add(compagny);
            }
        }

        private async void OnFilterChangedContact(object sender, TextChangedEventArgs args)
        {
            var filtered = (await StaticDataHelper.GetAllcontacts()).Where(contact => Filter(contact));
            Remove_NonMatching(filtered);
            AddBack_Contacts(filtered);
        }
        private bool Filter(Contact contact)
        {
            return contact.Name.Contains(FilterByNameOrPreName.Text, StringComparison.InvariantCultureIgnoreCase) ||
                    contact.PreName.Contains(FilterByNameOrPreName.Text, StringComparison.InvariantCultureIgnoreCase);
        }
        private void Remove_NonMatching(IEnumerable<Contact> filteredData)
        {
            for (int i = ContactsFiltered.Count - 1; i >= 0; i--)
            {
                var item = ContactsFiltered[i];
                if (!filteredData.Contains(item))
                {
                    ContactsFiltered.Remove(item);
                }
            }
        }

        private void AddBack_Contacts(IEnumerable<Contact> filteredData)
        {
            foreach (var item in filteredData)
            {
                if (!ContactsFiltered.Contains(item))
                {
                    ContactsFiltered.Add(item);
                }
            }
        }

        private async void OnFilterChangedCompagny(object sender, TextChangedEventArgs args)
        {
            var filtered = (await StaticDataHelper.GetAllCompany()).Where(company => Filter(company));
            Remove_NonMatching(filtered);
            AddBack_Contacts(filtered);
        }

        private bool Filter(Company company)
        {
            return company.Name.Contains(FilterByCompany.Text, StringComparison.InvariantCultureIgnoreCase);
        }
        private void Remove_NonMatching(IEnumerable<Company> filteredData)
        {
            for (int i = CompanyFiltered.Count - 1; i >= 0; i--)
            {
                var item = CompanyFiltered[i];
                if (!filteredData.Contains(item))
                {
                    CompanyFiltered.Remove(item);
                }
            }
        }

        private void AddBack_Contacts(IEnumerable<Company> filteredData)
        {
            foreach (var item in filteredData)
            {
                if (!CompanyFiltered.Contains(item))
                {
                    CompanyFiltered.Add(item);
                }
            }
        }

        private void Target_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count == 1)
            {
                // Prepare ListViewItem to be moved
                Contact tmp = (Contact)e.Items[0];

                e.Data.SetText(tmp.Name + " " + tmp.PreName + " " + tmp.Company);
                e.Data.RequestedOperation = DataPackageOperation.Link;
            }
        }
        private void Source_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            // Prepare a string with one dragged item per line
            StringBuilder items = new StringBuilder();
            foreach (Contact item in e.Items)
            {
                if (items.Length > 0) { items.AppendLine(); }
                if (item.ToString() != null)
                {
                    // Append name from contact object onto data string
                    items.Append(item.ToString() + " " + item.Company);
                }
            }
            // Set the content of the DataPackage
            e.Data.SetText(items.ToString());

            e.Data.RequestedOperation = DataPackageOperation.Move;

        }

        private async void ListViewItemContact_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var contact = ((FrameworkElement)sender).DataContext as Contact;
            var textBlockDoubleTapped = sender as TextBlock;
            if (textBlockDoubleTapped == null) return;
            EditTipTextBox.Text = textBlockDoubleTapped.Text;//contact.Name;
            EditTip.Target = ((TextBlock)sender);
            EditTipTextBox.DataContext = textBlockDoubleTapped;
            var binding = new Binding();
            binding.Mode = BindingMode.TwoWay;
            binding.Source = contact.Name;
            EditTipTextBox.SetBinding(TextBox.TextProperty, binding);
            EditTip.IsEnabled = true;
            EditTip.IsOpen = true;
            await Task.Delay(100);
            EditTipTextBox.Focus(FocusState.Programmatic);
            EditTipTextBox.SelectAll();

        }


        //private async void ListView_Drop(object sender, DragEventArgs e)
        //{
        //    ListView target = (ListView)sender;

        //    if (e.DataView.Contains(StandardDataFormats.Text))
        //    {
        //        DragOperationDeferral def = e.GetDeferral();
        //        string s = await e.DataView.GetTextAsync();
        //        string[] items = s.Split('\n');
        //        foreach (string item in items)
        //        {

        //            // Create Contact object from string, add to existing target ListView
        //            string[] info = item.Split(" ", 3);
        //            Contact temp = new Contact(info[0], info[1], info[2]);

        //            // Find the insertion index:
        //            Windows.Foundation.Point pos = e.GetPosition(target.ItemsPanelRoot);

        //            // If the target ListView has items in it, use the height of the first item
        //            //      to find the insertion index.
        //            int index = 0;
        //            if (target.Items.Count != 0)
        //            {
        //                // Get a reference to the first item in the ListView
        //                ListViewItem sampleItem = (ListViewItem)target.ContainerFromIndex(0);

        //                // Adjust itemHeight for margins
        //                double itemHeight = sampleItem.ActualHeight + sampleItem.Margin.Top + sampleItem.Margin.Bottom;

        //                // Find index based on dividing number of items by height of each item
        //                index = Math.Min(target.Items.Count - 1, (int)(pos.Y / itemHeight));

        //                // Find the item being dropped on top of.
        //                ListViewItem targetItem = (ListViewItem)target.ContainerFromIndex(index);

        //                // If the drop position is more than half-way down the item being dropped on
        //                //      top of, increment the insertion index so the dropped item is inserted
        //                //      below instead of above the item being dropped on top of.
        //                Windows.Foundation.Point positionInItem = e.GetPosition(targetItem);
        //                if (positionInItem.Y > itemHeight / 2)
        //                {
        //                    index++;
        //                }

        //                // Don't go out of bounds
        //                index = Math.Min(target.Items.Count, index);
        //            }
        //            // Only other case is if the target ListView has no items (the dropped item will be
        //            //      the first). In that case, the insertion index will remain zero.

        //            // Find correct source list
        //            if (target.Name == "DragDropListView")
        //            {
        //                // Find the ItemsSource for the target ListView and insert
        //                contacts1.Insert(index, temp);
        //                //Go through source list and remove the items that are being moved
        //                foreach (Contact contact in DragDropListView2.Items)
        //                {
        //                    if (contact.FirstName == temp.FirstName && contact.LastName == temp.LastName && contact.Company == temp.Company)
        //                    {
        //                        contacts2.Remove(contact);
        //                        break;
        //                    }
        //                }
        //            }
        //            else if (target.Name == "DragDropListView2")
        //            {
        //                contacts2.Insert(index, temp);
        //                foreach (Contact contact in DragDropListView.Items)
        //                {
        //                    if (contact.FirstName == temp.FirstName && contact.LastName == temp.LastName && contact.Company == temp.Company)
        //                    {
        //                        contacts1.Remove(contact);
        //                        break;
        //                    }
        //                }
        //            }
        //        }

        //        e.AcceptedOperation = DataPackageOperation.Move;
        //        def.Complete();
        //    }
        //}
    }

}
