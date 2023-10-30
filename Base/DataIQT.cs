using EnvDTE;
using Inventor;
using RtfPipe.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SplittableDataGridSAmple.Base.DataI;
using System.Xml.Linq;
using I = Inventor;
using System.Diagnostics;
using System.IO;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Drawing;
using System.ComponentModel;

namespace SplittableDataGridSAmple.Base
{
    public class DataIQT : DataIBase
    {

        public List<(string FullFileName, string DisplayName)> ReferencedDocuments { get; set; }

        public List<(string fullFileName, int Qt)> bom = new();

        private int _Qt;
        public int Qt
        {
            get { return _Qt; }
            set { _Qt = value; NotifyPropertyChanged(); }
        }
        private string _Material;
        public string Material
        {
            get { return _Material; }
            set { _Material = value; NotifyPropertyChanged(); }
        }
        private BitmapImage _BitmapImage;

        public BitmapImage BitmapImage
        {
            get { return _BitmapImage; }
            set { _BitmapImage = value; NotifyPropertyChanged(); }
        }

        public IList<CategoryType> GetCategoryTypes => Enum.GetValues(typeof(CategoryType)).Cast<CategoryType>().ToList();

        public DataIQT(string fullPathDocument, int qt)
        {
            I.ApprenticeServerDocument document = GetAppServer.Open(fullPathDocument);
            Document = document;
            NameFile = document.DisplayName; 
            FullPathName = Document.FullFileName;
            DocumentType = Document.DocumentType;
            Description = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[29].Value;
            PartNumber = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[5].Value;
            Material = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[20].Value;
            Qt = qt;
            if (IsCommerceType) { Category = CategoryType.Commerce; GetAppServer.Close(); return; };
            if (IsElementClientType) { Category = CategoryType.ElementClient; GetAppServer.Close(); return; };
            if (IsLaserType) { Category = CategoryType.Laser; GetAppServer.Close(); return; };
            if (IsProfileType) { Category = CategoryType.Profile; GetAppServer.Close(); return; };
            if (IsMecaniqueType) { Category = CategoryType.Mecanique; GetAppServer.Close(); return; };

            if (DocumentType == DocumentTypeEnum.kAssemblyDocumentObject) 
            {
                AssemblyComponentDefinition ass = document.ComponentDefinition as AssemblyComponentDefinition; //convertion en assemblage
                var Bom = ass.BOM;
                var obomViewStandard = Bom.BOMViews[1];// 1 - bom standard - 2 structured - 3 part only (2 et 3 need activation)
                foreach (BOMRow row in obomViewStandard.BOMRows)
                {
                    if (row.BOMStructure == BOMStructureEnum.kPhantomBOMStructure || row.BOMStructure == BOMStructureEnum.kReferenceBOMStructure) continue;
                    var FullDocumentName = ((ApprenticeServerDocument)(row.ComponentDefinitions[1]).Document).FullDocumentName;
                    var qtPart = int.Parse(row.TotalQuantity);
                    bom.Add((FullDocumentName, qtPart));
                }
                if (bom.Count == 0) { return; } // no children?
                if (bom[0].fullFileName.Length >=8 && PartNumber.Length >=8)
                {
                    if (System.IO.Path.GetFileNameWithoutExtension( bom[0].fullFileName)[0..7] == PartNumber[0..7]) { Category = CategoryType.MecanoSoudure; GetAppServer.Close(); return; }
                }
                Category = CategoryType.Assemblage;
                GetAppServer.Close();
                return;
            }

            Category = CategoryType.Inconnu;
            GetAppServer.Close();
        }

        private bool IsCommerceType => FullPathName.IndexOf("composants", StringComparison.OrdinalIgnoreCase) >= 0;
        private bool IsElementClientType => FullPathName.IndexOf("Elements client", StringComparison.OrdinalIgnoreCase) >= 0;
        private bool IsLaserType => Description.IndexOf("laser", StringComparison.OrdinalIgnoreCase) >= 0;
        private bool IsMecaniqueType => Description.IndexOf("#M", StringComparison.OrdinalIgnoreCase) >= 0;
        private bool IsProfileType => Description.IndexOf("#P", StringComparison.OrdinalIgnoreCase) >= 0;

        public override string ToString()
        {
            return $"name:{NameFile} description:{Description} qt:{Qt} nbChild:{ReferencedDocuments.Count}";
        }

        public async Task UpdateThumbnail()
        {
            if (BitmapImage != null) return;
            if (FullPathName == string.Empty) return;
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(FullPathName);
            var iconThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 256);
            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(iconThumbnail);
            if (bitmapImage != null)
            {
                //return bitmapImage;
                BitmapImage = bitmapImage;
            }
        }

    }
}
