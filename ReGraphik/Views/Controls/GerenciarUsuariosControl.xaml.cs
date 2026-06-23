using ReGraphik.ViewModels;
using System.Windows.Controls;

namespace ReGraphik.Views.Controls
{
    public partial class GerenciarUsuariosControl : UserControl
    {
        public GerenciarUsuariosControl()
        {
            InitializeComponent();
            DataContext = new GerenciarUsuariosViewModel();
        }
    }
}
