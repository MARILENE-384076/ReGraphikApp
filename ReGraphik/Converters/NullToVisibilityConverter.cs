using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReGraphik.Converters
{
    /// <summary>
    /// Conversor de valor (IValueConverter) para o WPF que transforma valores nulos em Visibilidade.
    /// Retorna Visibility.Visible quando o valor associado for nulo, e Visibility.Collapsed quando o valor não for nulo.
    /// Ideal para exibir placeholders, marcações de erro ou telas de "estado vazio" na interface.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Avalia o valor recebido pelo Binding e o converte para o enum Visibility do WPF.
        /// </summary>
        /// <param name="value">O valor atual recebido da fonte de dados (ViewModel).</param>
        /// <param name="targetType">O tipo de destino da propriedade no XAML (espera-se Visibility).</param>
        /// <param name="parameter">Parâmetro opcional do conversor (não utilizado neste caso).</param>
        /// <param name="culture">Informações de cultura/região da interface.</param>
        /// <returns>
        /// <see cref="Visibility.Visible"/> se o objeto for nulo; 
        /// <see cref="Visibility.Collapsed"/> se o objeto não for nulo.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Conversão reversa (da interface gráfica de volta para a ViewModel).
        /// Não é suportada por este conversor, pois ele foi desenhado para atuar de forma unidirecional (One-Way).
        /// </summary>
        /// <exception cref="NotImplementedException">Sempre lançado, pois a conversão reversa não é aplicável.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}