using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Microsoft.UI.Xaml.Media.Imaging;
using I=Inventor;

namespace SplittableDataGridSAmple.Base;

public class DataIProp : DataIBase
{

    public List<string> bom = new();
    
    private string _customerName;

    public string CustomerName
    {
        get
        {
            return _customerName;
        }
        set
        {
            _customerName = value;
            NotifyPropertyChanged();
        }
    }

    private string _ProjectName;

    public string ProjectName
    {
        get
        {
            return _ProjectName;
        }
        set
        {
            _ProjectName = value;
            NotifyPropertyChanged();
        }
    }

    private string _AuthorName;

    public string AuthorName
    {
        get
        {
            return _AuthorName;
        }
        set
        {
            _AuthorName = value;
            NotifyPropertyChanged();
        }
    }
    
    private BitmapImage _BitmapImage;

    public BitmapImage BitmapImage
    {
        get { return _BitmapImage; }
        set { _BitmapImage = value; NotifyPropertyChanged(); }
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

    public DataIProp(string fullPathDocument)
    {
        I.ApprenticeServerDocument document = GetAppServer.Open(fullPathDocument);
        Document = document;
        NameFile = document.DisplayName;
        FullPathName = Document.FullFileName;
        DocumentType = Document.DocumentType;
        Description = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[29].Value;
        PartNumber = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[5].Value;
        CustomerName = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[9].Value;
        ProjectName = (string)document.PropertySets["Design Tracking Properties"].ItemByPropId[7].Value;
        AuthorName = (string)document.PropertySets["Inventor Summary Information"].ItemByPropId[4].Value;
        
        if (DocumentType == I.DocumentTypeEnum.kAssemblyDocumentObject)
        {
            I.AssemblyComponentDefinition
                ass = document.ComponentDefinition as I.AssemblyComponentDefinition; //convertion en assemblage
            
            foreach (I.BOMRow row in
                     ass.BOM.BOMViews[1]
                         .BOMRows) // 1 - bom standard - 2 structured - 3 part only (2 et 3 need activation)
            {
                if (row.BOMStructure == I.BOMStructureEnum.kPhantomBOMStructure ||
                    row.BOMStructure == I.BOMStructureEnum.kReferenceBOMStructure) continue;
                try
                {
                    // var a = row.ComponentDefinitions[1];
                    var FullDocumentName = ((I.ApprenticeServerDocument)row.ComponentDefinitions[1].Document).FullDocumentName;
                    bom.Add(FullDocumentName);
                }
                catch (Exception)
                {
                    throw new Exception("Assemblage avec des liens rompus!! Corriger les liens");
                }
            }
        }
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