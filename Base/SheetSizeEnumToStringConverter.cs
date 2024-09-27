using Inventor;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTools.Base
{
    public sealed class SheetSizeEnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as DrawingSheetSizeEnum?) switch
            {
                DrawingSheetSizeEnum.kCustomDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.kADrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.kBDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.kCDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.kDDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.kEDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.kFDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.kA0DrawingSheetSize => "A0",
                DrawingSheetSizeEnum.kA1DrawingSheetSize => "A1",
                DrawingSheetSizeEnum.kA2DrawingSheetSize => "A2",
                DrawingSheetSizeEnum.kA3DrawingSheetSize => "A3",
                DrawingSheetSizeEnum.kA4DrawingSheetSize => "A4",
                DrawingSheetSizeEnum.k9x12InDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.k12x18InDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.k18x24InDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.k24x36InDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.k36x48InDrawingSheetSize => throw new NotImplementedException(),
                DrawingSheetSizeEnum.k30x42InDrawingSheetSize => throw new NotImplementedException(),
                null => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
