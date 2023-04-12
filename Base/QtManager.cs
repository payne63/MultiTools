using Inventor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            RecursiveAssignQtToPart(dataIQTs, rootDocument.FullPathName, rootDocument.Qt);
        }

        private static void RecursiveAssignQtToPart(List<DataIQT> dataIQTs, string fullPathName, int qtParent)
        {
            var CurrentData = dataIQTs.FirstOrDefault(x => x.FullPathName == fullPathName);
            if (CurrentData == null) throw new Exception("current Data non exist");
            if (CurrentData.DocumentType != DocumentTypeEnum.kAssemblyDocumentObject) return;

            I.ApprenticeServerDocument document = DataIBase.GetAppServer.Open(CurrentData.FullPathName);
            Trace.WriteLine($"Parent {System.IO.Path.GetFileName(fullPathName)}");
            AssemblyComponentDefinition ass = document.ComponentDefinition as AssemblyComponentDefinition; //convertion en assemblage
            var Bom = ass.BOM;
            var obomViewStandard = Bom.BOMViews[1];// 1 - bom standard - 2 structured - 3 part only (2 et 3 need activation)
            var BomData = new List<(string FullDisplayName,DataIQT identityData)>();
            foreach (BOMRow row in obomViewStandard.BOMRows)
            {
                if (row.BOMStructure == BOMStructureEnum.kPhantomBOMStructure || row.BOMStructure == BOMStructureEnum.kReferenceBOMStructure) continue;
                var FullDisplayName = ((ApprenticeServerDocument)(row.ComponentDefinitions[1]).Document).FullDocumentName;
                var identityData = dataIQTs.FirstOrDefault(x => x.FullPathName == FullDisplayName);
                if (identityData == null) continue;
                identityData.Qt += int.Parse(row.TotalQuantity) * qtParent;
                BomData.Add((FullDisplayName,identityData));
            }
            
            DataIBase.GetAppServer.Close();

            foreach (var child in BomData)
            {
                RecursiveAssignQtToPart(dataIQTs, child.FullDisplayName, child.identityData.Qt);
            }

        }

    }
}
