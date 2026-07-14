using ReGraphik.ViewModels;
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
    /// Interaction logic for EsqueciSenhaWindow.xaml
    /// </summary>
    public partial class EsqueciSenhaWindow : Window
    {
        public EsqueciSenhaWindow()
        {
            InitializeComponent();
            this.DataContext = new EsqueciSenhaViewModel();
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            /// Lógica para voltar ao login...
            this.Close();
        }
    }
}
