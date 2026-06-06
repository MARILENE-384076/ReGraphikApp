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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? status = value as string;

            // Define as cores para cada status usando códigos hexadecimais
            string hexColor = status switch
            {
                "Aguardando CADRI" => "#0d2a56",       // Azul Escuro
                "Aguardando Triagem" => "#1649a2",     // Azul Médio
                "Disponível" => "#64748B",             // Cinza
                "Disponível para Coleta" => "#3274ba",  // Azul Claro
                "Liberado para Venda" => "#2f80ec",    // Azul Muito Claro
                _ => "#64748B"                         // Cor padrão (Cinza)
            };

            // Se o parâmetro for "Foreground", ajustamos a cor do texto para garantir legibilidade
            if (parameter as string == "Foreground")
            {
                // Para os status "Aguardando CADRI" e "Aguardando Triagem", usamos branco para garantir contraste com
                // o fundo azul escuro. Para os outros status, usamos uma cor escura para garantir legibilidade sobre fundos mais claros.
                return (status == "Aguardando CADRI" || status == "Aguardando Triagem" || status == "Disponível para Coleta" || status == "Liberado para Venda" || status == "Disponível")
                    ? Brushes.White
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
