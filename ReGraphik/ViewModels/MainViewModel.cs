using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Views;
using ReGraphik.Views.Controls;
using System.Windows;
using System.Windows.Input;
// Certifique-se de adicionar o using correto para os seus serviços aqui se necessário, ex:
// using ReGraphik.Services; 

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel principal da aplicação, responsável por gerenciar a navegação entre as páginas e o estado do menu lateral.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        /// <summary>
        /// Referência à janela atual para controle de navegação e fechamento da aplicação.
        /// </summary>
        private readonly Window _currentWindow;
        private object _currentView;
        private string _nomeUsuario = string.Empty;
        private string _btnAtivo = "Dashboard";

        /// <summary>
        /// Propriedade que representa o usuário logado. Exposta para que outras partes da aplicação possam
        /// acessar informações do usuário, como nome e foto, sem precisar passar o objeto de usuário por parâmetros.
        /// </summary>
        public Usuario UsuarioLogado { get; }

        /// <summary>
        /// ViewModel do chat — exposto para o DataContext da MainWindow
        /// poder fazer binding no botão de mensagem e no painel flutuante.
        /// </summary>
        public ChatViewModel ChatViewModel { get; }

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); } /// Notifica a mudança da view atual para atualizar a interface
        }

        public string NomeUsuario
        {
            get => _nomeUsuario;
            set { _nomeUsuario = value; OnPropertyChanged(); }
        }

        public string BtnAtivo
        {
            get => _btnAtivo;
            set { _btnAtivo = value; OnPropertyChanged(nameof(BtnAtivo)); } /// Notifica a mudança do botão ativo para atualizar os estilos dos botões no menu lateral
        }

        /// <summary>
        /// Instancia cada view apenas uma vez e as mantém em memória para navegação rápida.
        /// </summary>
        private readonly DashboardControl _dashboardView;
        private readonly ResiduosControl _residuosView;
        private readonly EstoqueReversoControl _estoqueView;
        private readonly MapaControl _mapaView;
        private readonly RelatoriosControl _relatoriosView;
        private readonly ContaControl _contaView;

        /// <summary>
        /// Comandos de navegação para cada página. Reutilizam a mesma instância do comando,
        /// </summary>
        public ICommand NavegarDashboardCommand { get; }
        public ICommand NavegarResiduosCommand { get; }
        public ICommand NavegarEstoqueCommand { get; }
        public ICommand NavegarMapaCommand { get; }
        public ICommand NavegarRelatoriosCommand { get; }
        public ICommand NavegarContaCommand { get; }
        public ICommand SairCommand { get; }

        public ICommand ChatCommand { get; }

        /// <summary>
        /// Construtor do MainViewModel. Recebe o usuário logado e a janela atual para controle de navegação.
        /// </summary>
        /// <param name="usuario">Usuário logado</param>
        /// <param name="window">Janela atual</param>
        public MainViewModel(Usuario usuario, Window window)
        {
            UsuarioLogado = usuario;
            NomeUsuario = usuario.Nome ?? "Usuário";

            /// Instancia o ViewModel do chat passando o usuário logado
            ChatViewModel = new ChatViewModel(usuario);

            ChatCommand = new RelayCommand(ChatConversar);

            /// Carrega foto persistida do disco
            var fotoSalva = ConfiguracaoLocalService.CarregarFoto();
            if (fotoSalva != null)
                UsuarioSessaoService.Instancia.FotoCaminho = fotoSalva;

            /// Instancia cada view apenas uma vez
            _dashboardView = new DashboardControl(NomeUsuario);
            _residuosView = new ResiduosControl();
            _estoqueView = new EstoqueReversoControl();
            _mapaView = new MapaControl();
            _relatoriosView = new RelatoriosControl();
            _contaView = new ContaControl(UsuarioLogado);

            /// Começa na Dashboard
            _currentView = _dashboardView;

            /// Comandos reutilizam a mesma instância
            NavegarDashboardCommand = new RelayCommand(p => NavegarParaDashboard());
            NavegarResiduosCommand = new RelayCommand(p => NavegarParaResiduos());
            NavegarEstoqueCommand = new RelayCommand(p => NavegarParaEstoque());
            NavegarMapaCommand = new RelayCommand(p => NavegarParaMapa());
            NavegarRelatoriosCommand = new RelayCommand(p => NavegarParaRelatorios());
            NavegarContaCommand = new RelayCommand(p => NavegarParaConta());
            SairCommand = new RelayCommand(p => ExecutarSair());
        }

        private void NavegarParaDashboard()
        {
            ExecutarNavegacao("Dashboard", _dashboardView);
        }

        private void NavegarParaResiduos()
        {
            ExecutarNavegacao("Residuos", _residuosView);
        }

        private void NavegarParaEstoque()
        {
            ExecutarNavegacao("Estoque", _estoqueView);
        }

        private void NavegarParaMapa()
        {
            ExecutarNavegacao("Mapa", _mapaView);
        }

        private void NavegarParaRelatorios()
        {
            ExecutarNavegacao("Relatorios", _relatoriosView);
        }

        private void NavegarParaConta()
        {
            ExecutarNavegacao("Conta", _contaView);
        }

        /// <summary>
        /// Método centralizado para executar a navegação. Atualiza o botão ativo e a view atual.
        /// </summary>
        /// <param name="nomeBotao"></param>
        /// <param name="view"></param>
        private void ExecutarNavegacao(string nomeBotao, object view)
        {
            BtnAtivo = nomeBotao;
            CurrentView = view;
        }

        /// <summary>
        /// Exibe uma caixa de diálogo de confirmação antes de fechar a aplicação. Se o usuário confirmar, a janela é fechada.
        /// </summary>
        private void ExecutarSair()
        {
            var resultado = MessageBox.Show(
                "Deseja voltar para a tela de login?",
                "Confirmar Saída",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();

                if (Application.Current != null)
                {
                    Application.Current.MainWindow = loginWindow;
                }

                loginWindow.Show();
            }
        }

        private void ChatConversar()
        {
            var tela = new ChatPainelWindow();
            tela.ShowDialog(); 
            _currentWindow?.Close();
        }

    }
}