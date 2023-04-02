using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using I = Inventor;
using System.IO;
using SplittableDataGridSAmple.Base;
using SplittableDataGridSAmple.Elements;
using SplittableDataGridSAmple.Tabs;

namespace WindowsFormsTest
{
    public class DataManager
    {
        public static ProjectExplorerTab projectExplorerElements { get; set; }
        I.ApprenticeServerComponent appServer = new I.ApprenticeServerComponent();
        public List<Data> RootData { get; set; }
        string[] drawingsFind;
        Dictionary<string, string> DictionnaryConversionPath = new Dictionary<string, string>();

        public DataManager(string fileSelectedForRoot, ProjectExplorerTab _projectExplorerElements)
        {
            projectExplorerElements = _projectExplorerElements;
            RootData = new List<Data>() { CreateData(fileSelectedForRoot) };
            drawingsFind = Directory.GetFiles(System.IO.Path.GetDirectoryName(RootData.First().FullPathName),
                                                "*.idw", SearchOption.AllDirectories)
                .ToList().Where(x => !x.Contains("OldVersions")).ToArray();
            RecursiveDataCollect(RootData.First());
            EventFillData();
        }

        ~DataManager()
        {
            RootData.Clear();
            appServer.Close();
        }

        public Data CreateData(string file)
        {
            //Debug.WriteLine($"file:  {file}");
            if (!System.IO.File.Exists(file)) throw new Exception($"le fichier {file} n'existe pas");

            I.ApprenticeServerDocument inventorDocument = appServer.Open(file);
            //I.ApprenticeServerDrawingDocument a = appServer.Open(file) as I.ApprenticeServerDrawingDocument;
            //if (a == null) throw new Exception($"impossible d'ouvrir le fichier {file}");
            //var th = a.Sheets;
            if (inventorDocument == null) return null;

            I.PropertySet prop = inventorDocument.PropertySets["Design Tracking Properties"];
            I.Property material = prop["Material"];

            Data data = new Data()
            {

                NameFile = inventorDocument.DisplayName,//System.IO.Path.GetFileName(file),
                Author = (string)inventorDocument.PropertySets["Inventor Summary Information"].ItemByPropId[4].Value,
                Project = (string)inventorDocument.PropertySets["Design Tracking Properties"].ItemByPropId[7].Value,
                ReferenceDocument = ConvertReference(inventorDocument),
                FullPathName = inventorDocument.FullDocumentName,
                DocumentType = inventorDocument.DocumentType,
                //thumbnail = doc.PropertySets["Inventor Summary Information"].ItemByPropId[17].Value
            };
            //data.SetImage();
            inventorDocument.Close();

            return data;
        }


        private static List<string> ConvertReference(I.ApprenticeServerDocument appSerDoc)
        {
            List<string> list = new List<string>();
            foreach (I.ApprenticeServerDocument item in appSerDoc.ReferencedDocuments)
            {
                list.Add(item.FullDocumentName);
            }

            return list;
        }


        private void RecursiveDataCollect(Data data)
        {
            if (data == null) return;
            if (System.IO.Path.GetExtension(data.NameFile) == ".idw") return;

            foreach (string referenceFile in data.ReferenceDocument)
            {
                //Debug.WriteLine($"referenceFile: {referenceFile}");
                data.Children.Add(CreateData(referenceFile));
            }
            foreach (string drawingFile in drawingsFind)
            {
                if (drawingFile.Contains(System.IO.Path.GetFileNameWithoutExtension(data.NameFile)))
                {
                    data.Children.Add(CreateData(drawingFile));
                    //Debug.WriteLine($"data: {data.NameFile}  drawing: {drawingFile}");
                }
            }

            foreach (Data dataChildren in data.Children)
            {
                RecursiveDataCollect(dataChildren);
            }
        }
        public void ClearData() => RecursiveClearData(RootData.First());
        private void RecursiveClearData(Data data)
        {
            foreach (Data dataChildren in data.Children)
            {
                RecursiveClearData(dataChildren);
            }
            data.Children.Clear();
        }

