using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using I = Inventor;
using Inventor;
using Microsoft.UI.Xaml.Media.Imaging;

namespace SplittableDataGridSAmple.Models
{
    public class IDWPrintModel : INotifyPropertyChanged
    {
        private static I.ApprenticeServerComponent appServer = new I.ApprenticeServerComponent();
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public BitmapImage bitmapImage;

        public FileInfo FileInfoData { get; }

        private bool _IsPrint;
        public bool IsPrint
        {
            get { return _IsPrint; }
            set { _IsPrint = value; OnPropertyChanged(); }
        }

        private bool _IsDrawing;
        public bool IsDrawing
        {
            get { return _IsDrawing; }
            set { _IsDrawing = value; OnPropertyChanged(); }
        }

        private DrawingSheetSizeEnum _SheetSize;
        public DrawingSheetSizeEnum SheetSize
        {
            get { return _SheetSize; }
            set { _SheetSize = value; OnPropertyChanged(); }
        }

        private PageOrientationTypeEnum _Orientation;
        public PageOrientationTypeEnum Orientation
        {
            get { return _Orientation; }
            set { _Orientation = value; OnPropertyChanged(); }
        }

        private int _PageCount;
        public int PageCount
        {
            get { return _PageCount; }
            set { _PageCount = value; OnPropertyChanged(); }
        }


        public string Name => FileInfoData.Name;

        public IDWPrintModel(FileInfo fileInfo, PropertyChangedEventHandler propertyChangedEventHandler)
        {
            //if (appServer == null) { I.ApprenticeServerComponent appServer = new I.ApprenticeServerComponent(); }
            var drawingDocument = appServer.Open(fileInfo.FullName) as I.ApprenticeServerDrawingDocument;
            SheetSize = drawingDocument.Sheets[1].Size;
            Orientation = drawingDocument.Sheets[1].Orientation;
            PageCount = drawingDocument.Sheets.Count;
            FileInfoData = fileInfo;
            AutoSelectPrint();
            PropertyChanged += propertyChangedEventHandler;
        }

        public void AutoSelectPrint()
        {
            if (Name.EndsWith(".idw"))
            {
                if (SheetSize == DrawingSheetSizeEnum.kA4DrawingSheetSize || SheetSize == DrawingSheetSizeEnum.kA3DrawingSheetSize)
                {
                    IsDrawing = true;
                }
                else { IsDrawing = false; }
            }
            else if (Name.EndsWith("L.idw")) IsPrint = false; else IsPrint = true;
        }
    }
}
