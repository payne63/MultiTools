using Microsoft.Office.Interop.Outlook;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static MultiTools.MainWindow;
using Outlook = Microsoft.Office.Interop.Outlook;
using MultiTools.Helper;
using System.Runtime.InteropServices;

namespace MultiTools.Helper
{
    internal class OutlookHelper
    {
        Outlook.Application appOutlook = null;
        private Outlook.Application GetOutlookApplication()
        {
            if (Process.GetProcessesByName("OUTLOOK").Count() > 0)
            {

                // If so, use the GetActiveObject method to obtain the process and cast it to an Application object.
                appOutlook = Marshal2.GetActiveObject("Outlook.Application") as Outlook.Application;
            }
            else
            {

                // If not, create a new instance of Outlook and sign in to the default profile.
                appOutlook = new Outlook.Application();
                Outlook.NameSpace nameSpace = appOutlook.GetNamespace("MAPI");
                nameSpace.Logon("", "", Missing.Value, Missing.Value);
                nameSpace = null;
            }
            return appOutlook;
        }
        /// <summary>
        /// Open a New Mail with a text body and optional attachement files
        /// </summary>
        /// <param name="TextBody"></param>
        /// <param name="filesAttachementPath"></param>
        /// <exception cref="System.Exception"></exception>
        public void ShowNewMailITem(string mailFrom = "f.calet@free.fr",
                                        string mailTo = "maxpayne2024@gmail.com",
                                        string Subject = "Mail",
                                        string TextBody = "",
                                        params string[] filesAttachementPath)
        {
            if (appOutlook == null) GetOutlookApplication();
            if (appOutlook == null) { throw new System.Exception("impossible de générer une Instance d'Outlook"); }

            Outlook.MailItem mail = appOutlook.CreateItem(OlItemType.olMailItem) as Outlook.MailItem;
            var contact = appOutlook.CreateItem(OlItemType.olContactItem) as Outlook.ContactItem;
            foreach (var s in mail.Session.Accounts)
            {
                var r = s as Outlook.Account;
                Trace.WriteLine(r.SmtpAddress);
                if (r.SmtpAddress == mailFrom)
                {
                    mail.SendUsingAccount = r;
                }
            }
            mail.Subject = Subject;
            mail.To = mailTo;
            var text = TextBody + mail.HTMLBody;
            mail.HTMLBody = text + ReadSignature();
            if (filesAttachementPath != null)
            {
                if (filesAttachementPath.Length > 0)
                {
                    foreach (var file in filesAttachementPath)
                    {
                        mail.Attachments.Add(file);
                    }
                }
            }
            mail.Display(true);
        }
        private string ReadSignature()
        {
            string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\Signatures";
            string signature = string.Empty;
            DirectoryInfo diInfo = new DirectoryInfo(appDataDir);
            if (diInfo.Exists)
            {
                FileInfo[] fiSignature = diInfo.GetFiles("*.htm");
                if (fiSignature.Length > 0)
                {
                    StreamReader sr = new StreamReader(fiSignature[0].FullName, Encoding.Default);
                    signature = sr.ReadToEnd();

                    if (!string.IsNullOrEmpty(signature))
                    {
                        string fileName = fiSignature[0].Name.Replace(fiSignature[0].Extension, string.Empty);
                        signature = signature.Replace(fileName + "_files/", appDataDir + "/" + fileName + "_files/");
                    }
                }
            }

            return signature;
        }
    }
}
