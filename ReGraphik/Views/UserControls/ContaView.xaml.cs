using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
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

namespace ReGraphik.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ContaView.xaml
    /// </summary>
    public partial class ContaView : UserControl
    {
        public ContaView(Usuario usuario)
        {
            InitializeComponent();
            DataContext = new UsuarioViewModel(usuario);
        }
    }
}
