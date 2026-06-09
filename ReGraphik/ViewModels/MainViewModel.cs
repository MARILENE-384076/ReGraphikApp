using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Views.UserControls;
using System.Windows;
using System.Windows.Input;
// Certifique-se de adicionar o using correto para os seus serviços aqui se necessário, ex:
// using ReGraphik.Services; 

namespace ReGraphik.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        /// <summary>
        /// A MainViewModel é responsável por gerenciar a navegação entre as diferentes views da aplicação,
        /// </summary>
        private readonly Window _currentWindow;
        private object _currentView;
        private string _nomeUsuario = string.Empty;
        private string _btnAtivo = string.Empty;

        /// <summary>
        /// Armazena as informações do usuário logado, permitindo que elas sejam acessadas por todas as views e viewmodels,
        /// </summary>
        public Usuario UsuarioLogado { get; }

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public string NomeUsuario
        {
            get => _nomeUsuario;
            set { _nomeUsuario = value; OnPropertyChanged(); }
        }

        public string BtnAtivo
        {
            get => _btnAtivo;
            set { _btnAtivo = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Instancia cada view apenas uma vez para garantir que o estado seja mantido ao navegar entre elas, 
        /// evitando a criação de múltiplas instâncias e melhorando a performance da aplicação.
        /// </summary>
        private readonly DashboardView _dashboardView;
        private readonly ResiduosView _residuosView;
        private readonly EstoqueReversoView _estoqueView;
        private readonly MapaView _mapaView;
        private readonly RelatoriosView _relatoriosView;
        private readonly ContaView _contaView;

        /// <summary>
        /// Comandos de navegação reutilizam a mesma instância, centralizando a lógica de navegação no método ExecutarNavegacao,
        /// </summary>
        public ICommand NavegarDashboardCommand { get; }
        public ICommand NavegarResiduosCommand { get; }
        public ICommand NavegarEstoqueCommand { get; }
        public ICommand NavegarMapaCommand { get; }
        public ICommand NavegarRelatoriosCommand { get; }
        public ICommand NavegarContaCommand { get; }
        public ICommand SairCommand { get; }

        /// <summary>
        /// O construtor recebe o usuário logado e a janela atual para gerenciar a navegação e o estado visual do menu lateral.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="window"></param>
        public MainViewModel(Usuario usuario, Window window)
        {
            UsuarioLogado = usuario;
            _currentWindow = window;
            NomeUsuario = usuario.Nome ?? "Usuário";

            // ── TRECHO ADICIONADO ───────────────────────────────────
            // Carrega foto persistida do disco
            var fotoSalva = ConfiguracaoLocalService.CarregarFoto();
            if (fotoSalva != null)
                UsuarioSessaoService.Instancia.FotoCaminho = fotoSalva;
            // ────────────────────────────────────────────────────────

            // Instancia cada view apenas uma vez
            _dashboardView = new DashboardView(NomeUsuario);
            _residuosView = new ResiduosView();
            _estoqueView = new EstoqueReversoView();
            _mapaView = new MapaView();
            _relatoriosView = new RelatoriosView();
            _contaView = new ContaView(UsuarioLogado);

            /// Começa na Dashboard
            _currentView = _dashboardView;
            _btnAtivo = "Dashboard";

            /// Comandos reutilizam a mesma instância
            NavegarDashboardCommand = new RelayCommand(p => ExecutarNavegacao("Dashboard", _dashboardView));
            NavegarResiduosCommand = new RelayCommand(p => ExecutarNavegacao("Residuos", _residuosView));
            NavegarEstoqueCommand = new RelayCommand(p => ExecutarNavegacao("Estoque", _estoqueView));
            NavegarMapaCommand = new RelayCommand(p => ExecutarNavegacao("Mapa", _mapaView));
            NavegarRelatoriosCommand = new RelayCommand(p => ExecutarNavegacao("Relatorios", _relatoriosView));
            NavegarContaCommand = new RelayCommand(p => ExecutarNavegacao("Conta", _contaView));
            SairCommand = new RelayCommand(p => ExecutarSair());
        }

        /// <summary>
        /// Centraliza a lógica de navegação, garantindo que a mesma instância de cada view seja reutilizada e
        /// que a Dashboard atualize a foto do usuário sempre que for acessada.
        /// </summary>
        /// <param name="nomeBotao"></param>
        /// <param name="view"></param>
        private void ExecutarNavegacao(string nomeBotao, object view)
        {
            BtnAtivo = nomeBotao;
            CurrentView = view;

            if (nomeBotao == "Dashboard" && view is FrameworkElement elementoVisual)
            {
                /// Atualiza a foto do usuário na Dashboard toda vez que ela for acessada, garantindo que a imagem mais recente seja exibida.
                if (elementoVisual.DataContext is DashboardViewModel dashboardVM)
                {
                    dashboardVM.ImgFoto = FotoUserService.Foto;
                }
            }
        }

        /// <summary>
        /// Exibe uma mensagem de confirmação antes de fechar a aplicação, garantindo que o usuário não saia acidentalmente.
        /// </summary>
        private void ExecutarSair()
        {
            var resultado = MessageBox.Show(
                "Deseja realmente sair da aplicação?",
                "Confirmar Saída",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
                _currentWindow.Close();
        }
    }
}