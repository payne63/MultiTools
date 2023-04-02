using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplittableDataGridSAmple.Base
{
    class CategoryTypeEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //Trace.WriteLine(value.ToString());
            var enumObject = (DataI.CategoryType)value;
            return enumObject.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
