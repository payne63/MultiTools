using Inventor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiTools.Base.ValidationItem;

namespace MultiTools.Base
{
    public class ValidationManager
    {
        public static ValidationManager Instance = new ValidationManager();
        public List<ValidationItem> ValidationItems = new();

        public ValidationManager()
        {
            ValidationItems.Add(new ValidationItem("Absence Nom Dessinateur ?",
                "Nom du dessinateur non renseigné",
                (dataI) => { return dataI.Author == string.Empty ? ValidationItem.SeverityValidEnum.Low : ValidationItem.SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Absence Description ?",
                "Description non renseignée",
                (dataI) => { return dataI.Description == string.Empty ? ValidationItem.SeverityValidEnum.Low : ValidationItem.SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Absence Nom Client ?",
                "Client non renseigné",
                (dataI) => { return dataI.CostCenter == string.Empty ? ValidationItem.SeverityValidEnum.Low : ValidationItem.SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Absence Nom projet?",
                "Projet non renseigné",
                (dataI) => { return dataI.Project == string.Empty ? ValidationItem.SeverityValidEnum.Low : ValidationItem.SeverityValidEnum.NoProblem; }));

            ValidationItems.Add(new ValidationItem("Absence plan ?",
                "Aucun Plan",
                (dataI) =>
                {
                    if (dataI.Category == DataI.CategoryType.Commerce || dataI.Category == DataI.CategoryType.ElementClient)
                        return ValidationItem.SeverityValidEnum.NoProblem;
                    return dataI.drawingDocuments.Count == 0 ? ValidationItem.SeverityValidEnum.Medium : ValidationItem.SeverityValidEnum.NoProblem;
                }));

            ValidationItems.Add(new ValidationItem("Code = Nom du fichier ?",
                "Code pièce différent du nom de fichier",
                (dataI) => { return dataI.PartNumber != System.IO.Path.GetFileNameWithoutExtension(dataI.FullPathName) ? ValidationItem.SeverityValidEnum.High : ValidationItem.SeverityValidEnum.NoProblem; }));
        }
    }
}
