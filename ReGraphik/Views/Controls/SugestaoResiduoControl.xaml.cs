using ReGraphik.Models;
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
    /// Interaction logic for SugestaoResiduoControl.xaml
    /// </summary>
    public partial class SugestaoResiduoControl : UserControl
    {
        private Residuo _residuoSelecionado;
        public SugestaoResiduoControl(Residuo residuo)
        {
            InitializeComponent();
            DataContext = new SugestaoResiduoViewModel(residuo);
            _residuoSelecionado = residuo;
        }
    }
}
