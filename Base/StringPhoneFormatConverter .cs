using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Base
{
    public sealed class StringPhoneFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            var stringValue = (string)value;
            if (stringValue.Length != 10 || !stringValue.All(char.IsDigit))return "numéro invalide";

            return Regex.Replace(stringValue, @"(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})", "$1.$2.$3.$4.$5");
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            string language)
        {
            throw new NotImplementedException();
        }
    }
}
