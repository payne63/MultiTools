using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using I = Inventor;

namespace MultiTools.Base;

public abstract class DataIBase : INotifyPropertyChanged
{
    #region InotifyPropertyChange
    public event PropertyChangedEventHandler PropertyChanged;

    internal void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
    public enum CategoryType { Assemblage, MecanoSoudure, Mecanique, Profile, Laser, Commerce, ElementClient, Inconnu, }

    private FileInfo fileInfo;
    public FileInfo FileInfoData {
        get
        {
            if (fileInfo == null)
            {
                fileInfo = new FileInfo(FullPathName);
                if (!fileInfo.Exists) { throw new Exception($"File {FullPathName} doesn't exist "); }
            }
            return fileInfo;
        } 
    }
    public BitmapImage bitmapImage;

    private static I.ApprenticeServerComponent AppServer;

    internal static I.ApprenticeServerComponent GetAppServer
    {
        get
        {
            if (AppServer == null) AppServer = new I.ApprenticeServerComponent();
            return AppServer;
        }
    }

    private string _NameFile;
    public string NameFile
    {
        get { return _NameFile; }
        set { _NameFile = value; NotifyPropertyChanged(); }
    }

    public string _FullPathName;
    public string FullPathName
    {
        get { return _FullPathName; }
        set { _FullPathName = value; NotifyPropertyChanged(); }
    }

    private string _Description;
    public string Description
    {
        get { return _Description; }
        set { _Description = value; NotifyPropertyChanged(); }
    }

    private string _PartNumber;
    public string PartNumber
    {
        get { return _PartNumber; }
        set { _PartNumber = value; NotifyPropertyChanged(); }
    }

    private CategoryType _Category;
    public CategoryType Category
    {
        get { return _Category; }
        set { _Category = value; NotifyPropertyChanged(); }
    }

    public I.DocumentTypeEnum DocumentType;

    private I.ApprenticeServerDocument _Document;
    public I.ApprenticeServerDocument Document
    {
        get { return _Document; }
        set { _Document = value; }
    }
}