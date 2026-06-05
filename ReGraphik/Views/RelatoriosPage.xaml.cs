using ReGraphik.ViewModels;
using System.Windows.Controls;

namespace ReGraphik.Views.Pages
{
    public partial class RelatoriosPage : Page
    {
        public RelatoriosPage()
        {
            InitializeComponent();
            DataContext = new RelatorioViewModel();
        }
    }
}