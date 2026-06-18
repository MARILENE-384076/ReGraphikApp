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
    /// Converte imagem em base64String
    /// </summary>
    public class Base64ToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string base64String && !string.IsNullOrWhiteSpace(base64String))
            {
                try
                {
                    /// Trata strings base64 que possam vir com metadados/cabeçalhos de URLs de dados
                    if (base64String.Contains(","))
                    {
                        base64String = base64String.Substring(base64String.IndexOf(",") + 1);
                    }

                    byte[] binaryData = System.Convert.FromBase64String(base64String);

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(binaryData);
                    bi.CacheOption = BitmapCacheOption.OnLoad; /// Garante o carregamento imediato em memória
                    bi.EndInit();

                    return bi;
                }
                catch
                {
                    return null; 
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
