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
        public MensagemWindow()
        {
            InitializeComponent();
        }
        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
