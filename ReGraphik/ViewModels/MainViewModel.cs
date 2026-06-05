using ReGraphik.Models;
using ReGraphik.Views.UserControls;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel principal da aplicação, responsável por gerenciar a navegação entre as diferentes views (Dashboard, Resíduos, Estoque, Mapa, Relatórios e Conta).
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        /// <summary>
        /// Referência para a janela física (MainWindow) para permitir o fechamento da aplicação ao clicar em "Sair".
        /// </summary>
        private readonly Window _currentWindow;
        private object _currentView;
        private string _nomeUsuario = string.Empty;
        private string _btnAtivo = string.Empty;

        public Usuario UsuarioLogado { get; }

        /// <summary>
        /// Propriedade que representa a view atualmente exibida no frame principal da janela.
        /// </summary>
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

        public ICommand NavegarDashboardCommand { get; }
        public ICommand NavegarResiduosCommand { get; }
        public ICommand NavegarEstoqueCommand { get; }
        public ICommand NavegarMapaCommand { get; }
        public ICommand NavegarRelatoriosCommand { get; }
        public ICommand NavegarContaCommand { get; }
        public ICommand SairCommand { get; }

        /// <summary>
        /// Construtor do MainViewModel, que recebe o usuário logado e a janela atual como parâmetros. 
        /// Ele inicializa a view para a Dashboard e configura os comandos de navegação para cada seção da aplicação, além do comando de sair.
        /// </summary>
        /// <param name="usuario">O usuário logado na aplicação.</param>
        /// <param name="window">A janela atual (MainWindow) da aplicação.</param>
        /// 
        public MainViewModel(Usuario usuario, Window window)
        {
            UsuarioLogado = usuario;
            _currentWindow = window;
            NomeUsuario = usuario.Nome ?? "Usuário";

            // Inicializa na Dashboard
            _currentView = new DashboardView(NomeUsuario);
            _btnAtivo = "Dashboard";

            // Inicialização dos comandos
            NavegarDashboardCommand = new RelayCommand(p => ExecutarNavegacao("Dashboard", new DashboardView(NomeUsuario)));
            NavegarResiduosCommand = new RelayCommand(p => ExecutarNavegacao("Residuos", new ResiduosView()));
            NavegarEstoqueCommand = new RelayCommand(p => ExecutarNavegacao("Estoque", new EstoqueReversoView()));
            NavegarMapaCommand = new RelayCommand(p => ExecutarNavegacao("Mapa", new MapaView()));
            NavegarRelatoriosCommand = new RelayCommand(p => ExecutarNavegacao("Relatorios", new RelatoriosView()));
            NavegarContaCommand = new RelayCommand(p => ExecutarNavegacao("Conta", new ContaView(UsuarioLogado)));
            SairCommand = new RelayCommand(p => ExecutarSair());
        }

        private void ExecutarNavegacao(string nomeBotao, object novaView)
        {
            BtnAtivo = nomeBotao;
            CurrentView = novaView;
        }

        private void ExecutarSair()
        {
            var resultado = MessageBox.Show(
                "Deseja realmente sair da aplicação?",
                "Confirmar Saída",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                _currentWindow.Close();
            }
        }
    }
}