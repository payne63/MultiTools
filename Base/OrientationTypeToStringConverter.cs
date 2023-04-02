using Inventor;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Base
{
    public sealed class OrientationTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as PageOrientationTypeEnum?) switch
            {
                PageOrientationTypeEnum.kDefaultPageOrientation => "Defaut",
                PageOrientationTypeEnum.kLandscapePageOrientation => "Horizontal",
                PageOrientationTypeEnum.kPortraitPageOrientation => "Vertical",
                null => "null?",
                _ => throw new NotImplementedException(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
