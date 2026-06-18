using Firebase.Database;
using Firebase.Database.Query;
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
    /// Controla lista de conversas, carregamento de mensagens, envio
    /// e escuta em tempo real de novas mensagens via Firebase.
    /// </summary>
    public class ChatViewModel : BaseViewModel
    {
        private readonly ChatService _chatService;
        private readonly Usuario _usuarioLogado;
        private readonly DispatcherTimer _timerAtualizacao;

        // Mantém as assinaturas dos listeners do Firebase para cancelamento
        private readonly List<IDisposable> _listeners = [];

        // Rastreia IDs de mensagens já vistas para não re-notificar
        private readonly HashSet<string> _mensagensJaVistas = [];

        private ObservableCollection<Conversa> _conversas = [];
        private ObservableCollection<Mensagem> _mensagens = [];
        private ObservableCollection<Usuario> _usuariosDisponiveis = [];
        private Conversa? _conversaSelecionada;
        private string _textoMensagem = string.Empty;
        private bool _chatAberto;
        private bool _mostrarNovaConversa;
        private int _totalNaoLidas;
        private bool _carregando;

        // ── evento disparado quando chega mensagem nova de outro usuário ─────
        /// <summary>
        /// Disparado quando uma mensagem nova chega enquanto o chat está fechado.
        /// Parâmetros: nome do remetente, prévia do texto.
        /// </summary>
        public event Action<string, string>? NovaMensagemRecebida;

        // ── propriedades ─────────────────────────────────────────────────────
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

        // ── commands ─────────────────────────────────────────────────────────
        public ICommand AbrirChatCommand { get; }
        public ICommand FecharChatCommand { get; }
        public ICommand EnviarMensagemCommand { get; }
        public ICommand AbrirNovaConversaCommand { get; }
        public ICommand FecharNovaConversaCommand { get; }
        public ICommand IniciarConversaCommand { get; }
        public ICommand SelecionarConversaCommand { get; }
        public ICommand VoltarListaCommand { get; }

        // ── construtor ───────────────────────────────────────────────────────
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

            // Carrega conversas e notificações ao inicializar —
            // assim o badge já aparece antes de o usuário abrir o chat.
            _ = CarregarConversasPublicAsync();
            _ = AtualizarNotificacoesAsync();

            // Inicia a escuta em tempo real do Firebase para mensagens novas
            IniciarEscutaFirebase();

            // Timer de segurança: garante sync caso algum evento Firebase seja perdido
            _timerAtualizacao = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _timerAtualizacao.Tick += async (_, _) => await AtualizarNotificacoesAsync();
            _timerAtualizacao.Start();
        }

        // ── escuta Firebase em tempo real ────────────────────────────────────

        /// <summary>
        /// Assina o nó raiz "mensagens" do Firebase e reage a qualquer
        /// inserção nova. Para cada mensagem nova destinada ao usuário logado:
        ///   1. Atualiza o badge de não-lidas;
        ///   2. Se a conversa afetada estiver aberta, adiciona a mensagem na lista;
        ///   3. Se o painel estiver fechado, dispara o evento NovaMensagemRecebida
        ///      para o MainViewModel mostrar o toast.
        /// </summary>
        private void IniciarEscutaFirebase()
        {
            try
            {
                var db = FirebaseConfig.Client;

                // Assina todas as conversas que contêm o ID do usuário logado.
                // O Firebase SDK chama o callback a cada inserção/alteração no nó.
                var listener = db
                    .Child("mensagens")
                    .AsObservable<Mensagem>()
                    .Subscribe(evento =>
                    {
                        // Ignora eventos nulos ou de deleção
                        if (evento?.Object == null) return;

                        var msg = evento.Object;

                        // Só processa mensagens destinadas ao usuário logado
                        // e que ainda não foram processadas nesta sessão
                        if (msg.DestinatarioId != _usuarioLogado.Id) return;
                        if (_mensagensJaVistas.Contains(msg.Id)) return;

                        _mensagensJaVistas.Add(msg.Id);

                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            // 1. Atualiza o badge de não-lidas
                            await AtualizarNotificacoesAsync();

                            // 2. Se a conversa com este remetente estiver aberta,
                            //    adiciona a mensagem em tempo real na lista de mensagens
                            if (ConversaSelecionada?.UsuarioId == msg.RemetenteId)
                            {
                                msg.EhMinhaMensagem = false;
                                Mensagens.Add(msg);

                                // Marca como lida imediatamente
                                await _chatService.MarcarComoLidaAsync(
                                    msg.RemetenteId, _usuarioLogado.Id);
                                await AtualizarNotificacoesAsync();
                            }
                            else
                            {
                                // 3. Atualiza (ou cria) a linha da conversa na lista lateral
                                await AtualizarConversaNaListaAsync(msg);

                                // 4. Dispara o toast se o painel estiver fechado
                                if (!ChatAberto)
                                {
                                    var preview = msg.Texto.Length > 50
                                        ? msg.Texto[..50] + "..."
                                        : msg.Texto;
                                    NovaMensagemRecebida?.Invoke(msg.RemetenteNome, preview);
                                }
                            }
                        });
                    });

                _listeners.Add(listener);
            }
            catch
            {
                // Se o Firebase não estiver disponível, o timer de 15s garante sync
            }
        }

        /// <summary>
        /// Atualiza ou insere a entrada da conversa na lista lateral
        /// sem precisar recarregar todas as conversas do Firebase.
        /// </summary>
        private async Task AtualizarConversaNaListaAsync(Mensagem msg)
        {
            var naoLidas = await _chatService.ContarNaoLidasAsync(
                _usuarioLogado.Id, msg.RemetenteId);

            var existente = Conversas.FirstOrDefault(c => c.UsuarioId == msg.RemetenteId);

            if (existente != null)
            {
                // Atualiza dados da conversa existente
                existente.UltimaMensagem = msg.Texto.Length > 40
                    ? msg.Texto[..40] + "..."
                    : msg.Texto;
                existente.UltimaDataHora = msg.DataHora;
                existente.MensagensNaoLidas = naoLidas;

                // Move a conversa para o topo da lista
                Conversas.Remove(existente);
                Conversas.Insert(0, existente);
            }
            else
            {
                // Nova conversa — cria a entrada na lista
                Conversas.Insert(0, new Conversa
                {
                    UsuarioId = msg.RemetenteId,
                    UsuarioNome = msg.RemetenteNome,
                    UltimaMensagem = msg.Texto.Length > 40
                        ? msg.Texto[..40] + "..."
                        : msg.Texto,
                    UltimaDataHora = msg.DataHora,
                    MensagensNaoLidas = naoLidas
                });
            }
        }

        // ── métodos públicos ─────────────────────────────────────────────────

        /// <summary>
        /// Carrega todas as conversas do usuário logado.
        /// Exposto como público para que o MainViewModel possa chamá-lo
        /// ao abrir o painel de chat.
        /// </summary>
        public async Task CarregarConversasPublicAsync()
            => await CarregarConversasAsync();

        // ── métodos privados ─────────────────────────────────────────────────
        private async Task AbrirChatAsync()
        {
            ChatAberto = !ChatAberto;
            if (ChatAberto)
                await CarregarConversasAsync();
        }

        private async Task CarregarConversasAsync()
        {
            Carregando = true;
            try
            {
                var usuarios = await _chatService.ListarUsuariosAsync();
                var conversas = new List<Conversa>();

                foreach (var u in usuarios.Where(u =>
                    !string.IsNullOrEmpty(u.Id) &&
                    u.Id != _usuarioLogado.Id &&
                    u.Login != _usuarioLogado.Login))
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
                var msgs = await _chatService.ObterMensagensAsync(
                    _usuarioLogado.Id, outroUsuarioId);

                foreach (var m in msgs)
                {
                    m.EhMinhaMensagem = m.RemetenteId == _usuarioLogado.Id;
                    // Marca todos os IDs como vistos para não re-notificar
                    _mensagensJaVistas.Add(m.Id);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mensagens = new ObservableCollection<Mensagem>(
                        msgs.OrderBy(m => m.DataHora));
                });

                await _chatService.MarcarComoLidaAsync(outroUsuarioId, _usuarioLogado.Id);
                await AtualizarNotificacoesAsync();

                // Atualiza o badge da conversa na lista lateral
                var conv = Conversas.FirstOrDefault(c => c.UsuarioId == outroUsuarioId);
                if (conv != null)
                {
                    conv.MensagensNaoLidas = 0;
                    // Força atualização do binding (Conversa não implementa INotifyPropertyChanged)
                    var idx = Conversas.IndexOf(conv);
                    Conversas.RemoveAt(idx);
                    Conversas.Insert(idx, conv);
                }
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

            // Marca como vista antes de enviar para não gerar notificação para si mesmo
            _mensagensJaVistas.Add(mensagem.Id);

            TextoMensagem = string.Empty;

            // Adiciona localmente para resposta imediata (UX)
            Application.Current.Dispatcher.Invoke(() => Mensagens.Add(mensagem));

            await _chatService.EnviarMensagemAsync(mensagem);

            // Atualiza a lista lateral de conversas
            await CarregarConversasAsync();
        }

        private async Task AbrirNovaConversaAsync()
        {
            var usuarios = await _chatService.ListarUsuariosAsync();
            Application.Current.Dispatcher.Invoke(() =>
            {
                UsuariosDisponiveis = new ObservableCollection<Usuario>(
                    usuarios.Where(u =>
                        !string.IsNullOrEmpty(u.Id) &&
                        u.Id != _usuarioLogado.Id &&
                        u.Login != _usuarioLogado.Login));
            });
            MostrarNovaConversa = true;
        }

        private async Task IniciarConversaComAsync(Usuario? usuario)
        {
            if (usuario == null) return;

            // Impede conversa consigo mesmo
            if (usuario.Id == _usuarioLogado.Id ||
                usuario.Login == _usuarioLogado.Login)
            {
                MostrarNovaConversa = false;
                return;
            }

            MostrarNovaConversa = false;

            var existente = Conversas.FirstOrDefault(c => c.UsuarioId == usuario.Id);
            if (existente != null)
            {
                ConversaSelecionada = existente;
                return;
            }

            var novaConversa = new Conversa
            {
                UsuarioId = usuario.Id,
                UsuarioNome = usuario.Nome ?? usuario.Login ?? "Usuário",
                UltimaMensagem = "Iniciar conversa...",
                UltimaDataHora = DateTime.Now,
                MensagensNaoLidas = 0
            };

            Application.Current.Dispatcher.Invoke(() =>
                Conversas.Insert(0, novaConversa));

            ConversaSelecionada = novaConversa;
        }

        private async Task AtualizarNotificacoesAsync()
        {
            try
            {
                var usuarios = await _chatService.ListarUsuariosAsync();
                int total = 0;

                foreach (var u in usuarios.Where(u =>
                    !string.IsNullOrEmpty(u.Id) &&
                    u.Id != _usuarioLogado.Id))
                {
                    total += await _chatService.ContarNaoLidasAsync(_usuarioLogado.Id, u.Id);
                }

                Application.Current.Dispatcher.Invoke(() => TotalNaoLidas = total);
            }
            catch { /* silencioso */ }
        }

        public void Dispose()
        {
            _timerAtualizacao.Stop();

            // Cancela todos os listeners do Firebase
            foreach (var l in _listeners)
                l.Dispose();
            _listeners.Clear();
        }
    }
}