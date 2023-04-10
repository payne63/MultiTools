using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using I = Inventor;

namespace SplittableDataGridSAmple.Base
{
    static class QtManager
    {

        public static void AssignQtToPart(List<DataIQT> dataIQTs)
        {
            if (dataIQTs.Count == 0) return;
            var rootDocument = dataIQTs.First();
            rootDocument.Qt = 1;

            RecursiveAssignQtToPart(dataIQTs, rootDocument.FullPathName);


        }

        private static void RecursiveAssignQtToPart(List<DataIQT> dataIQTs, string fullPathName)
        {
            var CurrentData = dataIQTs.FirstOrDefault(x => x.FullPathName == fullPathName);
            if (CurrentData == null) throw new Exception("current Data non exist");
            if (CurrentData.DocumentType != DocumentTypeEnum.kAssemblyDocumentObject) return;

            I.ApprenticeServerDocument document = DataIBase.GetAppServer.Open(CurrentData.FullPathName);

            AssemblyComponentDefinition ass = document.ComponentDefinition as AssemblyComponentDefinition; //convertion en assemblage
            var Bom = ass.BOM;
            var obomViewPartOnly = Bom.BOMViews[1];// 1 - bom standard - 2 structured - 3 part only (2 et 3 need activation)
            foreach (BOMRow bomrowPartOnly in obomViewPartOnly.BOMRows)
            {
                var DisplayName = ((ApprenticeServerDocument)((ComponentDefinition)bomrowPartOnly.ComponentDefinitions[1]).Document).FullDocumentName;
                var qt = bomrowPartOnly.TotalQuantity;
                Trace.WriteLine("ItemNumber :" + DisplayName);
                Trace.WriteLine("totalQuantity :" + qt);
                var identityData = dataIQTs.FirstOrDefault(x => x.FullPathName == DisplayName);
                if (identityData == null) throw new Exception("pas de correspondance enfant");
                identityData.Qt += int.Parse(qt);
            }
            DataIBase.GetAppServer.Close();

            foreach (var dataChild in CurrentData.ReferencedDocuments)
            {
                RecursiveAssignQtToPart(dataIQTs, dataChild.FullFileName);
            }

        }

    }
}
