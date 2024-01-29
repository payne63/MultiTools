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
using IO = System.IO;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using System.Drawing;
using System.ComponentModel;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace SplittableDataGridSAmple.Base
{
    public class DataIQT : DataIBase
    {
        private readonly string CLSID_InventorSheetMetalPart_RegGUID = "9C464203-9BAE-11D3-8BAD-0060B0CE6BB4";
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

        private bool _trueSheetMetal;

        public bool IsTrueSheetMetal
        {
            get => _trueSheetMetal;
            set => _trueSheetMetal = value;
        }

        private string _Status = string.Empty;

        public string Status
        {
            get => _Status;
            set
            {
                _Status = value; NotifyPropertyChanged();
            }
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

            if (DocumentType == DocumentTypeEnum.kPartDocumentObject)
            {
                if (document.ComponentDefinition is SheetMetalComponentDefinition sheetMetalComponentDefinition) IsTrueSheetMetal = true;
            }

            if (IsCommerceType) { Category = CategoryType.Commerce; GetAppServer.Close(); return; };
            if (IsElementClientType) { Category = CategoryType.ElementClient; GetAppServer.Close(); return; };
            if (IsLaserType) { Category = CategoryType.Laser; GetAppServer.Close(); return; };
            if (IsProfileType) { Category = CategoryType.Profile; GetAppServer.Close(); return; };
            if (IsMecaniqueType) { Category = CategoryType.Mecanique; GetAppServer.Close(); return; };



            if (DocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
            {
                AssemblyComponentDefinition ass = document.ComponentDefinition as AssemblyComponentDefinition; //convertion en assemblage

                foreach (BOMRow row in ass.BOM.BOMViews[1].BOMRows)// 1 - bom standard - 2 structured - 3 part only (2 et 3 need activation)
                {
                    if (row.BOMStructure == BOMStructureEnum.kPhantomBOMStructure || row.BOMStructure == BOMStructureEnum.kReferenceBOMStructure) continue;
                    try
                    {
                        var a = row.ComponentDefinitions[1];
                        var FullDocumentName = ((ApprenticeServerDocument)(row.ComponentDefinitions[1]).Document).FullDocumentName;
                        var qtPart = int.Parse(row.TotalQuantity);
                        bom.Add((FullDocumentName, qtPart));
                    }
                    catch (Exception)
                    {
                        throw new Exception("Assemblage avec des liens rompus!! Corriger les liens");
                    }

                }
                if (bom.Count == 0) { return; } // no children?

                var childrens = bom
                    .Select(x => IO.Path.GetFileNameWithoutExtension(x.fullFileName))
                    .Where(x => x.Count() >= 8);
                if (childrens.Any(x => x[0..7] == PartNumber[0..7]))
                {
                    Category = CategoryType.MecanoSoudure;
                    return;
                }

                Category = CategoryType.Assemblage;
                //GetAppServer.Close();
                return;
            }

            Category = CategoryType.Inconnu;
            //GetAppServer.Close();
        }

        private bool IsCommerceType => FullPathName.IndexOf("composants", StringComparison.OrdinalIgnoreCase) >= 0;
        private bool IsElementClientType => FullPathName.IndexOf("Elements client", StringComparison.OrdinalIgnoreCase) >= 0;
        public bool IsLaserType => Description.IndexOf("laser", StringComparison.OrdinalIgnoreCase) >= 0;
        private bool IsMecaniqueType => Description.IndexOf("#M", StringComparison.OrdinalIgnoreCase) >= 0;
        private bool IsProfileType => Description.IndexOf("#P", StringComparison.OrdinalIgnoreCase) >= 0;

        public override string ToString() => $"name:{NameFile} description:{Description} qt:{Qt} nbChild:{ReferencedDocuments.Count}";

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
