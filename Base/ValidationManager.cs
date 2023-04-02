using Inventor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SplittableDataGridSAmple.Base.ValidationItem;

namespace SplittableDataGridSAmple.Base
{
    public class ValidationManager
    {
        public static ValidationManager Instance = new ValidationManager();
        public ObservableCollection<ValidationItem> ValidationItems = new();

        public ValidationManager()
        {
            ValidationItems.Add( new ValidationItem("Aucun Filtre",
                string.Empty,
                (dataI)=> SeverityValidEnum.NoProblem ));

            ValidationItems.Add(new ValidationItem("Absence Nom Dessinateur ?",
                "Nom du dessinateur non renseigné",
                (dataI) => { return dataI.Author == string.Empty ? SeverityValidEnum.Low : SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Absence Description ?",
                "Description non renseignée",
                (dataI) => { return dataI.Description == string.Empty ? SeverityValidEnum.Low : SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Absence Nom Client ?",
                "Client non renseigné",
                (dataI) => { return dataI.CostCenter == string.Empty ? SeverityValidEnum.Low : SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Absence Nom projet?",
                "Projet non renseigné",
                (dataI) => { return dataI.Project == string.Empty ? SeverityValidEnum.Low : SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Absence plan ?",
                "Aucun Plan",
                (dataI) => { return dataI.drawingDocuments.Count == 0 ? SeverityValidEnum.Medium : SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Code = Nom du fichier ?",
                "Code pièce différent du nom de fichier",
                (dataI) => { return dataI.PartNumber != System.IO.Path.GetFileNameWithoutExtension(dataI.FullPathName)  ? SeverityValidEnum.High : SeverityValidEnum.NoProblem; }));
        }
    }
}
