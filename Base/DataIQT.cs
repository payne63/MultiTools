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

namespace SplittableDataGridSAmple.Base
{
    class DataIQT : DataIBase
    {

        public List<(string FullFileName, string DisplayName)> ReferencedDocuments { get; set; }

        private int _Qt;
        public int Qt
        {
            get { return _Qt; }
            set { _Qt = value; }
        }

        private DataIQT(string fullPathDocument)
        {
            I.ApprenticeServerDocument document = GetAppServer.Open(fullPathDocument);
            Document = document;
            NameFile = document.DisplayName;
            FullPathName = Document.FullDocumentName;
            DocumentType = Document.DocumentType;
            Description = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[29].Value;
            PartNumber = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[5].Value;
            //if (DocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
            {
                ReferencedDocuments = document.ReferencedDocuments.Cast<ApprenticeServerDocument>().Select(rd => (rd.FullFileName, rd.DisplayName)).ToList();
            }
            Category = GetCategoryType();

            GetAppServer.Close();
        }
        public static List<DataIQT> GetDatadIQT(string fullPathDocumentRoot)
        {
            var result = new List<DataIQT>();
            RecursiveGetDataIQT(fullPathDocumentRoot, result);
            return result.DistinctBy(data => data.FullPathName).ToList();
        }

        private static void RecursiveGetDataIQT(string fullPathDocumentRoot, List<DataIQT> result)
        {
            var dataIQT = new DataIQT(fullPathDocumentRoot);
            result.Add(dataIQT);
            //if (dataIQT.DocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
            {
                foreach (var childData in dataIQT.ReferencedDocuments)
                {
                    RecursiveGetDataIQT(childData.FullFileName, result);
                }
            }
        }

        private CategoryType GetCategoryType()
        {
            if (FullPathName.IndexOf("composants", StringComparison.OrdinalIgnoreCase) >= 0) return CategoryType.Commerce;
            if (FullPathName.IndexOf("Elements client", StringComparison.OrdinalIgnoreCase) >= 0) return CategoryType.ElementClient;
            if (Description.IndexOf("laser", StringComparison.OrdinalIgnoreCase) >= 0) return CategoryType.Laser;
            if (Description.IndexOf("#M", StringComparison.OrdinalIgnoreCase) >= 0) return CategoryType.Mecanique;
            if (Description.IndexOf("#P", StringComparison.OrdinalIgnoreCase) >= 0) return CategoryType.Profile;
            if (ReferencedDocuments.Count > 0 && DocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
            {
                if (ReferencedDocuments.First().DisplayName[0..7] == PartNumber[0..7])
                    return CategoryType.MecanoSoudure;
                else return CategoryType.Assemblage;
            }
            return CategoryType.Inconnu;
        }
    }
}
