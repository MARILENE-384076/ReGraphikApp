using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReGraphik.Converters
{
    /// <summary>
    /// Converter que converte um bool para Visibility.
    /// True retorna Visible, False retorna Collapsed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}