using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReGraphik.Views
{
    /// <summary>
    /// Interaction logic for MensagemPDFWindow.xaml
    /// </summary>
    public partial class MensagemPdfWindow : Window
    {
        public enum TipoMensagem { Sucesso, Erro, Confirmacao }

        private MensagemPdfWindow(string titulo, string mensagem, TipoMensagem tipo)
        {
            InitializeComponent();

            TxtTitulo.Text = titulo;
            TxtMensagem.Text = mensagem;

            ConfigurarJanela(tipo);
        }

        private void ConfigurarJanela(TipoMensagem tipo)
        {
            switch (tipo)
            {
                case TipoMensagem.Sucesso:
                    /// Ícone de check verde (Sucesso)
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6F4EA"));
                    IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"));
                    MessageIcon.Kind = PackIconMaterialKind.CheckCircle;
                    MessageIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"));

                    BtnEsquerdo.Visibility = Visibility.Collapsed;
                    BtnDireito.Content = "Ok";
                    BtnDireito.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16A34A"));
                    break;

                case TipoMensagem.Erro:
                    /// Ícone de erro vermelho
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                    IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));
                    MessageIcon.Kind = PackIconMaterialKind.AlertOctagon;
                    MessageIcon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));

                    BtnEsquerdo.Visibility = Visibility.Collapsed;
                    BtnDireito.Content = "Fechar";
                    BtnDireito.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DC2626"));
                    break;

                case TipoMensagem.Confirmacao:
                    /// Ícone de arquivo PDF/Download ou Alerta Laranja para perguntar se deseja abrir
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBEB"));
                    IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                    MessageIcon.Kind = PackIconMaterialKind.FilePdfBox; // Ícone específico de PDF combina muito aqui!
                    MessageIcon.Foreground = (Brush)FindResource("LaranjaEsgColor");

                    BtnEsquerdo.Visibility = Visibility.Visible;
                    BtnEsquerdo.Content = "Não";
                    BtnDireito.Content = "Sim";
                    BtnDireito.Background = (Brush)FindResource("AzulMedioColor");
                    break;
            }
        }

        /// <summary>
        /// Exibe a janela de mensagem customizada para o fluxo de PDF do ReGraphik.
        /// </summary>
        public static bool? Exibir(string titulo, string mensagem, TipoMensagem tipo, Window owner = null)
        {
            var win = new MensagemPdfWindow(titulo, mensagem, tipo);
            win.Owner = owner ?? Application.Current.MainWindow;
            return win.ShowDialog();
        }

        private void BtnEsquerdo_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnDireito_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
