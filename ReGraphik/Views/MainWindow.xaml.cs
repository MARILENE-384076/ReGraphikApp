using System.Windows;
using System.Windows.Controls;
using ReGraphik.Models;
using ReGraphik.ViewModels;
using ReGraphik.Views.UserControls;

namespace ReGraphik.Views
{
    /// <summary>
    /// Lógica de interação para MainWindow.xaml.
    /// Representa a janela principal da aplicação, responsável por gerenciar a navegação 
    /// entre as páginas no frame principal e o estado visual do menu lateral.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(Usuario usuario) 
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(usuario, this);
        }

    }
}