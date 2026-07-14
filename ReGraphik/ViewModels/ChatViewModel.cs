using Firebase.Database;
using Firebase.Database.Query;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Views;
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

        /// <summary>
        /// Mantém as assinaturas dos listeners do Firebase para cancelamento
        /// </summary>
        private readonly List<IDisposable> _listeners = [];

        /// <summary>
        /// Rastreia IDs de mensagens já vistas para não re-notificar
        /// </summary>
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

        /// <summary>
        /// Disparado quando uma mensagem nova chega enquanto o chat está fechado.
        /// Parâmetros: nome do remetente, prévia do texto.
        /// </summary>
        public event Action<string, string>? NovaMensagemRecebida;

        /// <summary>
        /// Lista de conversas do usuário logado, exibida na lateral do painel de chat.
        /// </summary>
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

        /// <summary>
        /// Conversa atualmente aberta no painel de chat. Ao ser alterada, dispara o carregamento das mensagens correspondentes.
        /// </summary>
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
        /// Comando que alterna a visibilidade do painel de chat. Se o painel for aberto, carrega as conversas do usuário logado.
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
        /// Inicializa uma nova instância do ViewModel de chat, configurando o serviço de chat, comandos e listeners do Firebase.
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

            /// Carrega as conversas e atualiza notificações ao iniciar
            _ = CarregarConversasPublicAsync();
            _ = AtualizarNotificacoesAsync();

            /// Inicia a escuta em tempo real do Firebase para mensagens novas
            IniciarEscutaFirebase();

            /// Timer de segurança: garante sync caso algum evento Firebase seja perdido
            _timerAtualizacao = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(15)
            };
            _timerAtualizacao.Tick += async (_, _) => await AtualizarNotificacoesAsync();
            _timerAtualizacao.Start();
        }


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
                var db = FirebaseConfigService.Client;

                /// Assina o nó "mensagens" do Firebase para receber eventos em tempo real
                var listener = db
                    .Child("mensagens")
                    .AsObservable<Mensagem>()
                    .Subscribe(evento =>
                    {
                        /// Ignora eventos nulos ou de deleção
                        if (evento?.Object == null) return;

                        var msg = evento.Object;

                        /// Ignora mensagens que não são destinadas ao usuário logado ou que já foram vistas
                        if (msg.DestinatarioId != _usuarioLogado.Id) return;
                        if (_mensagensJaVistas.Contains(msg.Id)) return;

                        _mensagensJaVistas.Add(msg.Id);

                        Application.Current.Dispatcher.Invoke(async () =>
                        {
                            /// Atualiza o badge de não-lidas
                            await AtualizarNotificacoesAsync();

                            /// Se a conversa com este remetente estiver aberta, adiciona a mensagem em tempo real na lista de mensagens
                            if (ConversaSelecionada?.UsuarioId == msg.RemetenteId)
                            {
                                msg.EhMinhaMensagem = false;
                                Mensagens.Add(msg);

                                /// Marca como lida imediatamente
                                await _chatService.MarcarComoLidaAsync(
                                    msg.RemetenteId, _usuarioLogado.Id);
                                await AtualizarNotificacoesAsync();
                            }
                            else
                            {
                                /// Atualiza (ou cria) a linha da conversa na lista lateral
                                await AtualizarConversaNaListaAsync(msg);

                                /// Dispara o toast se o painel estiver fechado
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
            catch (Exception ex) 
            {
                /// Loga o erro no console de depuração para análise posterior
                System.Diagnostics.Debug.WriteLine($"Erro ao iniciar escuta do Firebase: {ex.Message}");

                /// Mostra uma mensagem de erro ao usuário, garantindo que seja executada na thread da UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro de Conexão", $"Erro ao iniciar escuta do Firebase!", MensagemWindow.TipoMensagem.Erro);
                });
            }
        }

        /// <summary>
        /// Atualiza ou insere a entrada da conversa na lista lateral
        /// sem precisar recarregar todas as conversas do Firebase.
        /// </summary>
        private async Task AtualizarConversaNaListaAsync(Mensagem msg)
        {
            try
            {
                var naoLidas = await _chatService.ContarNaoLidasAsync(
                    _usuarioLogado.Id, msg.RemetenteId);

                var existente = Conversas.FirstOrDefault(c => c.UsuarioId == msg.RemetenteId);

                if (existente != null)
                {
                    /// Atualiza dados da conversa existente
                    existente.UltimaMensagem = msg.Texto.Length > 40
                        ? msg.Texto[..40] + "..."
                        : msg.Texto;
                    existente.UltimaDataHora = msg.DataHora;
                    existente.MensagensNaoLidas = naoLidas;

                    /// Move a conversa para o topo da lista
                    Conversas.Remove(existente);
                    Conversas.Insert(0, existente);
                }
                else
                {
                    /// Nova conversa — cria a entrada na lista
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
            catch (Exception ex)
            {
                /// Loga o erro no console de depuração para análise posterior
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar conversa na lista: {ex.Message}");

                /// Mostra uma mensagem de erro ao usuário, garantindo que seja executada na thread da UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro de Conexão", $"Erro ao atualizar conversa na lista!", MensagemWindow.TipoMensagem.Erro);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task CarregarConversasPublicAsync()
            => await CarregarConversasAsync();

        /// <summary>
        /// Alterna a visibilidade do painel de chat. Se o painel for aberto, carrega as conversas do usuário logado.
        /// </summary>
        /// <returns></returns>
        private async Task AbrirChatAsync()
        {
            ChatAberto = !ChatAberto;
            if (ChatAberto)
                await CarregarConversasAsync();
        }

        /// <summary>
        /// Carrega a lista de conversas do usuário logado, obtendo a última mensagem e o número de mensagens não lidas para cada conversa.
        /// </summary>
        /// <returns></returns>
        private async Task CarregarConversasAsync()
        {
            Carregando = true;

            try
            {
                /// Obtém todos os usuários do sistema, exceto o usuário logado
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
            catch (Exception ex)
            {
                /// Loga o erro no console de depuração para análise posterior
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar conversas: {ex.Message}");
                /// Mostra uma mensagem de erro ao usuário, garantindo que seja executada na thread da UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro de Conexão", $"Erro ao carregar conversas!", MensagemWindow.TipoMensagem.Erro);
                });
            }
            finally
            {
                Carregando = false;
            }
        }

        /// <summary>
        /// Carrega as mensagens da conversa selecionada, marcando todas como lidas e atualizando o badge de não-lidas.
        /// </summary>
        /// <param name="outroUsuarioId"></param>
        /// <returns></returns>
        private async Task CarregarMensagensAsync(string outroUsuarioId)
        {
            Carregando = true;
            try
            {
                /// Obtém todas as mensagens entre o usuário logado e o outro usuário
                var msgs = await _chatService.ObterMensagensAsync(
                    _usuarioLogado.Id, outroUsuarioId);

                foreach (var m in msgs)
                {
                    m.EhMinhaMensagem = m.RemetenteId == _usuarioLogado.Id;
                    /// Marca todos os IDs como vistos para não re-notificar
                    _mensagensJaVistas.Add(m.Id);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Mensagens = new ObservableCollection<Mensagem>(
                        msgs.OrderBy(m => m.DataHora));
                });

                await _chatService.MarcarComoLidaAsync(outroUsuarioId, _usuarioLogado.Id);
                await AtualizarNotificacoesAsync();

                /// Atualiza o badge da conversa na lista lateral
                var conv = Conversas.FirstOrDefault(c => c.UsuarioId == outroUsuarioId);
                if (conv != null)
                {
                    conv.MensagensNaoLidas = 0;
                    /// Força atualização do binding (Conversa não implementa INotifyPropertyChanged)
                    var idx = Conversas.IndexOf(conv);
                    Conversas.RemoveAt(idx);
                    Conversas.Insert(idx, conv);
                }
            }
            catch (Exception ex)
            {
                /// Loga o erro no console de depuração para análise posterior
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar mensagens: {ex.Message}");

                /// Mostra uma mensagem de erro ao usuário, garantindo que seja executada na thread da UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro de Conexão", $"Erro ao carregar mensagens!", MensagemWindow.TipoMensagem.Erro);
                });
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task EnviarMensagemAsync()
        {
            try
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

                /// Marca como vista antes de enviar para não gerar notificação para si mesmo
                _mensagensJaVistas.Add(mensagem.Id);

                TextoMensagem = string.Empty;

                /// Adiciona localmente para resposta imediata (UX)
                Application.Current.Dispatcher.Invoke(() => Mensagens.Add(mensagem));

                await _chatService.EnviarMensagemAsync(mensagem);

                /// Atualiza a lista lateral de conversas
                await CarregarConversasAsync();
            }
            catch (Exception ex)
            {
                /// Loga o erro no console de depuração para análise posterior
                System.Diagnostics.Debug.WriteLine($"Erro ao enviar mensagem: {ex.Message}");

                /// Mostra uma mensagem de erro ao usuário, garantindo que seja executada na thread da UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro de Conexão", $"Erro ao enviar mensagem!", MensagemWindow.TipoMensagem.Erro);
                });
            }
        }

        /// <summary>
        /// Abre a tela de nova conversa, listando todos os usuários disponíveis para iniciar uma conversa.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Inicia uma nova conversa com o usuário selecionado. Se já existir uma conversa, apenas a seleciona. Impede iniciar conversa consigo mesmo.
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private async Task IniciarConversaComAsync(Usuario? usuario)
        {
            if (usuario == null) return;

            /// Impede conversa consigo mesmo
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

            /// Cria uma nova conversa com o usuário selecionado
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

        /// <summary>
        /// Atualiza o total de mensagens não lidas do usuário logado, somando todas as conversas. Atualiza a propriedade TotalNaoLidas para refletir no badge do painel de chat.
        /// </summary>
        /// <returns></returns>
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
            catch (Exception ex)
            {
                /// Loga o erro no console de depuração para análise posterior
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar notificações: {ex.Message}");
                /// Mostra uma mensagem de erro ao usuário, garantindo que seja executada na thread da UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro de Conexão", $"Erro ao atualizar notificações!", MensagemWindow.TipoMensagem.Erro);
                });
            }
        }

        /// <summary>
        /// Libera recursos, interrompe o timer de atualização e cancela todos os listeners do Firebase para evitar vazamentos de memória.
        /// </summary>
        public void Dispose()
        {
            _timerAtualizacao.Stop();

            /// Cancela todos os listeners do Firebase
            foreach (var l in _listeners)
                l.Dispose();
            _listeners.Clear();
        }
    }
}