using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ReGraphik.Converters
{
    public class CaminhoParaImagemConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string caminho && !string.IsNullOrWhiteSpace(caminho))
            {
                try
                {
                    string caminhoLimpo = caminho.Trim();

                    // Se for uma URL (Web / Firebase HTTPS ou HTTP)
                    if (caminhoLimpo.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(caminhoLimpo, UriKind.Absolute);

                        // CORREÇÃO AQUI:
                        // Removemos o DelayCreation que causava conflito.
                        // Usamos apenas OnDemand ou nenhum CreateOption para URLs externas,
                        // permitindo que o WPF gerencie o download em background nativamente.
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;

                        bitmap.EndInit();
                        return bitmap;
                    }

                    // Se for um arquivo físico local
                    if (File.Exists(caminhoLimpo))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(caminhoLimpo, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    // Escreve o erro detalhado na janela de Saída (Output) do Visual Studio
                    System.Diagnostics.Debug.WriteLine($"[FOTO ERROR] Falha ao carregar caminho: {caminho}. Erro: {ex.Message}");
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