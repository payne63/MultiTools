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
using MultiTools.Base;
using System.Collections.ObjectModel;
//using System.Text.Json;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using MsgReader.Outlook;
using System.Threading.Tasks;
using AvitechTools.Models;
using Inventor;
using DocumentFormat.OpenXml.Math;
using MultiTools.Helper;
using MultiTools.Interfaces;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MultiTools.Tabs.TestTab;

public sealed partial class Test3Tab : TabViewItem, IInitTab
{
    public Test3Tab()
    {
        this.InitializeComponent();
    }
    public void InitTabAsync()
    {

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
                    var contact = msg.Contact;
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
        e.AcceptedOperation = DataPackageOperation.Move;
    }

    private void Button_Click_Dxf(object sender, RoutedEventArgs e)
    {
        var partFile = @"E:\testPlan\P641-051-03.ipt";

    }

    private async void Button_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            var drawingDocument = DXFBuilderHelper.BuildTrueSheetMetal(null,@"E:\testPlan\P641-110.ipt");
        }
        catch (Exception ex)
        {
            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Erreur",
                Content = ex.Message,
                PrimaryButtonText = "Fermer",
                DefaultButton = ContentDialogButton.Primary,
            };
            _ = await dialog.ShowAsync();
        }
    }
}