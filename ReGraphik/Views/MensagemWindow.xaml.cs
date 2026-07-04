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
    /// Interaction logic for MensagemWindow.xaml
    /// </summary>
    public partial class MensagemWindow : Window
    {
        public enum TipoMensagem { Sucesso, Erro, Confirmacao, Aviso }

        private MensagemWindow(string titulo, string mensagem, TipoMensagem tipo)
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
                    // Verde de Sucesso (Igual ao seu padrão do XAML original)
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0FDF4"));
                    IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCFCE7"));
                    MessageIcon.Kind = PackIconMaterialKind.CheckCircleOutline;
                    MessageIcon.Foreground = (Brush)FindResource("VerdeColor");

                    BtnAcao.Content = "Entendi";
                    BtnAcao.Background = (Brush)FindResource("AzulMedioColor");
                    break;

                case TipoMensagem.Erro:
                    // Vermelho de Alerta/Erro
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                    IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA5A5"));
                    MessageIcon.Kind = PackIconMaterialKind.AlertCircleOutline;
                    MessageIcon.Foreground = (Brush)FindResource("VermelhoColor");

                    BtnAcao.Content = "Fechar";
                    BtnAcao.Background = (Brush)FindResource("VermelhoColor");
                    break;

                case TipoMensagem.Aviso:
                case TipoMensagem.Confirmacao:
                    // Laranja / Ambar para avisos do sistema
                    IconBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFBEB"));
                    IconBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                    MessageIcon.Kind = PackIconMaterialKind.AlertOutline;
                    MessageIcon.Foreground = (Brush)FindResource("LaranjaEsgColor");

                    BtnAcao.Content = "Ok";
                    BtnAcao.Background = (Brush)FindResource("AzulMedioColor");
                    break;
            }
        }

        /// <summary>
        /// Exibe a janela de mensagem geral customizada para o ReGraphik.
        /// </summary>
        /// <param name="titulo">Título em destaque na mensagem</param>
        /// <param name="mensagem">Texto descritivo do corpo</param>
        /// <param name="tipo">Estilo visual (Sucesso, Erro, Aviso)</param>
        /// <param name="owner">Janela pai para centralização</param>
        public static bool? Exibir(string titulo, string mensagem, TipoMensagem tipo, Window owner = null)
        {
            var win = new MensagemWindow(titulo, mensagem, tipo);
            win.Owner = owner ?? Application.Current.MainWindow;
            return win.ShowDialog();
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

