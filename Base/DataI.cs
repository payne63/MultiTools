using ABI.Windows.UI;
using AvitechTools.Models;
using Inventor;
using Microsoft.Office.Interop.Outlook;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using SixLabors.ImageSharp;
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
        public enum CategoryType { Inconnu, Laser, Commerce, Profile, MecanoSoudure, Assemblage, ElementClient }

        #region Inotify_Business
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion 

        public ValidationManager Validation { get { return ValidationManager.Instance; } }

        private static I.ApprenticeServerComponent AppServer;

        private I.ApprenticeServerComponent GetAppServer
        {
            get
            {
                if (AppServer == null)
                {
                    AppServer = new ApprenticeServerComponent();
                }
                return AppServer;
            }
        }

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
        public string FullPathName ;
        public I.DocumentTypeEnum DocumentType;

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


        public DataI(string fullPathDocument, RecursiveType recursive = RecursiveType.True, int deep = 0)
        {
            I.ApprenticeServerDocument document = GetAppServer.Open(fullPathDocument);
            Document = document;
            NameFile = document.DisplayName;
            FullPathName = Document.FullDocumentName;
            DocumentType = Document.DocumentType;
            Author = (string)document.PropertySets["Inventor Summary Information"].ItemByPropId[4].Value;
            Project = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[7].Value;
            Description = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[29].Value;
            PartNumber = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[5].Value;
            CostCenter = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[9].Value;
            Deep = deep;
            instanceProjectExplorer.DeepMax = deep > instanceProjectExplorer.DeepMax ? deep : instanceProjectExplorer.DeepMax;
            
            Category = GetCategoryType();
            var referencedDocuments = document.ReferencedDocuments.Cast<ApprenticeServerDocument>().Select(rd => rd.FullFileName).ToList();
            GetAppServer.Close();

            if (recursive == RecursiveType.True)
            {
                foreach (var fullFileName in referencedDocuments)
                {
                    ReferencedDataI.Add(new DataI(fullFileName, RecursiveType.True, deep + 1));
                }
            }
            if (recursive == RecursiveType.OneTime)
            {
                foreach (var fullFileName in referencedDocuments)
                {
                    ReferencedDataI.Add(new DataI(fullFileName, RecursiveType.False));
                }
            }
        }

        public CategoryType GetCategoryType()
        {
            if (FullPathName.IndexOf("composants", StringComparison.OrdinalIgnoreCase) >= 0) return CategoryType.Commerce;
            if (FullPathName.IndexOf("Elements client", StringComparison.OrdinalIgnoreCase) >= 0) return CategoryType.ElementClient;
            if (Description.IndexOf("laser", StringComparison.OrdinalIgnoreCase) >= 0) return CategoryType.Laser;
            if (ReferencedDataI.Count > 0)
            {
                if (ReferencedDataI.First().PartNumber[0..7] == PartNumber[0..7])
                    return CategoryType.MecanoSoudure;
                else return CategoryType.Assemblage;
            }
            return CategoryType.Inconnu;
        }

        public string GetNbErrorMessage()
        {
            var sum = 0;
            Trace.WriteLine($"errormessageCheck => {Category}");
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


        public IEnumerable<string> GetErrorsDescriptionMessage()
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

        public IEnumerable<string> GetErrorsMessage()
        {
            foreach (var item in Validation.ValidationItems)
            {
                var resultValidation = item.CheckValidation(this);
                if (resultValidation != ValidationItem.SeverityValidEnum.NoProblem)
                {
                    yield return item.Description;
                }
            }
        }

        public MenuFlyout GetFlyoutErrors()
        {
            var menuFlyout = new MenuFlyout() { Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom };
            foreach (var error in GetErrorsDescriptionMessage())
                menuFlyout.Items.Add(new MenuFlyoutItem { Text = error, Icon = new FontIcon { Glyph = "\uE783" } });
            if (menuFlyout.Items.Count == 0) menuFlyout.Items.Add(new MenuFlyoutItem { Text = "Pas d'erreur", Icon = new FontIcon { Glyph = "\uE8E1" } });
            return menuFlyout;
        }

        public Brush GetErrorColorBrush()
        {
            return GetNbErrorMessage() switch
            {
                "0" => App.Current.Resources["SystemFillColorSuccessBrush"] as SolidColorBrush,
                "1" => App.Current.Resources["SystemFillColorCautionBrush"] as SolidColorBrush,
                _ => App.Current.Resources["SystemFillColorCriticalBrush"] as SolidColorBrush,
            };
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
