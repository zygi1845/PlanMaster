using System;
using System.Globalization;
using System.Windows.Data;

namespace Frontend.ViewModels
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;
            
            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string paramValue)
            {
                return Enum.Parse(targetType, paramValue);
            }
            return Binding.DoNothing;
        }
    }
}