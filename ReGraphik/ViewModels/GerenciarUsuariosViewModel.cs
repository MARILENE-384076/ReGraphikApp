using CommunityToolkit.Mvvm.Input;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Mail;
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
        private readonly ChatService _chatService;
        private readonly EmailService _emailService;

        /// <summary>
        /// Define o limite máximo de usuários cadastrados no sistema
        /// </summary>
        private const int LIMITE_MAXIMO_USUARIOS = 50;

        private string _nomeUsuario;
        public string NomeUsuario
        {
            get => _nomeUsuario;
            set
            {
                _nomeUsuario = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Usuario.Iniciais));
            }
        }

        /// <summary>
        /// Obtém o caminho ou URL da imagem de perfil do usuário logado diretamente do Firebase.
        /// </summary>
        public string? FotoPerfil
        {
            get
            {
                string? caminho = UsuarioSessaoService.Instancia.FotoCaminho;

                if (string.IsNullOrWhiteSpace(caminho))
                    return null;

                return caminho.Trim();
            }
        }

        /// <summary>
        /// Lista de usuários cadastrados no sistema, exibida na interface.
        /// </summary>
        private ObservableCollection<Usuario> _usuarios = [];
        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set { _usuarios = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// E-mail do novo usuário a ser convidado, preenchido pelo administrador na interface.
        /// </summary>
        private string _emailConvite = string.Empty;
        public string EmailConvite
        {
            get => _emailConvite;
            set { _emailConvite = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Perfil do novo usuário a ser convidado, selecionado pelo administrador na interface.
        /// </summary>
        private string _perfilSelecionado = "Usuário";
        public string PerfilSelecionado
        {
            get => _perfilSelecionado;
            set
            {
                _perfilSelecionado = value;
                OnPropertyChanged(nameof(PerfilSelecionado));
            }
        }

        /// <summary>
        /// Token gerado (exibido após gerar convite)
        /// </summary>
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

        /// <summary>
        /// Estados de UI
        /// </summary>
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

        /// <summary>
        /// Limite de tokens restantes para convites, lido diretamente das configurações persistentes do app.
        /// </summary>
        public int LimiteTokensRestantes
        {
            get => Properties.Settings.Default.LimiteTokensRestantes;
            set
            {
                Properties.Settings.Default.LimiteTokensRestantes = value;
                Properties.Settings.Default.Save(); /// Salva no arquivo físico permanentemente
                OnPropertyChanged(nameof(LimiteTokensRestantes));
            }
        }

        /// <summary>
        /// Comandos para a interface
        /// </summary>
        public ICommand CarregarUsuariosCommand { get; }
        public ICommand GerarConviteCommand { get; }
        public ICommand CopiarTokenCommand { get; }
        public ICommand LimparConviteCommand { get; }

        /// <summary>
        /// Construtor da classe
        /// </summary>
        public GerenciarUsuariosViewModel()
        {
            _conviteService = new ConviteService();
            _chatService = new ChatService();
            _emailService = new EmailService();

            /// Vincular evento de sessão primeiro para não perder nenhuma atualização
            UsuarioSessaoService.Instancia.PropertyChanged += OnUsuarioSessaoPropertyChanged;

            /// Inicializar dados com segurança
            AtualizarDadosSessao();

            CarregarUsuariosCommand = new RelayCommand(async _ => await CarregarUsuariosAsync());
            GerarConviteCommand = new RelayCommand(async _ => await GerarConviteAsync(), _ => !GerandoConvite);
            CopiarTokenCommand = new RelayCommand(_ => CopiarToken(), _ => TokenGeradoVisivel);
            LimparConviteCommand = new RelayCommand(_ => LimparConvite());

            _ = CarregarUsuariosAsync();
        }

        /// <summary>
        /// Lê os dados atuais do serviço de sessão e força a atualização na UI.
        /// </summary>
        private void AtualizarDadosSessao()
        {
            /// Busca o nome do usuário de forma segura
            NomeUsuario = UsuarioSessaoService.Instancia.UsuarioLogado?.Nome ?? string.Empty;

            /// Força a atualização das propriedades dependentes na UI
            OnPropertyChanged(nameof(FotoPerfil));
            OnPropertyChanged(nameof(Usuario.Iniciais));
        }

        /// <summary>
        /// Manipula a mudança de propriedades no serviço de sessão do usuário.
        /// </summary>
        private void OnUsuarioSessaoPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            /// Se qualquer propriedade relevante da sessão mudar, atualizamos tudo de forma segura
            if (e.PropertyName == nameof(UsuarioSessaoService.FotoCaminho) ||
                e.PropertyName == nameof(UsuarioSessaoService.UsuarioLogado))
            {
                /// Executa na Thread principal da UI para evitar problemas de sincronização no WPF
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AtualizarDadosSessao();
                });
            }
        }

        /// <summary>
        /// Carregar lista de usuários
        /// </summary>
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

        /// <summary>
        /// Gerar convite para cadastro
        /// </summary>
        private async Task GerarConviteAsync()
        {
            LimparMensagens();

            /// Validação do limite de tokens gerados pelo administrador 
            if (LimiteTokensRestantes <= 0)
            {
                MensagemErro = "Você atingiu o limite de 50 tokens gerados nesta sessão.";
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir(
                        "Limite de Tokens Atingido",
                        "Não é possível gerar novos tokens. O limite de 50 envios foi alcançado.",
                        MensagemWindow.TipoMensagem.Aviso
                    );
                });
                return;
            }

            /// Validação preventiva do limite de usuários antes de disparar o convite
            if (Usuarios != null && Usuarios.Count >= LIMITE_MAXIMO_USUARIOS)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir(
                        "Limite Atingido",
                        $"O sistema atingiu o limite máximo de {LIMITE_MAXIMO_USUARIOS} usuários ativos permitidos nesta licença.",
                        MensagemWindow.TipoMensagem.Aviso
                    );
                });
                return;
            }

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

            /// Verifica se o e-mail já tem um convite pendente
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
                string emailDestino = EmailConvite.Trim();

                /// Gera o token no Firebase
                string token = await _conviteService.GerarConviteAsync(emailDestino, perfilUsuario);
                TokenGerado = token;

                /// Envia o e-mail com o token gerado
                await _emailService.EnviarTokenPorEmailAsync(emailDestino, token);

                /// Decrementa o contador local após o sucesso na geração
                LimiteTokensRestantes--;

                /// Mensagem de sucesso atualizada informando que também foi enviado por e-mail
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir(
                        "Convite Enviado!",
                        $"Convite gerado com sucesso para {emailDestino} e enviado por e-mail.",
                        MensagemWindow.TipoMensagem.Sucesso
                    );
                });

                Mensagem = "Convite ativo. O token também está disponível abaixo.";

                /// Força atualização da lista de usuários para garantir o contador local correto
                _ = CarregarUsuariosAsync();
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

        /// <summary>
        /// Copiar token para área de transferência 
        /// </summary>
        private void CopiarToken()
        {
            if (string.IsNullOrEmpty(TokenGerado)) return;
            Clipboard.SetText(TokenGerado);
            Mensagem = "Token copiado para a área de transferência!";
        }

        /// <summary>
        /// Limpar formulário de convite
        /// </summary>
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