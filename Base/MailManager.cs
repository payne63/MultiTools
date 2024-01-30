using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Base
{
    public static class MailManager
    {
        public static async Task CreateMailQuotation(object sender, RoutedEventArgs e)
        {
            if (IsUserIsNotSelected()) return;

            var contact = ((FrameworkElement)sender).DataContext as Contact;
            if (contact == null) throw new Exception("impossible de récuperer le datacontext");
            if (await IsMailisValide(contact.Mail, ((FrameworkElement)sender).XamlRoot) == false) return;
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
        public static async Task CreateMailOrder(object sender, RoutedEventArgs e)
        {
            if (IsUserIsNotSelected()) return;

            var contact = ((FrameworkElement)sender).DataContext as Contact;
            if (contact == null) throw new Exception("impossible de récuperer le datacontext");
            if (await IsMailisValide(contact.Mail, ((FrameworkElement)sender).XamlRoot) == false) return;
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

        public static async Task CreateMail(object sender, RoutedEventArgs e)
        {
            if (IsUserIsNotSelected()) return;

            var contact = ((FrameworkElement)sender).DataContext as Contact;
            if (contact == null) throw new Exception("impossible de récuperer le datacontext");
            if (await IsMailisValide(contact.Mail,((FrameworkElement)sender).XamlRoot) == false) return;
            var outlookHelper = new Helper.OutlookHelper();
            var civilite = contact.IsMale ? "Mr" : "Mme";
            outlookHelper.ShowNewMailITem(MainWindow.Instance.GetSelectedUser.MailAdress,
                contact.Mail,
                "Information",
                $"Bonjour {civilite} {contact.Name} <br />" +
                $"  <br />" +
                $"  <br />");
        }

        private static bool IsUserIsNotSelected()
        {
            return MainWindow.Instance.GetSelectedUser == null;
        }

        private static async Task<bool> IsMailisValide(string mail,XamlRoot xamlRoot)
        {
            try
            {
                MailAddress mailAdress = new MailAddress(mail);
                return true;
            }
            catch (FormatException)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = xamlRoot;
                dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
                dialog.Title = "Mail invalide";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Close;
                var result = await dialog.ShowAsync();
                return false;
            }
        }
    }
}
