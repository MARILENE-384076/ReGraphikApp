using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ReGraphik.Converters
{
    /// <summary>
    /// Converter que mapeia os diferentes status dos resíduos para cores específicas, permitindo uma visualização rápida e intuitiva do estado de cada resíduo na interface do usuário.
    /// </summary>
    public class StatusToColorConverter : IValueConverter
    {
        /// <summary>
        /// Converte o status do resíduo (string) em uma cor (SolidColorBrush) correspondente.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? status = value as string;

            /// Define as cores para cada status usando códigos hexadecimais
            string hexColor = status switch
            {
                "Disponível" => "#64748B",     /// CinzaColor
                "Reservado" => "#3274ba",      /// AzulClaroColor
                "Em Análise" => "#1649a2",     /// AzulMedioColor
                "Coletado" => "#0d2a56",       /// AzulEscuroColor
                "Indisponível" => "#475569",   /// CinzaEscuroColor
                _ => "#64748B"                 /// CinzaColor (Padrão)
            };

            /// Se o parâmetro for "Foreground", retornamos a cor apropriada para o texto, garantindo contraste com o fundo.
            if (parameter as string == "Foreground")
            {
                /// Retorna Branco para status com fundos mais escuros (Azuis e Cinza Escuro) 
                /// para garantir a legibilidade (Contraste Acessível)
                return (status == "Reservado" || status == "Em Análise" || status == "Coletado" || status == "Indisponível" || status == "Disponível")
                    ? Brushes.White
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }

        /// <summary>
        /// Implementação do método ConvertBack não é necessária para este conversor, pois ele é usado apenas para 
        /// converter o status em uma cor. Portanto, lançamos uma exceção NotImplementedException.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
