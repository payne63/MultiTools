// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SplittableDataGridSAmple.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SplittableDataGridSAmple.Tabs
{
    public sealed partial class FolderProjectCreationTab : TabViewItem, Interfaces.IInitTab, INotifyPropertyChanged
    {

        #region onPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        private DirectoryInfo _FolderSelected;
        public DirectoryInfo FolderSelected
        {
            get { return _FolderSelected; }
            set { _FolderSelected = value; OnPropertyChanged(); }
        }

        private string _CodeProject;
        public string CodeProject
        {
            get { return _CodeProject; }
            set { _CodeProject = value; OnPropertyChanged(); }
        }

        private string _CustomerName;
        public string CustomerName
        {
            get { return _CustomerName; }
            set { _CustomerName = value; OnPropertyChanged(); }
        }
        private string _ProjectName;

        public string ProjectName
        {
            get { return _ProjectName; }
            set { _ProjectName = value; OnPropertyChanged(); }
        }


        public bool _IsInterfaceEnabled;
        public bool IsInterfaceEnabled
        {
            get { return _IsInterfaceEnabled; }
            set { _IsInterfaceEnabled = value; OnPropertyChanged(); }
        }

        private Visibility _FolderSelectedVisibity;
        public Visibility FolderSelectedVisibity
        {
            get { return _FolderSelectedVisibity; }
            set { _FolderSelectedVisibity = value; OnPropertyChanged(); }
        }


        public FolderProjectCreationTab()
        {
            this.InitializeComponent();
        }

        public void InitTabAsync()
        {
            IsInterfaceEnabled = true;
            FolderSelectedVisibity = Visibility.Collapsed;
        }

        private async void TabViewItem_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count == 1)
                {
                    var file = items.First();
                    if (Directory.Exists(file.Path))
                    {
                        FolderSelected = new DirectoryInfo(file.Path);
                        var split = FolderSelected.Name.Split(" - ");
                        if (split.Count() == 3)
                        {
                            CodeProject = split[0];
                            ProjectName = split[1];
                            CustomerName = split[2];
                        }
                        FolderSelectedVisibity = Visibility.Visible;
                    }
                }
            }
        }

        private void TabViewItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private async void Button_Click_RenameFolder(object sender, RoutedEventArgs e)
        {
            if (FolderSelected == null)
            {
                await OpenSimpleMessage("Veuillez selectioner en premier un répertoire cible");
                return;
            }
            if (FolderSelected.GetFiles().Length != 0 || FolderSelected.GetDirectories().Length != 0)
            {
                await OpenSimpleMessage("Le répertoire cible n'est pas vide, action impossible");
                return;
            }
            if (CodeProject == string.Empty || ProjectName == string.Empty || CustomerName == string.Empty)
            {
                await OpenSimpleMessage("Veuillez remplir tout les champs");
                return;
            }
            IsInterfaceEnabled = false;
            var newFolder = FolderSelected.Parent.FullName + $"\\{CodeProject} - {ProjectName} - {CustomerName}";
            if (FolderSelected.FullName != newFolder)
                Directory.Move(FolderSelected.FullName, newFolder);
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Consultations & Commandes");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Documents contractuelss\\{CodeProject} - Documents Client");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Documents contractuelss\\{CodeProject} - Documents Internes");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes électriques\\{CodeProject} - Implantation armoire");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes électriques\\{CodeProject} - Notice");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes électriques\\{CodeProject} - Programme automate");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes électriques\\{CodeProject} - Programme écran");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes électriques\\{CodeProject} - Schéma électrique");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes électriques\\{CodeProject} - Schéma pneumatique");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes mécaniques\\{CodeProject} - Composants");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes mécaniques\\{CodeProject} - Elements client");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Etudes mécaniques\\{CodeProject} - Plans");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Suivi\\{CodeProject} - Notice technique");
            Directory.CreateDirectory(newFolder + $"\\{CodeProject} - Suivi\\{CodeProject} - Photos");
            IsInterfaceEnabled = true;
            FolderSelectedVisibity = Visibility.Collapsed;

            await OpenSimpleMessage("création terminée");
        }

        private void Button_Click_ClearInfo(object sender, RoutedEventArgs e)
        {
            FolderSelected = null;
            FolderSelectedVisibity = Visibility.Collapsed;
            ProjectName = string.Empty;
            CodeProject = string.Empty;
            CustomerName = string.Empty;
        }

        private async Task OpenSimpleMessage(string Message)
        {
            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = Message,
                PrimaryButtonText = "Ok",
                DefaultButton = ContentDialogButton.Primary,
            };
            _ = await dialog.ShowAsync();
        }
    }
}
