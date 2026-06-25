using ReGraphik.Models;
using ReGraphik.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel da tela de Gerenciamento de Usuários.
    /// Acessível apenas pelo perfil Administrador.
    /// Permite listar usuários, convidar novos e inativar existentes.
    /// </summary>
    public class GerenciarUsuariosViewModel : BaseViewModel
    {
        private readonly ConviteService _conviteService;
        private readonly ChatService _chatService; // reutiliza ListarUsuariosAsync

        // ── lista de usuários ─────────────────────────────────────────────
        private ObservableCollection<Usuario> _usuarios = [];
        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set { _usuarios = value; OnPropertyChanged(); }
        }

        // ── campo e-mail do convite ───────────────────────────────────────
        private string _emailConvite = string.Empty;
        public string EmailConvite
        {
            get => _emailConvite;
            set { _emailConvite = value; OnPropertyChanged(); }
        }

        private string _perfilSelecionado = "Usuário"; // Valor padrão
        public string PerfilSelecionado
        {
            get => _perfilSelecionado;
            set
            {
                _perfilSelecionado = value;
                OnPropertyChanged(nameof(PerfilSelecionado)); // Ou SetProperty se usar CommunityToolkit
            }
        }

        // ── token gerado (exibido após gerar convite) ─────────────────────
        private string _tokenGerado = string.Empty;
        public string TokenGerado
        {
            get => _tokenGerado;
            set
            {
                _tokenGerado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TokenGeradoVisivel));
            }
        }
        public bool TokenGeradoVisivel => !string.IsNullOrEmpty(TokenGerado);

        // ── estados de UI ─────────────────────────────────────────────────
        private bool _carregando;
        public bool Carregando
        {
            get => _carregando;
            set { _carregando = value; OnPropertyChanged(); }
        }

        private bool _gerandoConvite;
        public bool GerandoConvite
        {
            get => _gerandoConvite;
            set { _gerandoConvite = value; OnPropertyChanged(); }
        }

        private string _mensagem = string.Empty;
        public string Mensagem
        {
            get => _mensagem;
            set { _mensagem = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemMensagem)); }
        }
        public bool TemMensagem => !string.IsNullOrEmpty(Mensagem);

        private string _mensagemErro = string.Empty;
        public string MensagemErro
        {
            get => _mensagemErro;
            set { _mensagemErro = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemErro)); }
        }
        public bool TemErro => !string.IsNullOrEmpty(MensagemErro);

        // ── commands ──────────────────────────────────────────────────────
        public ICommand CarregarUsuariosCommand { get; }
        public ICommand GerarConviteCommand { get; }
        public ICommand CopiarTokenCommand { get; }
        public ICommand LimparConviteCommand { get; }

        // ── construtor ────────────────────────────────────────────────────
        public GerenciarUsuariosViewModel()
        {
            _conviteService = new ConviteService();
            _chatService = new ChatService();

            CarregarUsuariosCommand = new RelayCommand(async _ => await CarregarUsuariosAsync());
            GerarConviteCommand = new RelayCommand(async _ => await GerarConviteAsync(), _ => !GerandoConvite);
            CopiarTokenCommand = new RelayCommand(_ => CopiarToken(), _ => TokenGeradoVisivel);
            LimparConviteCommand = new RelayCommand(_ => LimparConvite());

            _ = CarregarUsuariosAsync();
        }

        // ── carregar lista de usuários ────────────────────────────────────
        private async Task CarregarUsuariosAsync()
        {
            Carregando = true;
            try
            {
                var lista = await _chatService.ListarUsuariosAsync();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Usuarios = new ObservableCollection<Usuario>(
                        lista.OrderBy(u => u.Nome));
                });
            }
            catch
            {
                MensagemErro = "Erro ao carregar usuários.";
            }
            finally
            {
                Carregando = false;
            }
        }

        // ── gerar convite ─────────────────────────────────────────────────
        private async Task GerarConviteAsync()
        {
            LimparMensagens();

            if (string.IsNullOrWhiteSpace(EmailConvite))
            {
                MensagemErro = "Informe o e-mail do novo usuário.";
                return;
            }

            if (!EmailConvite.Contains('@') || !EmailConvite.Contains('.'))
            {
                MensagemErro = "Informe um e-mail válido.";
                return;
            }

            // Verifica se o e-mail já tem um convite pendente
            bool jaTemConvite = await _conviteService.ExisteConvitePendenteAsync(EmailConvite.Trim());
            if (jaTemConvite)
            {
                MensagemErro = "Este e-mail já possui um convite ativo. Aguarde expirar (48h) ou verifique o Firebase.";
                return;
            }

            try
            {
                GerandoConvite = true;

                string perfilUsuario = PerfilSelecionado == "Administrador" ? "Admin" : "User";

                string token = await _conviteService.GerarConviteAsync(EmailConvite.Trim(), perfilUsuario);
                TokenGerado = token;

                Mensagem = $"Convite gerado com sucesso para {EmailConvite}. " +
                           $"Envie o token abaixo ao usuário. Válido por 48 horas.";
            }
            catch
            {
                MensagemErro = "Erro ao gerar o convite. Verifique a conexão com o Firebase.";
            }
            finally
            {
                GerandoConvite = false;
            }
        }

        // ── copiar token para área de transferência ───────────────────────
        private void CopiarToken()
        {
            if (string.IsNullOrEmpty(TokenGerado)) return;
            Clipboard.SetText(TokenGerado);
            Mensagem = "Token copiado para a área de transferência!";
        }

        // ── limpar formulário de convite ──────────────────────────────────
        private void LimparConvite()
        {
            EmailConvite = string.Empty;
            TokenGerado = string.Empty;
            LimparMensagens();
        }

        private void LimparMensagens()
        {
            Mensagem = string.Empty;
            MensagemErro = string.Empty;
        }
    }
}
