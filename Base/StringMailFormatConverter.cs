using Microsoft.UI.Xaml.Data;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Base
{
    public sealed class StringMailFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            var stringValue = (string)value;
            try
            {
                MailAddress mailAdress = new MailAddress(stringValue);
                return mailAdress.ToString();
            }
            catch (FormatException)
            {
                return "mail invalide";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            string language)
        {
            throw new NotImplementedException();
        }
    }
}
