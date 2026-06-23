using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Views;
using ReGraphik.Views.Controls;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel principal da aplicação, responsável por gerenciar a navegação
    /// entre as páginas e o estado do menu lateral.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly Window _currentWindow;
        private object _currentView;
        private string _nomeUsuario = string.Empty;
        private string _btnAtivo = "Dashboard";

        /// <summary>
        /// Mantém a instância única do painel de chat para reutilização
        /// </summary>
        private ChatPainelWindow? _chatWindow;

        public Usuario UsuarioLogado { get; }

        /// <summary>
        /// ViewModel do chat — exposto para o DataContext da MainWindow
        /// poder fazer binding no botão de mensagem e no painel flutuante.
        /// </summary>
        public ChatViewModel ChatViewModel { get; }

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        public string NomeUsuario
        {
            get => _nomeUsuario;
            set { _nomeUsuario = value; OnPropertyChanged(); }
        }

        public string BtnAtivo
        {
            get => _btnAtivo;
            set { _btnAtivo = value; OnPropertyChanged(nameof(BtnAtivo)); }
        }

        private readonly DashboardControl _dashboardView;
        private readonly ResiduosControl _residuosView;
        private readonly EstoqueReversoControl _estoqueView;
        private readonly MapaControl _mapaView;
        private readonly RelatoriosControl _relatoriosView;
        private readonly EsgControl _esgView;
        private readonly ContaControl _contaView;

        /// <summary>
        /// Comandos 
        /// </summary>
        public ICommand NavegarDashboardCommand { get; }
        public ICommand NavegarResiduosCommand { get; }
        public ICommand NavegarEstoqueCommand { get; }
        public ICommand NavegarMapaCommand { get; }
        public ICommand NavegarRelatoriosCommand { get; }
        public ICommand NavegarContaCommand { get; }
        public ICommand NavegarEsgCommand { get; }
        public ICommand SairCommand { get; }
        public ICommand ChatCommand { get; }
        public ICommand irParaRelatorios { get; }

        public MainViewModel(Usuario usuario, Window window)
        {
            UsuarioLogado = usuario;
            _currentWindow = window;
            NomeUsuario = usuario.Nome ?? "Usuário";

            /// Instancia o ChatViewModel passando o usuário logado.
            /// A inscrição no Firebase (escuta em tempo real) começa aqui,
            /// assim o badge de notificações já funciona antes de abrir o chat.
            ChatViewModel = new ChatViewModel(usuario);

            /// Quando o ChatViewModel receber uma nova mensagem em background,
            /// exibe um toast discreto na tela principal.
            ChatViewModel.NovaMensagemRecebida += OnNovaMensagemRecebida;

            ChatCommand = new RelayCommand(ChatConversar);

            /// Carrega foto persistida do disco
            var fotoSalva = ConfiguracaoLocalService.CarregarFoto();
            if (fotoSalva != null)
                UsuarioSessaoService.Instancia.FotoCaminho = fotoSalva;
            
            NavegarDashboardCommand = new RelayCommand(p => NavegarParaDashboard());
            NavegarResiduosCommand = new RelayCommand(p => NavegarParaResiduos());
            NavegarEstoqueCommand = new RelayCommand(p => NavegarParaEstoque());
            NavegarMapaCommand = new RelayCommand(p => NavegarParaMapa());
            NavegarRelatoriosCommand = new RelayCommand(p => NavegarParaRelatorios());
            NavegarEsgCommand = new RelayCommand(p => NavegarParaEsg());
            NavegarContaCommand = new RelayCommand(p => NavegarParaConta());
            SairCommand = new RelayCommand(p => ExecutarSair());

            _dashboardView = new DashboardControl(NomeUsuario);
            _residuosView = new ResiduosControl();
            _estoqueView = new EstoqueReversoControl();
            _mapaView = new MapaControl();
            _relatoriosView = new RelatoriosControl();
            _contaView = new ContaControl(UsuarioLogado);
            _esgView = new EsgControl(usuario, NavegarRelatoriosCommand);

            _currentView = _dashboardView;

            
        }

        /// <summary>
        /// Controles de navegação entre abas
        /// </summary>
        private void NavegarParaDashboard() => ExecutarNavegacao("Dashboard", _dashboardView);
        private void NavegarParaResiduos() => ExecutarNavegacao("Residuos", _residuosView);
        private void NavegarParaEstoque() => ExecutarNavegacao("Estoque", _estoqueView);
        private void NavegarParaMapa() => ExecutarNavegacao("Mapa", _mapaView);
        private void NavegarParaRelatorios() => ExecutarNavegacao("Relatorios", _relatoriosView);
        private void NavegarParaEsg() => ExecutarNavegacao("Esg", _esgView);
        private void NavegarParaConta() => ExecutarNavegacao("Conta", _contaView);

        private void ExecutarNavegacao(string nomeBotao, object view)
        {
            BtnAtivo = nomeBotao;
            CurrentView = view;
        }

        /// <summary>
        /// Abre (ou traz para frente) o painel de chat.
        /// As conversas já estão carregadas porque o ChatViewModel escuta o
        /// Firebase desde o momento em que o usuário faz login.
        /// </summary>
        private void ChatConversar()
        {
            /// Reutiliza a janela se já foi criada e ainda está aberta
            if (_chatWindow != null && _chatWindow.IsLoaded)
            {
                _chatWindow.Activate();
                return;
            }

            _chatWindow = new ChatPainelWindow();
            _chatWindow.DataContext = ChatViewModel;

            _chatWindow.ShowInTaskbar = false;

            // 2. Define a janela principal do sistema como dona do chat
            _chatWindow.Owner = System.Windows.Application.Current.MainWindow;

            /// Ao abrir, força recarregamento das conversas para refletir
            /// mensagens novas recebidas enquanto o painel estava fechado
            _ = ChatViewModel.CarregarConversasPublicAsync();

            _chatWindow.Show();
        }

        /// <summary>
        /// Callback disparado pelo ChatViewModel quando uma mensagem nova
        /// chega de outro usuário enquanto o painel está fechado. Exibe um toast discreto sem interromper o fluxo do usuário.
        /// </summary>
        private void OnNovaMensagemRecebida(string remetenteNome, string textoPreview)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                /// Só exibe o toast se o painel de chat não estiver visível
                if (_chatWindow == null || !_chatWindow.IsVisible)
                {
                    var toast = new ChatToastWindow(remetenteNome, textoPreview);

                    /// Ao clicar no toast, abre o chat direto na conversa certa
                    toast.Clicado += () => ChatConversar();
                    toast.Show();
                }
            });
        }

        /// <summary>
        /// Volta para a tela de login/cadastro
        /// </summary>
        private void ExecutarSair()
        {
            /// Executa na Thread correta da interface visual
            Application.Current.Dispatcher.Invoke(() =>
            {
                var confirmWindow = new ReGraphik.Views.SairMensagemWindow();

                /// Configurações para travar o pop-up sem criar abas extras na barra de ferramentas
                confirmWindow.Owner = Application.Current.MainWindow;
                confirmWindow.ShowInTaskbar = false;

                /// Abre a janela e espera o usuário responder (Retorna true se clicar em Sair)
                bool? resultado = confirmWindow.ShowDialog();

                if (resultado == true)
                {
                    /// Para o timer e libera recursos do chat antes de sair
                    ChatViewModel.Dispose();
                    ChatViewModel.NovaMensagemRecebida -= OnNovaMensagemRecebida;

                    var loginWindow = new LoginWindow();
                    if (Application.Current != null)
                        Application.Current.MainWindow = loginWindow;

                    loginWindow.Show();
                    _currentWindow?.Close();
                }
            });
        }
    }
}