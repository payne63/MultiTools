using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using I = Inventor;

namespace MultiTools.Base;
public class DataIClean : DataIBase, IEqualityComparer<DataIClean>
{
    private string relativeFolderPath;
    public string RelativeFolderPath
    {
        get => relativeFolderPath;
        set
        {
            relativeFolderPath = value; NotifyPropertyChanged();
        }
    }
    public DataIClean(string fullPathDocument, string PathRootDocument)
    {
        I.ApprenticeServerDocument document = GetAppServer.Open(fullPathDocument);
        Description = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[29].Value;
        PartNumber = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[5].Value;
        FullPathName = document.FullDocumentName;
        DocumentType = document.DocumentType;
        var rootPath = Path.GetPathRoot(PathRootDocument );
        var pathDocument = Path.GetPathRoot(fullPathDocument);

        relativeFolderPath = pathDocument.Replace(rootPath, "");

        document.Close();
        GetAppServer.Close();
    }

    public bool Equals(DataIClean x, DataIClean y)
    {
        if (x == null || y == null) return false;
        return x.FullPathName == y.FullPathName;
    }
    public int GetHashCode([DisallowNull] DataIClean obj)
    {
        return obj.FullPathName.GetHashCode();
    }
}
