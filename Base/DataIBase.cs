using System.ComponentModel;
using System.Runtime.CompilerServices;
using I=Inventor;

namespace SplittableDataGridSAmple
{
    public abstract class DataIBase :INotifyPropertyChanged
    {
        #region InotifyPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;
        internal void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public enum CategoryType { Inconnu, Laser, Commerce, Profile, MecanoSoudure, Assemblage, ElementClient, Mecanique }

        private static I.ApprenticeServerComponent AppServer;

        static internal I.ApprenticeServerComponent GetAppServer
        {   get
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
}