using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ReGraphik.Converters
{
    /// <summary>
    /// Conversor de valor (IValueConverter) para o WPF que transforma uma string em formato Base64 em um objeto <see cref="BitmapImage"/>.
    /// Ideal para renderizar imagens que vêm de APIs ou bancos de dados diretamente na interface do usuário (ex: controle Image).
    /// </summary>
    public class Base64ToImageConverter : IValueConverter
    {
        /// <summary>
        /// Avalia a string Base64 recebida, limpa possíveis cabeçalhos de URL de dados (Data URI) e a converte para um fluxo de imagem em memória.
        /// </summary>
        /// <param name="value">A string Base64 contendo os dados da imagem recebida do Binding.</param>
        /// <param name="targetType">O tipo de destino da propriedade no XAML (espera-se ImageSource ou semelhante).</param>
        /// <param name="parameter">Parâmetro opcional do conversor (não utilizado neste caso).</param>
        /// <param name="culture">Informações de cultura/região da interface.</param>
        /// <returns>
        /// Um <see cref="BitmapImage"/> pronto para ser exibido na interface gráfica; 
        /// ou <c>null</c> caso a string seja inválida, vazia ou ocorra um erro na conversão.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string base64String && !string.IsNullOrWhiteSpace(base64String))
            {
                try
                {
                    /// Trata strings base64 que possam vir com metadados/cabeçalhos de URLs de dados (ex: "data:image/png;base64,...")
                    if (base64String.Contains(","))
                    {
                        base64String = base64String.Substring(base64String.IndexOf(",") + 1);
                    }

                    byte[] binaryData = System.Convert.FromBase64String(base64String);

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(binaryData);
                    bi.CacheOption = BitmapCacheOption.OnLoad; /// Garante o carregamento imediato em memória, permitindo fechar o stream em seguida
                    bi.EndInit();

                    return bi;
                }
                catch
                {
                    /// Em caso de falha de conversão (ex: string não é um base64 válido de imagem), 
                    /// retorna null silenciosamente para não quebrar a UI
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Conversão reversa (da interface gráfica de volta para a ViewModel).
        /// Não é suportada por este conversor, pois ele foi desenhado para atuar de forma unidirecional (One-Way).
        /// </summary>
        /// <exception cref="NotImplementedException">Sempre lançado, pois a extração da imagem de volta para string não está implementada aqui.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}