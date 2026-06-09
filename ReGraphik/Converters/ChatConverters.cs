using System.Globalization;
using System.Windows;
using System.Windows.Data;

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
            return ehMinha ? "#1649a2" : "#FFFFFF";
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
            return ehMinha ? "#FFFFFF" : "#1E293B";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Oculta o badge de notificação quando não há mensagens não lidas.
    /// </summary>
    public class NaoLidasVisibilidadeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int count && count > 0 ? Visibility.Visible : Visibility.Collapsed;
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

    /// <summary>
    /// Formata o badge de notificações: mostra "99+" se passar de 99.
    /// </summary>
    public class BadgeNotificacaoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count > 99 ? "99+" : count.ToString();
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
