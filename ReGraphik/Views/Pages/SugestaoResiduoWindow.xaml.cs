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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReGraphik.Views.Pages
{
    /// <summary>
    /// Interação lógica para SugestaoResiduoWindow.xaml
    /// </summary>
    public partial class SugestaoResiduoWindow : Window 
    {
        public SugestaoResiduoWindow(Models.Residuo residuo)
        {
            InitializeComponent();
        }

        /// <summary>
        /// Evento de clique para fechar a janela conforme definido no seu XAML (Click="BtnFechar_Click")
        /// </summary>
        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}