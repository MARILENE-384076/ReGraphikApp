using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReGraphik.Converters
{
    /// <summary>
    /// Converter que converte uma string para Visibility.
    /// Retorna Visible se a string não for nula ou vazia, Collapsed caso contrário.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}