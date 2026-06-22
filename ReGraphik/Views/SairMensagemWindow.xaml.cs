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
    /// Interaction logic for SairMensagemWindow.xaml
    /// </summary>
    public partial class SairMensagemWindow : Window
    {
        public SairMensagemWindow()
        {
            InitializeComponent();
        }
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; /// Define que o usuário desistiu de sair
            this.Close();
        }

        private void BtnSair_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; /// Define que o usuário confirmou a saída
            this.Close();
        }
    }
}