        public void EventFillData() => RecursiveEventFillData(RootData.First());
        private void RecursiveEventFillData(Data data)
        {
            foreach (Data dataChildren in data.Children)
            {
                RecursiveEventFillData(dataChildren);
            }
            data.IsSelectedClick += projectExplorerElements.UpdateSelectionElement;
        }

        public void PrepareNewData(string newProjetCode, string newBasePath, string newProjetName, string newAuthor)
        {
            List<Data> ListDataToChange = GetAlldata();
            string oldProjetCode = ListDataToChange.Last().NameFile.Substring(0, 5);
            string basePath = System.IO.Path.GetDirectoryName(ListDataToChange.Last().FullPathName);
            foreach (Data data in ListDataToChange)
            {
                data.NewNameFile = data.NameFile.Replace(oldProjetCode, newProjetCode);
                data.NewFullPathName = data.FullPathName.Replace(basePath, newBasePath).Replace(oldProjetCode, newProjetCode);
                data.Project = newProjetName;
                data.Author = newAuthor;

                if (!DictionnaryConversionPath.ContainsKey(data.FullPathName))
                {
                    DictionnaryConversionPath.Add(data.FullPathName, data.NewFullPathName);
                }
            }
        }

        public void StartCopie()
        {

            List<Data> ListDataToChange = GetAlldata();
            ListDataToChange.Sort();
            foreach (Data data in ListDataToChange)
            {
                I.ApprenticeServerDocument doc = appServer.Open(data.FullPathName);
                I.FileSaveAs fileSaveAs = appServer.FileSaveAs;
                if (doc == null) throw new Exception("impossible d'ouvrir le ficher");
                doc.PropertySets["Inventor Summary Information"].ItemByPropId[4].Value = data.Author;           //auteur
                doc.PropertySets["Design Tracking Properties"].ItemByPropId[7].Value = data.Project;            //Nom du projet
                doc.PropertySets["Inventor Document Summary Information"].ItemByPropId[15].Value = "AVITECH";   //nom de l'entreprise
                doc.PropertySets["Inventor Summary Information"].ItemByPropId[6].Value =
                    $"recodification automatique du projet ";                                                   // annotation
                doc.PropertySets["Design Tracking Properties"].ItemByPropId[5].Value = System.IO.Path.GetFileNameWithoutExtension(data.NewNameFile);

                foreach (I.DocumentDescriptor item in doc.ReferencedDocumentDescriptors)
                {
                    if (DictionnaryConversionPath.ContainsKey(item.ReferencedFileDescriptor.FullFileName))
                    {
                        string replaceName = DictionnaryConversionPath[item.ReferencedFileDescriptor.FullFileName];

                        // le fichier doit exister pour pouvoir changer le référence
                        item.ReferencedFileDescriptor.ReplaceReference(replaceName);
                    }
                }

                if (!Directory.Exists(System.IO.Path.GetDirectoryName(data.NewFullPathName))) // si le répertoire n'existe pas, on le créer
                {
                    Directory.CreateDirectory(data.NewFullPathName);
                }

                fileSaveAs.AddFileToSave(doc, data.NewFullPathName);
                fileSaveAs.ExecuteSaveCopyAs();
                doc.Close();
            }
            //MessageBox.Show("Clone terminé!");
        }


        /// <summary>
        /// Return la liste complete des items dans une seule liste
        /// </summary>
        /// <returns></returns>
        public List<Data> GetAlldata()
        {
            List<Data> listAllData = new List<Data>();
            RecursiveGetData(RootData.First(), listAllData);
            //foreach (Data data in listAllData)
            //{
            //    Debug.WriteLine(data.FullPathName);
            //}

            return listAllData;
        }

        private void RecursiveGetData(Data data, List<Data> list)
        {
            foreach (Data dataChildren in data.Children)
            {
                RecursiveGetData(dataChildren, list);
            }
            list.Add(data);
        }


    }
}

