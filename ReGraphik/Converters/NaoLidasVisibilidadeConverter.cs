using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReGraphik.Converters
{
    /// <summary>
    /// Conversor de valor (IValueConverter) para o WPF que controla a exibição de badges ou alertas baseando-se em contagens numéricas.
    /// Retorna Visibility.Visible se houver pelo menos 1 item (ex: mensagens não lidas), e Visibility.Collapsed caso contrário.
    /// Evita que elementos visuais vazios (como círculos de notificação com número zero) poluam a interface do usuário.
    /// </summary>
    public class NaoLidasVisibilidadeConverter : IValueConverter
    {
        /// <summary>
        /// Avalia a quantidade de mensagens ou itens recebidos e determina se o elemento gráfico deve ficar visível.
        /// </summary>
        /// <param name="value">O valor numérico (geralmente um int) que representa o total de itens não lidos.</param>
        /// <param name="targetType">O tipo de destino da propriedade no XAML (espera-se Visibility).</param>
        /// <param name="parameter">Parâmetro opcional do conversor (não utilizado neste caso).</param>
        /// <param name="culture">Informações de cultura/região da interface.</param>
        /// <returns>
        /// <see cref="Visibility.Visible"/> se o valor for um número inteiro maior que zero; 
        /// <see cref="Visibility.Collapsed"/> se for zero, negativo ou se o tipo recebido for inválido.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int count && count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Conversão reversa (da interface gráfica de volta para a ViewModel).
        /// Não é suportada por este conversor, pois ele foi desenhado para atuar de forma unidirecional (One-Way).
        /// </summary>
        /// <exception cref="NotImplementedException">Sempre lançado, pois a conversão reversa não é aplicável.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}