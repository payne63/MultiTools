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
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using System.Diagnostics;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.DialogPage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UpdateContactDialogPage : Page
    {
        private Contact _contactContext;
        public Contact oldContact;
        private ObservableCollection<Company> _companyObservable;
        public Contact contactContext
        {
            get { return _contactContext; }
            set { _contactContext = value; }
        }

        public UpdateContactDialogPage()
        {
            this.InitializeComponent();
        }

        public UpdateContactDialogPage(Contact contact, ObservableCollection<Company> companies)
        {
            this.InitializeComponent();
            this._companyObservable = companies;
            this.oldContact = contact;
            this.contactContext = contact.CreateDeepCopy();
        }

        private void updateCompany_Loaded(object sender, RoutedEventArgs e)
        {
            updateCompany.SelectedIndex = _companyObservable.ToList().FindIndex(c => c.Name == oldContact.Company.Name);
        }

        private void updateCompany_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            contactContext.Company = _companyObservable[((ComboBox)sender).SelectedIndex];
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

        private void TabViewItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        public void readMSG(FileInfo fileInfo)
        {
            try
            {
                if (fileInfo.Extension.ToLower().Equals(".msg"))
                {

                    using (var msg = new MsgReader.Outlook.Storage.Message(fileInfo.FullName))
                    {
                        var contact = msg.Contact;
                        if (contact != null)
                        {
                            contactContext.Name = contact.GivenName;
                            contactContext.PreName = contact.SurName;
                            contactContext.JobDescription = contact.Function;

                            contactContext.Mail = contact.Email1EmailAddress;

                            var phonelist = new List<string>();
                            phonelist.Add(contact.BusinessTelephoneNumber);
                            phonelist.Add(contact.BusinessTelephoneNumber2);
                            phonelist.Add(contact.CellularTelephoneNumber);
                            phonelist.Add(contact.HomeTelephoneNumber);
                            phonelist.Add(contact.HomeTelephoneNumber2);

                            var CompletNumbers = phonelist.Where(x => x != string.Empty).ToList();
                            switch (CompletNumbers.Count())
                            {
                                case 0:
                                    return;
                                case 1:
                                    contactContext.PhoneNumber = CompletNumbers[0];
                                    break;
                                default:
                                    contactContext.PhoneNumber = CompletNumbers[0];
                                    contactContext.PhoneNumber2 = CompletNumbers[1];
                                    break;
                            }


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
    }
}
