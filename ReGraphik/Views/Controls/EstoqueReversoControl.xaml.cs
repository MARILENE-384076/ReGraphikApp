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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReGraphik.Views.Controls
{
    /// <summary>
    /// Define a interação lógica para EstoqueReversoControl.xaml
    /// </summary>
    public partial class EstoqueReversoControl : UserControl
    {
        public EstoqueReversoControl()
        {
            InitializeComponent();
            DataContext = new EstoqueReversoViewModel();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
