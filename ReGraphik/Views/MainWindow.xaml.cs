using System.Windows;
using System.Windows.Controls;
using ReGraphik.Models; 
using ReGraphik.Views.Pages;

namespace ReGraphik.Views
{
    /// <summary>
    /// Lógica de interação para MainWindow.xaml.
    /// Representa a janela principal da aplicação, responsável por gerenciar a navegação 
    /// entre as páginas no frame principal e o estado visual do menu lateral.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Armazena a referência do botão de navegação atualmente ativo para controle de estilo.
        /// </summary>
        private Button? _btnAtivo;

        /// <summary>
        /// [ALTERADO] Armazena o objeto completo do usuário logado na aplicação.
        /// </summary>
        private Usuario _usuarioLogado;

        /// <summary>
        /// Inicializa uma nova instância da janela <see cref="MainWindow"/>.
        /// </summary>
        /// <param name="usuario">O objeto do usuário autenticado.</param>
        public MainWindow(Usuario usuario) 
        {
            InitializeComponent();

            _usuarioLogado = usuario; 

            
            TxtNomeUsuario.Text = _usuarioLogado.Nome ?? "Usuário";

            
            MainFrame.Navigate(new DashboardPage(_usuarioLogado.Nome ?? "Usuário"));
            _btnAtivo = BtnDashboard;
        }

        /// <summary>
        /// Atualiza os estilos visuais dos botões do menu de navegação.
        /// Remove o destaque do botão anterior e aplica o estilo de "ativo" ao botão recém-clicado.
        /// </summary>
        /// <param name="btn">O botão que acabou de ser selecionado pelo usuário.</param>
        private void SetarNavAtivo(Button btn)
        {
            if (_btnAtivo != null)
                _btnAtivo.Style = (Style)FindResource("NavBtn");

            btn.Style = (Style)FindResource("NavBtnAtivo");
            _btnAtivo = btn;
        }

        /// <summary>
        /// Manipulador do evento de clique para o botão "Dashboard".
        /// Navega o frame principal para a <see cref="DashboardPage"/>.
        /// </summary>
        /// <param name="sender">O objeto que acionou o evento.</param>
        /// <param name="e">Os argumentos do evento de roteamento.</param>
        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetarNavAtivo(BtnDashboard);
            MainFrame.Navigate(new DashboardPage(_usuarioLogado.Nome ?? "Usuário")); // 
        }

        /// <summary>
        /// Manipulador do evento de clique para o botão "Cadastrar Resíduos".
        /// Navega o frame principal para a <see cref="ResiduosPage"/>.
        /// </summary>
        /// <param name="sender">O objeto que acionou o evento.</param>
        /// <param name="e">Os argumentos do evento de roteamento.</param>
        private void Residuos_Click(object sender, RoutedEventArgs e)
        {
            SetarNavAtivo(BtnResiduos);
            MainFrame.Navigate(new ResiduosPage());
        }

        /// <summary>
        /// Manipulador do evento de clique para o botão "Estoque Reverso".
        /// Navega o frame principal para a <see cref="EstoqueReversoPage"/>.
        /// </summary>
        /// <param name="sender">O objeto que acionou o evento.</param>
        /// <param name="e">Os argumentos do evento de roteamento.</param>
        private void EstoqueReverso_Click(object sender, RoutedEventArgs e)
        {
            SetarNavAtivo(BtnEstoqueReverso);
            MainFrame.Navigate(new EstoqueReversoPage());
        }

        /// <summary>
        /// Manipulador do evento de clique para o botão "Mapa".
        /// Navega o frame principal para a <see cref="MapaPage"/>.
        /// </summary>
        /// <param name="sender">O objeto que acionou o evento.</param>
        /// <param name="e">Os argumentos do evento de roteamento.</param>
        private void Mapa_Click(object sender, RoutedEventArgs e)
        {
            SetarNavAtivo(BtnMapa);
            MainFrame.Navigate(new MapaPage());
        }

        /// <summary>
        /// Manipulador do evento de clique para o botão "Relatórios".
        /// Navega o frame principal para a <see cref="RelatoriosPage"/>.
        /// </summary>
        /// <param name="sender">O objeto que acionou o evento.</param>
        /// <param name="e">Os argumentos do evento de roteamento.</param>
        private void Relatorios_Click(object sender, RoutedEventArgs e)
        {
            SetarNavAtivo(BtnRelatorios);
            MainFrame.Navigate(new RelatoriosPage());
        }

        /// <summary>
        /// Manipulador do evento de clique para o botão "Minha Conta".
        /// Navega o frame principal para a <see cref="ContaPage"/>, permitindo a edição do perfil.
        /// </summary>
        /// <param name="sender">O objeto que acionou o evento.</param>
        /// <param name="e">Os argumentos do evento de roteamento.</param>
        private void Conta_Click(object sender, RoutedEventArgs e)
        {
            SetarNavAtivo(BtnConta);
            
            MainFrame.Navigate(new ContaPage(_usuarioLogado));
        }

        /// <summary>
        /// Manipulador do evento de clique para o botão "Sair".
        /// Exibe um prompt de confirmação e, se aceito, encerra a sessão atual 
        /// redirecionando o usuário de volta para a tela de login.
        /// </summary>
        /// <param name="sender">O objeto que acionou o evento.</param>
        /// <param name="e">Os argumentos do evento de roteamento.</param>
        private void Sair_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "Deseja realmente sair da aplicação?",
                "Confirmar Saída",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                var loginTela = new LoginWindow();
                loginTela.Show();
                this.Close(); 
            }
        }
    }
}