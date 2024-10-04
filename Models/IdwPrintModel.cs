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
using MultiTools.Base;
using MultiTools.Helper;

namespace MultiTools.Models;

public class IdwPrintModel : NotifyPropertyChangedBase
{
    // private static readonly I.ApprenticeServerComponent appServer = new();

    // public event PropertyChangedEventHandler PropertyChanged;
    // protected void OnPropertyChanged([CallerMemberName] string name = null)
    // {
    //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    // }

    // public BitmapImage bitmapImage;

    public FileInfo FileInfoData
    {
        get;
    }

    private bool _IsPrint;

    public bool IsPrint
    {
        get => _IsPrint;
        set
        {
            _IsPrint = value;
            OnPropertyChanged();
        }
    }

    private bool _IsDrawing;

    public bool IsDrawing
    {
        get => _IsDrawing;
        set
        {
            _IsDrawing = value;
            OnPropertyChanged();
        }
    }

    private DrawingSheetSizeEnum _SheetSize;

    public DrawingSheetSizeEnum SheetSize
    {
        get => _SheetSize;
        set
        {
            _SheetSize = value;
            OnPropertyChanged();
        }
    }

    private PageOrientationTypeEnum _Orientation;

    public PageOrientationTypeEnum Orientation
    {
        get => _Orientation;
        set
        {
            _Orientation = value;
            OnPropertyChanged();
        }
    }

    private int _PageNumber;

    public int PageNumber
    {
        get => _PageNumber;
        set
        {
            _PageNumber = value;
            OnPropertyChanged();
        }
    }

    private bool _isOnlyOnePage;

    public bool IsOnlyOnePage
    {
        get => _isOnlyOnePage;
        set
        {
            _isOnlyOnePage = value;
            OnPropertyChanged();
        }
    }

    public bool isMultiPage => !IsOnlyOnePage;
    public string Name => FileInfoData.Name;

    private IdwPrintModel(string filePath,
        DrawingSheetSizeEnum drawingSheetSizeEnum,
        PageOrientationTypeEnum drawingOrientationEnum,
        int pageNumber,
        bool isOnlyOnePage,
        PropertyChangedEventHandler propertyChangedEventHandler)
    {
        SheetSize = drawingSheetSizeEnum;
        Orientation = drawingOrientationEnum;
        PageNumber = pageNumber;
        IsOnlyOnePage = isOnlyOnePage;
        FileInfoData = new FileInfo(filePath);
        DefineSelectPrint();
        PropertyChanged += propertyChangedEventHandler;
    }

    public void DefineSelectPrint()
    {
        if (Name.EndsWith(".idw"))
        {
            if (SheetSize == DrawingSheetSizeEnum.kA4DrawingSheetSize ||
                SheetSize == DrawingSheetSizeEnum.kA3DrawingSheetSize)
            {
                IsDrawing = true;
            }
            else
            {
                IsDrawing = false;
            }
        }
        else if (Name.EndsWith("L.idw")) IsPrint = false;
        else IsPrint = true;
    }

    public static async IAsyncEnumerable<IdwPrintModel> GetIdwPrintModels(string filePath, PropertyChangedEventHandler nbPDFDXFPropertyChanged)
    {
        ApprenticeServerDrawingDocument drawingDocument = null;
        await Task.Run((() =>
            {
                drawingDocument = ApprenticeHelper.GetApprenticeDrawingDocument(filePath);
            })
        );
        if (drawingDocument == null)
        {
            yield break;
        }

        for (var page = 1; page < drawingDocument.Sheets.Count + 1; page++)
        {
            yield return new IdwPrintModel(filePath,
                drawingDocument.Sheets[page].Size,
                drawingDocument.Sheets[page].Orientation,
                page,
                drawingDocument.Sheets.Count == 1,
                nbPDFDXFPropertyChanged);
        }
    }
    
}