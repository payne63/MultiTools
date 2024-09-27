using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace MultiTools.Models
{
    public class IDWModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public FileInfo FileInfoData { get; }
        public bool IsLaser => Name.EndsWith("L.idw");
        public bool IsDrawing => Name.EndsWith(".idw");
        public string Name => FileInfoData.Name;

        public BitmapImage bitmapImage;

        private bool _MakePDF;
        public bool MakePDF
        {
            get { return _MakePDF; }
            set { _MakePDF = value; OnPropertyChanged(); }
        }

        private bool _MakeDXF;
        public bool MakeDXF
        {
            get { return _MakeDXF; }
            set { _MakeDXF = value; OnPropertyChanged(); }
        }

        public IDWModel(FileInfo fileInfo,PropertyChangedEventHandler propertyChangedEventHandler)
        {
            FileInfoData = fileInfo;
            AutoSelectPDFStatus();
            AutoSelectDXFStatus();
            PropertyChanged += propertyChangedEventHandler;
        }

        public void AutoSelectPDFStatus() => MakePDF = IsDrawing;
        public void AutoSelectDXFStatus() => MakeDXF = IsLaser;


    }
}
