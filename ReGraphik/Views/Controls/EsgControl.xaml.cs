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
        /// <summary>
        /// Abre o link do botão no navegador padrão do sistema.
        /// </summary>
        private void AbrirLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is string url)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName        = url,
                    UseShellExecute = true
                });
            }
        }
        
    }
    
}