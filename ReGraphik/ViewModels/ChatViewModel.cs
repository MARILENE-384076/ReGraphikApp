using ReGraphik.Models;
using ReGraphik.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel responsável por gerenciar o painel de chat entre usuários.
    /// Controla lista de conversas, carregamento de mensagens e envio.
    /// </summary>
    public class ChatViewModel : BaseViewModel
    {
        private readonly ChatService _chatService;
        private readonly Usuario _usuarioLogado;
        private readonly DispatcherTimer _timerAtualizacao;

        
        private ObservableCollection<Conversa> _conversas = [];
        private ObservableCollection<Mensagem> _mensagens = [];
        private ObservableCollection<Usuario> _usuariosDisponiveis = [];
        private Conversa? _conversaSelecionada;
        private string _textoMensagem = string.Empty;
        private bool _chatAberto;
        private bool _mostrarNovaConversa;
        private int _totalNaoLidas;
        private bool _carregando;

        public ObservableCollection<Conversa> Conversas
        {
            get => _conversas;
            set { _conversas = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Mensagem> Mensagens
        {
            get => _mensagens;
            set { _mensagens = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Usuario> UsuariosDisponiveis
        {
            get => _usuariosDisponiveis;
            set { _usuariosDisponiveis = value; OnPropertyChanged(); }
        }

        public Conversa? ConversaSelecionada
        {
            get => _conversaSelecionada;
            set
            {
                _conversaSelecionada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TemConversaSelecionada));
                OnPropertyChanged(nameof(NomeConversaAtual));
                if (value != null)
                    _ = CarregarMensagensAsync(value.UsuarioId);
            }
        }

        public string TextoMensagem
        {
            get => _textoMensagem;
            set { _textoMensagem = value; OnPropertyChanged(); }
        }

        public bool ChatAberto
        {
            get => _chatAberto;
            set { _chatAberto = value; OnPropertyChanged(); }
        }

        public bool MostrarNovaConversa
        {
            get => _mostrarNovaConversa;
            set { _mostrarNovaConversa = value; OnPropertyChanged(); }
        }

        public int TotalNaoLidas
        {
            get => _totalNaoLidas;
            set { _totalNaoLidas = value; OnPropertyChanged(); OnPropertyChanged(nameof(TemMensagensNaoLidas)); }
        }

        public bool TemMensagensNaoLidas => TotalNaoLidas > 0;
        public bool TemConversaSelecionada => ConversaSelecionada != null;
        public string NomeConversaAtual => ConversaSelecionada?.UsuarioNome ?? string.Empty;

        public bool Carregando
        {
            get => _carregando;
            set { _carregando = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comandos
        /// </summary>
        public ICommand AbrirChatCommand { get; }
        public ICommand FecharChatCommand { get; }
        public ICommand EnviarMensagemCommand { get; }
        public ICommand AbrirNovaConversaCommand { get; }
        public ICommand FecharNovaConversaCommand { get; }
        public ICommand IniciarConversaCommand { get; }
        public ICommand SelecionarConversaCommand { get; }
        public ICommand VoltarListaCommand { get; }

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="usuarioLogado"></param>
        public ChatViewModel(Usuario usuarioLogado)
        {
            _usuarioLogado = usuarioLogado;
            _chatService = new ChatService();

            AbrirChatCommand = new RelayCommand(async () => await AbrirChatAsync());
            FecharChatCommand = new RelayCommand(() => ChatAberto = false);
            EnviarMensagemCommand = new RelayCommand(async () => await EnviarMensagemAsync());
            AbrirNovaConversaCommand = new RelayCommand(async () => await AbrirNovaConversaAsync());
            FecharNovaConversaCommand = new RelayCommand(() => MostrarNovaConversa = false);
            IniciarConversaCommand = new RelayCommand<Usuario>(async u => await IniciarConversaComAsync(u));
            SelecionarConversaCommand = new RelayCommand<Conversa>(c => ConversaSelecionada = c);
            VoltarListaCommand = new RelayCommand(() => ConversaSelecionada = null);

            /// Timer para verificar novas mensagens a cada 15 segundos
            _timerAtualizacao = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _timerAtualizacao.Tick += async (_, _) => await AtualizarNotificacoesAsync();
            _timerAtualizacao.Start();

            /// Carrega contagem inicial de não lidas
            _ = AtualizarNotificacoesAsync();
        }

        /// <summary>
        /// Métodos privados
        /// </summary>
        /// <returns></returns>
        private async Task AbrirChatAsync()
        {
            ChatAberto = !ChatAberto;

            if (ChatAberto)
            {
                await CarregarConversasAsync();
            }
        }

        private async Task CarregarConversasAsync()
        {
            Carregando = true;
            try
            {
                var usuarios = await _chatService.ListarUsuariosAsync();
                var conversas = new List<Conversa>();

                foreach (var u in usuarios.Where(u => u.Id != _usuarioLogado.Id))
                {
                    var msgs = await _chatService.ObterMensagensAsync(_usuarioLogado.Id, u.Id);
                    if (!msgs.Any()) continue;

                    var ultima = msgs.OrderByDescending(m => m.DataHora).First();
                    var naoLidas = await _chatService.ContarNaoLidasAsync(_usuarioLogado.Id, u.Id);

                    conversas.Add(new Conversa
                    {
                        UsuarioId = u.Id,
                        UsuarioNome = u.Nome ?? u.Login ?? "Usuário",
                        UltimaMensagem = ultima.Texto.Length > 40
                            ? ultima.Texto[..40] + "..."
                            : ultima.Texto,
                        UltimaDataHora = ultima.DataHora,
                        MensagensNaoLidas = naoLidas
                    });
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Conversas = new ObservableCollection<Conversa>(
                        conversas.OrderByDescending(c => c.UltimaDataHora));
                });
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task CarregarMensagensAsync(string outroUsuarioId)
        {
            Carregando = true;
            try
            {
                var msgs = await _chatService.ObterMensagensAsync(_usuarioLogado.Id, outroUsuarioId);

                foreach (var m in msgs)
                    m.EhMinhaMensagem = m.RemetenteId == _usuarioLogado.Id;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mensagens = new ObservableCollection<Mensagem>(
                        msgs.OrderBy(m => m.DataHora));
                });

                /// Marca como lidas
                await _chatService.MarcarComoLidaAsync(outroUsuarioId, _usuarioLogado.Id);
                await AtualizarNotificacoesAsync();
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task EnviarMensagemAsync()
        {
            if (string.IsNullOrWhiteSpace(TextoMensagem) || ConversaSelecionada == null)
                return;

            var mensagem = new Mensagem
            {
                Id = Guid.NewGuid().ToString(),
                RemetenteId = _usuarioLogado.Id,
                RemetenteNome = _usuarioLogado.Nome ?? "Usuário",
                DestinatarioId = ConversaSelecionada.UsuarioId,
                Texto = TextoMensagem.Trim(),
                DataHora = DateTime.Now,
                Lida = false,
                EhMinhaMensagem = true
            };

            TextoMensagem = string.Empty;

            /// Adiciona localmente para resposta imediata (UX)
            Application.Current.Dispatcher.Invoke(() => Mensagens.Add(mensagem));

            await _chatService.EnviarMensagemAsync(mensagem);

            /// Atualiza lista de conversas
            await CarregarConversasAsync();
        }

        private async Task AbrirNovaConversaAsync()
        {
            var usuarios = await _chatService.ListarUsuariosAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                UsuariosDisponiveis = new ObservableCollection<Usuario>(
                    usuarios.Where(u => u.Id != _usuarioLogado.Id));
            });
            MostrarNovaConversa = true;
        }

        private async Task IniciarConversaComAsync(Usuario? usuario)
        {
            if (usuario == null) return;

            MostrarNovaConversa = false;

            /// Verifica se conversa já existe na lista
            var existente = Conversas.FirstOrDefault(c => c.UsuarioId == usuario.Id);
            if (existente != null)
            {
                ConversaSelecionada = existente;
                return;
            }
            
            /// Cria conversa nova localmente
            var novaConversa = new Conversa
            {
                UsuarioId = usuario.Id,
                UsuarioNome = usuario.Nome ?? usuario.Login ?? "Usuário",
                UltimaMensagem = "Iniciar conversa...",
                UltimaDataHora = DateTime.Now,
                MensagensNaoLidas = 0
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                Conversas.Insert(0, novaConversa);
            });

            ConversaSelecionada = novaConversa;
        }

        private async Task AtualizarNotificacoesAsync()
        {
            try
            {
                var usuarios = await _chatService.ListarUsuariosAsync();
                int total = 0;

                foreach (var u in usuarios.Where(u => u.Id != _usuarioLogado.Id))
                    total += await _chatService.ContarNaoLidasAsync(_usuarioLogado.Id, u.Id);

                Application.Current.Dispatcher.Invoke(() => TotalNaoLidas = total);
            }
            catch { /* silencioso */ }
        }

        public void Dispose()
        {
            _timerAtualizacao.Stop();
        }
    }
}
