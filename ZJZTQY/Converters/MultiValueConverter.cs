using System;
using System.Globalization;
using System.Windows.Data;

namespace ZJZTQY.Converters
{
    public class EmailValidMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return false;
            var text = values[0] as string;
            bool hasError = values[1] is bool b1 && b1;     
            bool isSending = values[2] is bool b2 && b2;
            return !string.IsNullOrWhiteSpace(text) && !hasError && !isSending;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}