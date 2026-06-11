using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ReGraphik.Converters
{
    /// <summary>
    /// Alinha a mensagem à direita se for do usuário logado (EhMinhaMensagem = true),
    /// ou à esquerda para mensagens recebidas.
    /// </summary>
    public class MensagemAlinhamentoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ehMinha = value is bool b && b;
            return ehMinha ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Define a cor de fundo do balão: azul para mensagens enviadas, branco para recebidas.
    /// </summary>
    public class MensagemCorFundoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ehMinha = value is bool b && b;
            return ehMinha
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1649a2"))
                : new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Define a cor do texto do balão: branco para enviadas, escuro para recebidas.
    /// </summary>
    public class MensagemCorTextoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ehMinha = value is bool b && b;
            return ehMinha
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Formata o horário da mensagem de forma amigável.
    /// </summary>
    public class DataHoraMensagemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime dt) return string.Empty;
            var diff = DateTime.Now - dt;
            if (diff.TotalMinutes < 1) return "agora";
            if (diff.TotalHours < 1) return $"{(int)diff.TotalMinutes}min";
            if (dt.Date == DateTime.Today) return dt.ToString("HH:mm");
            return dt.ToString("dd/MM HH:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    
}