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
        /// Propriedade para controlar a visibilidade na UI
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Mantém a instância única do painel de chat para reutilização
        /// </summary>
        private ChatPainelWindow? _chatWindow;


        public Usuario UsuarioLogado { get; }


        /// <summary>
        /// ViewModel do chat
        /// </summary>
        public ChatViewModel ChatViewModel { get; }


        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }


        public string NomeUsuario
        {
            get => _nomeUsuario;
            set
            {
                _nomeUsuario = value;
                OnPropertyChanged();
            }
        }


        public string BtnAtivo
        {
            get => _btnAtivo;
            set
            {
                _btnAtivo = value;
                OnPropertyChanged(nameof(BtnAtivo));
            }
        }

        private readonly DashboardControl _dashboardView;
        private readonly ResiduosControl _residuosView;
        private readonly EstoqueReversoControl _estoqueView;
        private readonly MapaControl _mapaView;
        private readonly RelatoriosControl _relatoriosView;
        private readonly EsgControl _esgView;
        private readonly ContaControl _contaView;

        /// <summary>
        /// View de gerenciamento de usuários (somente administrador)
        /// </summary>
        private readonly GerenciarUsuariosControl? _gerenciarUsuariosView;



        #region Commands

        public ICommand NavegarDashboardCommand { get; }
        public ICommand NavegarResiduosCommand { get; }
        public ICommand NavegarEstoqueCommand { get; }
        public ICommand NavegarMapaCommand { get; }
        public ICommand NavegarRelatoriosCommand { get; }
        public ICommand NavegarContaCommand { get; }
        public ICommand NavegarEsgCommand { get; }

        public ICommand NavegarGerenciarUsuariosCommand { get; }

        public ICommand SairCommand { get; }

        public ICommand ChatCommand { get; }

        public ICommand irParaRelatorios { get; }


        #endregion

        /// <summary>
        /// Inicializa uma nova instância do MainViewModel, configurando as views, comandos e eventos necessários para a navegação e funcionalidades da aplicação.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="window"></param>

        public MainViewModel(Usuario usuario, Window window)
        {

            UsuarioLogado = usuario;

            _currentWindow = window;

            /// Assina o evento de sessão expirada para realizar logout automático por inatividade
            UsuarioSessaoService.Instancia.SessaoExpirada += OnSessaoExpirada;


            NomeUsuario = usuario.Nome ?? "Usuário";

            /// Verifica a forma de perfil logado
            bool ehAdministrador = string.Equals(usuario.Perfil, "Administrador", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(usuario.Perfil, "Admin", StringComparison.OrdinalIgnoreCase);

            /// CHAT

            ChatViewModel = new ChatViewModel(usuario);

            ChatViewModel.NovaMensagemRecebida += OnNovaMensagemRecebida;

            ChatCommand = new RelayCommand(ChatConversar);


            /// FOTO USUÁRIO

            var fotoSalva = ConfiguracaoLocalService.CarregarFoto();

            if (fotoSalva != null)
                UsuarioSessaoService.Instancia.FotoCaminho = fotoSalva;

            /// NAVEGAÇÃO 

            NavegarDashboardCommand =
                new RelayCommand(p => NavegarParaDashboard());


            NavegarResiduosCommand =
                new RelayCommand(p => NavegarParaResiduos());


            NavegarEstoqueCommand =
                new RelayCommand(p => NavegarParaEstoque());


            NavegarMapaCommand =
                new RelayCommand(p => NavegarParaMapa());


            NavegarRelatoriosCommand =
                new RelayCommand(p => NavegarParaRelatorios());


            NavegarEsgCommand =
                new RelayCommand(p => NavegarParaEsg());


            NavegarContaCommand =
                new RelayCommand(p => NavegarParaConta());



            /// GERENCIAMENTO DE USUÁRIOS - Disponível apenas para Administrador 

            NavegarGerenciarUsuariosCommand =
                new RelayCommand(
                    p => NavegarParaGerenciarUsuarios(),
                    p => usuario.Perfil == "Administrador"
                );



            SairCommand =
                new RelayCommand(p => ExecutarSair());

            /// Views

            _dashboardView =
                new DashboardControl(usuario);


            _residuosView =
                new ResiduosControl();


            _estoqueView =
                new EstoqueReversoControl();


            _mapaView =
                new MapaControl();


            _relatoriosView =
                new RelatoriosControl();


            _contaView =
                new ContaControl(UsuarioLogado);


            _esgView =
                new EsgControl(usuario, NavegarRelatoriosCommand);



            /// Cria gerenciamento somente se administrador 
            if (usuario.Perfil == "Administrador")
            {
                _gerenciarUsuariosView = new GerenciarUsuariosControl();
                IsAdmin = true;
            }
            else
            {
                IsAdmin = false;
            }

            CurrentView = _dashboardView;

        }

        #region Navegação


        private void NavegarParaDashboard() =>
            ExecutarNavegacao(
                "Dashboard",
                _dashboardView);



        private void NavegarParaResiduos() =>
            ExecutarNavegacao(
                "Residuos",
                _residuosView);



        private void NavegarParaEstoque() =>
            ExecutarNavegacao(
                "Estoque",
                _estoqueView);



        private void NavegarParaMapa() =>
            ExecutarNavegacao(
                "Mapa",
                _mapaView);



        private void NavegarParaRelatorios() =>
            ExecutarNavegacao(
                "Relatorios",
                _relatoriosView);



        private void NavegarParaEsg() =>
            ExecutarNavegacao(
                "Esg",
                _esgView);



        private void NavegarParaConta() =>
            ExecutarNavegacao(
                "Conta",
                _contaView);



        private void NavegarParaGerenciarUsuarios()
        {
            if (_gerenciarUsuariosView != null)
            {
                ExecutarNavegacao(
                    "GerenciarUsuarios",
                    _gerenciarUsuariosView);
            }
        }

        private void ExecutarNavegacao(
            string nomeBotao,
            object view)
        {

            BtnAtivo = nomeBotao;

            CurrentView = view;

            // Reinicia o timer de inatividade a cada troca de tela
            UsuarioSessaoService.Instancia.ResetarTimer();

        }

        #endregion

        #region Chat


        private void ChatConversar()
        {

            if (_chatWindow != null &&
                _chatWindow.IsLoaded)
            {
                _chatWindow.Activate();
                return;
            }



            _chatWindow =
                new ChatPainelWindow();


            _chatWindow.DataContext =
                ChatViewModel;



            _chatWindow.ShowInTaskbar =
                false;



            _chatWindow.Owner =
                Application.Current.MainWindow;



            _ = ChatViewModel.CarregarConversasPublicAsync();



            _chatWindow.Show();

        }





        private void OnNovaMensagemRecebida(
            string remetenteNome,
            string textoPreview)
        {


            Application.Current.Dispatcher.Invoke(() =>
            {

                if (_chatWindow == null ||
                    !_chatWindow.IsVisible)
                {


                    var toast =
                        new ChatToastWindow(
                            remetenteNome,
                            textoPreview);



                    toast.Clicado += () =>
                        ChatConversar();



                    toast.Show();

                }

            });

        }



        #endregion

        #region Logout

        /// <summary>
        /// Chamado pelo <see cref="UsuarioSessaoService"/> quando o timer de inatividade expira.
        /// Exibe um aviso ao usuário e redireciona para a tela de login.
        /// </summary>
        private void OnSessaoExpirada()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    "Sua sessão expirou por inatividade. Faça login novamente.",
                    "Sessão Encerrada",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                RealizarLogout();
            });
        }

        /// <summary>
        /// Encerra a sessão do usuário: libera recursos, cancela eventos e retorna à tela de login.
        /// </summary>
        private void RealizarLogout()
        {
            /// Desassinar eventos primeiro (Evita Memory Leaks e NullReference)
            UsuarioSessaoService.Instancia.SessaoExpirada -= OnSessaoExpirada;
            ChatViewModel.NovaMensagemRecebida -= OnNovaMensagemRecebida;

            /// Encerrar os serviços e descartar ViewModels
            UsuarioSessaoService.Instancia.EncerrarSessao();
            ChatViewModel.Dispose();

            /// Transição de Janelas
            var loginWindow = new LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();

            _currentWindow.Close();
        }

        private void ExecutarSair()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var confirmWindow = new SairMensagemWindow
                {
                    Owner = Application.Current.MainWindow,
                    ShowInTaskbar = false
                };

                bool? resultado = confirmWindow.ShowDialog();

                if (resultado == true)
                {
                    confirmWindow.Owner = null; 
                    RealizarLogout();
                }
            });
        }


        #endregion

    }
}