using System.Windows.Controls;
using ReGraphik.ViewModels; 

namespace ReGraphik.Views.Pages
{
    public partial class DashboardPage : Page
    {
        public DashboardPage(string nomeUsuario = "Usuário")
        {
            InitializeComponent();
            DataContext = new DashboardViewModel(nomeUsuario);

        }

    }
}