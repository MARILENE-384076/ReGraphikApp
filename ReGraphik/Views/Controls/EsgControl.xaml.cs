using ReGraphik.Models;
using ReGraphik.ViewModels;
using System.Windows.Controls;

namespace ReGraphik.Views.Controls
{
    /// <summary>
    /// Define a interação lógica para EsgControl.xaml.
    /// </summary>
    public partial class EsgControl : UserControl
    {
        /// <summary>
        /// Inicializa o controle com o usuário logado para personalizar o relatório exportado.
        /// </summary>
        public EsgControl(Usuario usuario)
        {
            InitializeComponent();
            DataContext = new EsgViewModel(usuario);
        }
        
    }
    
}