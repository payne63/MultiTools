using AvitechTools.Models;
using Inventor;
using Microsoft.Office.Interop.Outlook;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SplittableDataGridSAmple.Helper;
using SplittableDataGridSAmple.Models;
using SplittableDataGridSAmple.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using I = Inventor;

namespace SplittableDataGridSAmple.Base
{
    public class DataI : INotifyPropertyChanged
    {
        public static ProjectExplorerTab instanceProjectExplorer;
        public enum RecursiveType { True, False, OneTime }
        public enum CategoryType { Inconnu, Laser, Commerce, Profile, MecanoSoudure, Assemblage }

        #region Inotify_Business
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion 

        public ValidationManager Validation { get { return ValidationManager.Instance; } }

        private Visibility _Visibility = Visibility.Visible;
        public Visibility IsVisibility
        {
            get { return _Visibility; }
            set { _Visibility = value; NotifyPropertyChanged(); }
        }


        private string _NameFile;
        public string NameFile
        {
            get { return _NameFile; }
            set { _NameFile = value; NotifyPropertyChanged(); }
        }

        private string _Description;
        public string Description
        {
            get { return _Description; }
            set { _Description = value; NotifyPropertyChanged(); }
        }

        private string _Author;
        public string Author
        {
            get { return _Author; }
            set { _Author = value; NotifyPropertyChanged(); }
        }

        private string _Project;
        public string Project
        {
            get { return _Project; }
            set { _Project = value; NotifyPropertyChanged(); }
        }

        private string _CostCenter;
        public string CostCenter
        {
            get { return _CostCenter; }
            set { _CostCenter = value; }
        }


        private I.ApprenticeServerDocument _Document;
        public I.ApprenticeServerDocument Document
        {
            get { return _Document; }
            set { _Document = value; }
        }

        public static int DeepMax2;

        public int Deep;
        public int GetWidthFromDeep => (30 + instanceProjectExplorer._DeepMax * 15) - Deep * 15;
        public string DeepS => Deep.ToString();
        public string FullPathName => Document.FullDocumentName;
        public I.DocumentTypeEnum DocumentType => Document.DocumentType;

        public List<DataI> ReferencedDataI = new();
        public ObservableCollection<DataI> drawingDocuments { get; set; } = new();
        public int GetNbDrawing => drawingDocuments.Count;

        private string _PartNumber;
        public string PartNumber
        {
            get { return _PartNumber; }
            set { _PartNumber = value; NotifyPropertyChanged(); }
        }

        public string Code2 { get { return NameFile.Substring(0, NameFile.LastIndexOf('.')); } }

        private CategoryType _Category;
        public CategoryType Category
        {
            get { return _Category; }
            set { _Category = value; NotifyPropertyChanged(); }
        }

        public BitmapImage bitmapImage;

        public string GetIconBase()
        {
            return DocumentType switch
            {
                Inventor.DocumentTypeEnum.kPartDocumentObject => "\uE71A",
                Inventor.DocumentTypeEnum.kAssemblyDocumentObject => "\uE74C",
                _ => throw new System.Exception("type de document inventor non défini")
            };
        }


        public DataI(I.ApprenticeServerDocument document, RecursiveType recursive = RecursiveType.True, int deep = 0)
        {
            Document = document;
            NameFile = document.DisplayName;
            Author = (string)document.PropertySets["Inventor Summary Information"].ItemByPropId[4].Value;
            Project = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[7].Value;
            Description = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[29].Value;
            PartNumber = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[5].Value;
            CostCenter = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[9].Value;
            Deep = deep;
            instanceProjectExplorer.DeepMax = deep > instanceProjectExplorer.DeepMax ? deep : instanceProjectExplorer.DeepMax;

            if (document.DocumentType == DocumentTypeEnum.kPartDocumentObject) return; // si c'est une pièce => pas de recherche d'enfant

            if (recursive == RecursiveType.True)
            {
                foreach (I.ApprenticeServerDocument referencedDocument in document.ReferencedDocuments)
                {
                    ReferencedDataI.Add(new DataI(referencedDocument, RecursiveType.True, deep + 1));
                }
            }
            if (recursive == RecursiveType.OneTime)
            {
                foreach (I.ApprenticeServerDocument referencedDocument in document.ReferencedDocuments)
                {
                    ReferencedDataI.Add(new DataI(referencedDocument, RecursiveType.False));
                }
            }
        }

        public void Update(List<string> drawingFileFound)
        {
            foreach (var drawing in drawingFileFound)
            {

            }
        }

        public string GetNbErrorMessage()
        {
            var sum = 0;
            foreach (var item in Validation.ValidationItems)
            {
                var resultValidation = item.CheckValidation(this);
                sum += resultValidation != ValidationItem.SeverityValidEnum.NoProblem ? 1 : 0;
            }
            return sum.ToString();
        }

        public MenuFlyout GetFlyoutDrawings()
        {
            var menuFlyout = new MenuFlyout() { Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom };
            foreach (var drawing in drawingDocuments)
            {
                var flyOutItem = new MenuFlyoutItem { Text = drawing.NameFile, Icon = new FontIcon { Glyph = "\uEC88" } };
                flyOutItem.Click += (object sender, RoutedEventArgs e) => InventorManagerHelper.GetActualInventorApp()?.Documents.Open(drawing.FullPathName);
                menuFlyout.Items.Add(flyOutItem);
            }
            if (menuFlyout.Items.Count == 0) { menuFlyout.Items.Add(new MenuFlyoutItem { Text = "Aucun plan !", Icon = new FontIcon { Glyph = "\uE783" } }); };
            return menuFlyout;
        }


        public IEnumerable<string> GetErrorsMessage()
        {
            foreach (var item in Validation.ValidationItems)
            {
                var resultValidation = item.CheckValidation(this);
                if (resultValidation != ValidationItem.SeverityValidEnum.NoProblem)
                {
                    yield return item.ErrorDescription;
                }
            }
        }

        public MenuFlyout GetFlyoutErrors()
        {
            var menuFlyout = new MenuFlyout() { Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom };
            foreach (var error in GetErrorsMessage())
                menuFlyout.Items.Add(new MenuFlyoutItem { Text = error, Icon = new FontIcon { Glyph = "\uE783" } });
            return menuFlyout;
        }

        public void RecursiveClearData()
        {
            foreach (var _ref in ReferencedDataI)
            {
                _ref.RecursiveClearData();
            }
            ReferencedDataI.Clear();
        }
    }
}
