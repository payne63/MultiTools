using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Base {
    public sealed class BoolColorGenderConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var boolValue = (bool)value;
            if (boolValue) 
            {
                return new SolidColorBrush(Colors.CadetBlue);
            }
            else 
            {
                return new SolidColorBrush(Colors.LightPink);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            string language) {
            throw new NotImplementedException();
        }
    }
}
