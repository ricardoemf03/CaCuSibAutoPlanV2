using System;
using System.Globalization;
using System.Windows.Data;

namespace CaCuSibAutoPlan.Converters
{
    public class RadioButtonStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string selected = value as string;
            string option = parameter as string;
            return string.Equals(selected, option, StringComparison.OrdinalIgnoreCase);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = value is bool && (bool)value;
            if (!isChecked) return Binding.DoNothing;
            return parameter as string;
        }
    }
}
