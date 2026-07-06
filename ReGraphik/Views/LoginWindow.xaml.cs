
using ReGraphik.Services;
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
using System.Windows.Shapes;

namespace ReGraphik.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            /// Unifica o contexto. A sua LoginViewModel deve ser a dona da tela.
            var trocaVM = new TrocaAbaViewModel();
            this.DataContext = trocaVM;

            /// Se a aba de cadastro realmente precisar de uma VM separada, 
            /// injete a referência do Login nela para compartilharem a mesma sessão!
            CadastroTab.DataContext = trocaVM.CadastroVM;

            /// Evita que o timer de 15 minutos expire na sua cara enquanto você está 
            /// digitando os dados do formulário nesta janela!
            this.PreviewKeyDown += (s, e) => UsuarioSessaoService.Instancia.ResetarTimer();
            this.PreviewMouseDown += (s, e) => UsuarioSessaoService.Instancia.ResetarTimer();
        }

    }
}
