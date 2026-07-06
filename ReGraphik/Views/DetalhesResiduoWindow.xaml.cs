using ReGraphik.Models;
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
    /// Interaction logic for DetalhesResiduoWindow.xaml
    /// </summary>
    public partial class DetalhesResiduoWindow : Window
    {
        public DetalhesResiduoWindow(Residuo residuo)
        {
            InitializeComponent();
            this.DataContext = residuo;
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }
    }
}
