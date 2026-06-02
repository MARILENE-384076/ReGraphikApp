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
using ReGraphik.Models;

namespace ReGraphik.Views.Pages
{
    /// <summary>
    /// Interação lógica para ContaPage.xaml
    /// </summary>
    public partial class ContaPage : Page
    {
    
        private Usuario _usuarioAtual;

        
        public ContaPage(Usuario usuario)
        {
            InitializeComponent();

            
            _usuarioAtual = usuario;

        }
    }
}