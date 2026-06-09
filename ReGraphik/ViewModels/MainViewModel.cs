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
        private readonly Window _currentWindow;
        private object _currentView;
        private string _nomeUsuario = string.Empty;
        private string _btnAtivo = string.Empty;

        public Usuario UsuarioLogado { get; }

        /// <summary>
        /// ViewModel do chat — exposto para o DataContext da MainWindow
        /// poder fazer binding no botão de mensagem e no painel flutuante.
        /// </summary>
        public ChatViewModel ChatViewModel { get; }

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

        // ── Views criadas uma única vez e reutilizadas ──────────
        private readonly DashboardView _dashboardView;
        private readonly ResiduosView _residuosView;
        private readonly EstoqueReversoView _estoqueView;
        private readonly MapaView _mapaView;
        private readonly RelatoriosView _relatoriosView;
        private readonly ContaView _contaView;
        // ────────────────────────────────────────────────────────

        public ICommand NavegarDashboardCommand { get; }
        public ICommand NavegarResiduosCommand { get; }
        public ICommand NavegarEstoqueCommand { get; }
        public ICommand NavegarMapaCommand { get; }
        public ICommand NavegarRelatoriosCommand { get; }
        public ICommand NavegarContaCommand { get; }
        public ICommand SairCommand { get; }

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

            // Começa na Dashboard
            _currentView = _dashboardView;
            _btnAtivo = "Dashboard";

            // Comandos reutilizam a mesma instância
            NavegarDashboardCommand = new RelayCommand(p => ExecutarNavegacao("Dashboard", _dashboardView));
            NavegarResiduosCommand = new RelayCommand(p => ExecutarNavegacao("Residuos", _residuosView));
            NavegarEstoqueCommand = new RelayCommand(p => ExecutarNavegacao("Estoque", _estoqueView));
            NavegarMapaCommand = new RelayCommand(p => ExecutarNavegacao("Mapa", _mapaView));
            NavegarRelatoriosCommand = new RelayCommand(p => ExecutarNavegacao("Relatorios", _relatoriosView));
            NavegarContaCommand = new RelayCommand(p => ExecutarNavegacao("Conta", _contaView));
            SairCommand = new RelayCommand(p => ExecutarSair());
        }

        private void ExecutarNavegacao(string nomeBotao, object view)
        {
            BtnAtivo = nomeBotao;
            CurrentView = view;
        }

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