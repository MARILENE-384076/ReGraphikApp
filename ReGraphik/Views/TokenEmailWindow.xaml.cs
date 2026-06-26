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
    /// Interaction logic for TokenEmailWindow.xaml
    /// </summary>
    public partial class TokenEmailWindow : Window
    {
        /// O construtor agora aceita dois parâmetros: o token e o e-mail do destinatário
        public TokenEmailWindow(string tokenGerado, string emailDestinatario)
        {
            InitializeComponent();

            /// Altera o token no corpo do e-mail
            TxtTokenExibido.Text = tokenGerado;

            /// Altera o e-mail dinamicamente no campo "Para:" do cabeçalho
            TxtDestinatarioSimulado.Text = emailDestinatario;
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCopiar_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TxtTokenExibido.Text))
            {
                Clipboard.SetText(TxtTokenExibido.Text);
            }
        }
    }
}
