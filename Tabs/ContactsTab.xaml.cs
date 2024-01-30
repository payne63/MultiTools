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
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.UI.Xaml.Documents;
using SplittableDataGridSAmple.Helper;
using Windows.System;
using System.Net.Mail;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs
{
    public sealed partial class ContactsTab : TabViewItem, Interfaces.IInitTab
    {
        public static string TitleTab = "ContactsTab";
        public ObservableCollection<Contact> Contacts { get; private set; } = new();
        public ObservableCollection<Company> Companys { get; private set; } = new();

        public ContactsTab()
        {
            this.InitializeComponent();
            CVS.IsSourceGrouped = true;
        }

        public CollectionViewSource CVS { get; private set; } = new();

        //public ObservableCollection<Contact> FilteredContacts = new();


        public ObservableCollection<GroupInfoList> GetContactsGrouped(ObservableCollection<Contact> listOfContact)
        {
            var query = from item in listOfContact
                        group item by item.Company.Name into g
                        orderby g.Key
                        select new GroupInfoList(g) { Key = g.Key };
            var result = query.ToList<GroupInfoList>();
            foreach (List<Contact> group in result)
            {
                group.Sort((x, y) => string.Compare(x.Name, y.Name));
            }
            foreach (var gil in result)
            {
                gil.Address = ((List<Contact>)gil).First().Company.Address;
                gil.Type = ((List<Contact>)gil).First().Company.Type;
                gil.Company = ((List<Contact>)gil).First().Company;
            }
            return new ObservableCollection<GroupInfoList>(result);
        }


        public async void InitTab()
        {
            await LoadCompanys();
            await LoadContacts();
            /*
            //Companys.Add(new Company { Name = "Francaise de Gastronomie", Address = "Sainte Anne, 43100 Vieille-Brioude", Type = Company.TypeEnum.Customer });
            //Companys.Add(new Company { Name = "Limagrain Ennezat", Address = "21 Av. de la Gare, 63720 Ennezat", Type = Company.TypeEnum.Customer });
            //Companys.Add(new Company { Name = "LFO Saint Flour", Address = "Rue Léopold Chastang, 15100 Saint-Flour", Type = Company.TypeEnum.Customer });
            //Companys.Add(new Company { Name = "Ammeral", Address = "ZAC, 42120 Commelle-Vernay", Type = Company.TypeEnum.Supplier | Company.TypeEnum.Customer });

            //Contacts.Add(new Contact
            //{
            //    Name = "Tremouiller",
            //    PreName = "Franck",
            //    Mail = "maitenance@francaisdegastronomie.fr",
            //    JobDescription = "Responsable Maintenance",
            //    PhoneNumber = "57167161",
            //    PhoneNumber2 = "51244131",
            //    Company = Companys.First(c => c.Name == "Francaise de Gastronomie")
            //});
            //Contacts.Add(new Contact
            //{
            //    Name = "XXXX",
            //    PreName = "Joel",
            //    Mail = "francaisdegastronomie.fr",
            //    JobDescription = "Responsable de Site",
            //    PhoneNumber = "52717617",
            //    Company = Companys.First(c => c.Name == "Francaise de Gastronomie")
            //});
            //Contacts.Add(new Contact
            //{
            //    Name = "aaaaa",
            //    PreName = "bbbb",
            //    Mail = "francaisdegastronomie.fr",
            //    JobDescription = "Responsable de Site",
            //    PhoneNumber = "52717617",
            //    Company = Companys.First(c => c.Name == "Francaise de Gastronomie")
            //});
            //Contacts.Add(new Contact
            //{
            //    Name = "Souleyrasse",
            //    PreName = "Amaury",
            //    Mail = "amaury@limgrain.fr",
            //    JobDescription = "Responsable Maintenance",
            //    PhoneNumber = "12345678",
            //    Company = Companys.First(c => c.Name == "Limagrain Ennezat")
            //});
            //Contacts.Add(new Contact
            //{
            //    Name = "Cornet",
            //    PreName = "Gilles",
            //    Mail = "Gilles.cornet@ammeral.com",
            //    PhoneNumber = "445161671",
            //    PhoneNumber2 = "517524165",
            //    Company = Companys.First(c => c.Name == "Ammeral")
            //});
            */
            CVS.Source = GetContactsGrouped(Contacts);
        }

        private async Task<bool> IsMailisValide(string mail)
        {
            try
            {
                MailAddress mailAdress = new MailAddress(mail);
                return true;
            }
            catch (FormatException)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "Mail invalide";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Close;
                var result = await dialog.ShowAsync();
                return false;
            }
        }

        private async Task MessageUserNotSelected()
        {
            TeachingTipSelectUser.IsOpen = true;
            await Task.Delay(5000);
            TeachingTipSelectUser.IsOpen = false;
        }

        private async void Button_Click_MailQuotation(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.GetSelectedUser == null)
            {
                await MessageUserNotSelected();
                return;
            }
            var contact = ((FrameworkElement)sender).DataContext as Contact;
            if (contact == null) throw new Exception("impossible de récuperer le datacontext");
            if (await IsMailisValide(contact.Mail) == false) return;
            var outlookHelper = new Helper.OutlookHelper();
            var civilite = contact.IsMale ? "Mr" : "Mme";
            outlookHelper.ShowNewMailITem(MainWindow.Instance.GetSelectedUser.MailAdress,
                contact.Mail,
                "Demande de prix",
                $"Bonjour {civilite} {contact.Name} <br />" +
                $" Pouvez vous me faire votre meilleur offre pour les éléments ci dessous: <br />" +
                $" - <br />" +
                $"  <br />" +
                $"  <br />" +
                $"  <br />");
        }
        private async void Button_Click_MailOrder(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.GetSelectedUser == null)
            {
                await MessageUserNotSelected();
                return;
            }
            var contact = ((FrameworkElement)sender).DataContext as Contact;
            if (contact == null) throw new Exception("impossible de récuperer le datacontext");
            if (await IsMailisValide(contact.Mail) == false) return;
            var outlookHelper = new Helper.OutlookHelper();
            var civilite = contact.IsMale ? "Mr" : "Mme";
            outlookHelper.ShowNewMailITem(MainWindow.Instance.GetSelectedUser.MailAdress,
                contact.Mail,
                "Commande",
                $"Bonjour {civilite} {contact.Name} <br />" +
                $" Veuillez trouver ci joint la commande AV- <br />" +
                $"Merci de nous confirmer par retour votre bonne réception <br />" +
                $"  <br />" +
                $"  <br />" +
                $"  <br />",
                Environment.CurrentDirectory + "\\JsonData\\users.json");
        }

        private async void Button_Click_Mail(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Instance.GetSelectedUser == null)
            {
                await MessageUserNotSelected();
                return;
            }
            var contact = ((FrameworkElement)sender).DataContext as Contact;
            if (contact == null) throw new Exception("impossible de récuperer le datacontext");
            if (await IsMailisValide(contact.Mail) == false) return;
            var outlookHelper = new Helper.OutlookHelper();
            var civilite = contact.IsMale ? "Mr" : "Mme";
            outlookHelper.ShowNewMailITem(MainWindow.Instance.GetSelectedUser.MailAdress,
                contact.Mail,
                "Information",
                $"Bonjour {civilite} {contact.Name} <br />" +
                $"  <br />" +
                $"  <br />");
        }

        private void UpdateCVS()
        {
            var filtered = Contacts.Where(contact => FilterContact(contact) && FilterCompany(contact.Company));
            CVS.Source = GetContactsGrouped(new ObservableCollection<Contact>(filtered));
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => UpdateCVS();

        private void OnFilterChanged(object sender, TextChangedEventArgs args) => UpdateCVS();

        private bool FilterContact(Contact contact)
        {
            return (contact.Name.Contains(FilterByNameOrPreName.Text, StringComparison.InvariantCultureIgnoreCase) ||
                    contact.PreName.Contains(FilterByNameOrPreName.Text, StringComparison.InvariantCultureIgnoreCase)) &&
                    contact.Company.Name.Contains(FilterByCompany.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool FilterCompany(Company company)
        {
            Company.TypeEnum actualFilterType = CheckCustomer.IsChecked == true ? Company.TypeEnum.Client : Company.TypeEnum.Aucun;
            actualFilterType |= CheckSupplier.IsChecked == true ? Company.TypeEnum.Fournisseur : Company.TypeEnum.Aucun;
            actualFilterType |= CheckIntern.IsChecked == true ? Company.TypeEnum.Interne : Company.TypeEnum.Aucun;
            return (actualFilterType & company.Type) != 0;
        }

        private void Button_Click_ContactAskToDelete(object sender, RoutedEventArgs e)
        {
            teachingTipDeleteContact.FrameWorkElementLink = sender;
            teachingTipDeleteContact.IsOpen = true;
        }

        private void DeleteContact_Click(TeachingTip sender, object args)
        {
            teachingTipDeleteContact.IsOpen = false;
            Contact contact = ((FrameworkElement)teachingTipDeleteContact.FrameWorkElementLink).DataContext as Contact;
            Trace.WriteLine(contact);
            if (contact == null) return;
            Trace.WriteLine(contact.Name);
            Contacts.Remove(contact);
            teachingTipDeleteContact.FrameWorkElementLink = null;
            UpdateCVS();
            SaveContacts();
        }

        private async void Button_Click_ContactAskToUpdate(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Edition des informations du Contact";
            dialog.PrimaryButtonText = "Sauvegarder";
            dialog.CloseButtonText = "Annuler";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var updateContactDialogPage = new DialogPage.UpdateContactDialogPage(((FrameworkElement)sender).DataContext as Contact, Companys);
            dialog.Content = updateContactDialogPage;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                updateContactDialogPage.oldContact.CopyData(updateContactDialogPage.contactContext);
                UpdateCVS();
                SaveContacts();
            }
        }

        private async void Button_Click_AddContact(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Ajouter un Contact";
            dialog.PrimaryButtonText = "Sauvegarder";
            dialog.CloseButtonText = "Annuler";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var updateContactDialogPage = new DialogPage.UpdateContactDialogPage(new() { Company = Companys.First() }, Companys);
            dialog.Content = updateContactDialogPage;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Contacts.Add(updateContactDialogPage.contactContext);
                UpdateCVS();
                SaveContacts();
            }
        }

        private async void Button_Click_AddCompagny(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Ajouter une Entreprise";
            dialog.PrimaryButtonText = "Sauvegarder";
            dialog.CloseButtonText = "Annuler";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var updateContactDialogPage = new DialogPage.NewCompanyDialogPage(new());
            dialog.Content = updateContactDialogPage;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Companys.Add(updateContactDialogPage.CompanyContextCopy);
                UpdateCVS();
                SaveCompanys();
            }
        }

        private void Button_Click_Navigate(object sender, RoutedEventArgs e)
        {
            var address = ((GroupInfoList)((Button)sender).DataContext).Address;
            var name = ((GroupInfoList)((Button)sender).DataContext).Key;
            Process myProcess = new Process();
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = $"https://www.google.fr/maps/dir/Avitech/{name} {address}";
            myProcess.Start();
        }

        private async void Button_Click_EditCompany(object sender, RoutedEventArgs e)
        {
            var CompanySelect = ((Button)sender).DataContext as GroupInfoList;
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Ajouter une Entreprise";
            dialog.PrimaryButtonText = "Sauvegarder";
            dialog.CloseButtonText = "Annuler";
            dialog.DefaultButton = ContentDialogButton.Primary;
            var updateCompagnyDialogPage = new DialogPage.NewCompanyDialogPage(new() { Name = CompanySelect.Key as string , Address = CompanySelect.Address , Type= CompanySelect.Type});
            dialog.Content = updateCompagnyDialogPage;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                CompanySelect.Company.CopyData(updateCompagnyDialogPage.CompanyContextCopy);
                UpdateCVS();
                SaveCompanys();
            }

        }

        private async Task LoadContacts()
        {
            Contacts.Clear();
            foreach (var contact in await JsonHelper.LoadArray<Base.Contact>(MainWindow.ContactsDataPath))
            {
                Contacts.Add(contact);
            }
        }

        private async void SaveContacts()
        {
            await JsonHelper.SaveArray<Base.Contact>(Contacts.ToArray(), MainWindow.ContactsDataPath);
        }

        private async Task LoadCompanys()
        {
            Companys.Clear();
            foreach (var compagny in await JsonHelper.LoadArray<Base.Company>(MainWindow.CompanyDataPath))
            {
                Companys.Add(compagny);
            }
        }

        private async void SaveCompanys()
        {
            await JsonHelper.SaveArray<Base.Company>(Companys.ToArray(), MainWindow.CompanyDataPath);
        }
    }
    public class GroupInfoList : List<Contact>
    {
        public GroupInfoList(IEnumerable<Contact> items) : base(items)
        {
        }
        public object Key { get; set; }
        public string Address { get; set; }
        public Company.TypeEnum Type { get; set; }
        public Company Company { get; set; }
        //public string StringTypeEnum
        //{
        //    get { return $"({Type})"; }
        //    set { }
        //}
    }
}
