using ReGraphik.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ReGraphik.Services
{/// <summary>
 /// Serviço singleton que mantém o estado do usuário logado durante a sessão,
 /// incluindo foto de perfil, dados do usuário e controle de logout automático por inatividade.
 /// </summary>
    public class UsuarioSessaoService : INotifyPropertyChanged
    {
        // ─── Singleton ───────────────────────────────────────────────────────────────
        private static UsuarioSessaoService? _instancia;

        /// <summary>Instância única do serviço de sessão (padrão Singleton).</summary>
        public static UsuarioSessaoService Instancia => _instancia ??= new UsuarioSessaoService();

        // ─── Constante de inatividade ─────────────────────────────────────────────
        /// <summary>
        /// Tempo máximo de inatividade antes do logout automático (em minutos).
        /// </summary>
        private const int MinutosInatividade = 15;

        // ─── Timer de inatividade ─────────────────────────────────────────────────
        private readonly DispatcherTimer _timerInatividade;

        // ─── Propriedades de sessão ───────────────────────────────────────────────
        private string? _fotoCaminho;
        private Usuario? _usuarioLogado;

        /// <summary>
        /// Caminho local da foto de perfil do usuário, compartilhado entre todas as Views.
        /// </summary>
        public string? FotoCaminho
        {
            get => _fotoCaminho;
            set { _fotoCaminho = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Objeto do usuário atualmente autenticado na sessão.
        /// </summary>
        public Usuario? UsuarioLogado
        {
            get => _usuarioLogado;
            set
            {
                _usuarioLogado = value;
                OnPropertyChanged();
                // CRÍTICO: Avisa a UI e as ViewModels que o atalho do ID também mudou!
                OnPropertyChanged(nameof(IdUsuarioLogado));
            }
        }

        /// <summary>
        /// ID do usuário logado. Atalho para <see cref="UsuarioLogado"/>?.Id.
        /// Usado pelos ViewModels para associar registros ao usuário da sessão.
        /// </summary>
        public string? IdUsuarioLogado => _usuarioLogado?.Id;

        // ─── Evento de logout por inatividade ────────────────────────────────────
        /// <summary>
        /// Disparado quando o sistema detecta inatividade superior ou quando uma expiração é forçada.
        /// Assine este evento no MainViewModel para executar o fluxo de saída da janela.
        /// </summary>
        public event Action? SessaoExpirada;

        // ─── Construtor privado ───────────────────────────────────────────────────
        private UsuarioSessaoService()
        {
            _timerInatividade = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(MinutosInatividade)
            };
            _timerInatividade.Tick += OnInatividade;
        }

        // ─── API pública ──────────────────────────────────────────────────────────

        /// <summary>
        /// Inicia o monitoramento de inatividade. Deve ser chamado logo após o login bem-sucedido.
        /// </summary>
        public void IniciarSessao(Usuario usuario)
        {
            /// 1. Força a parada total do timer anterior para não carregar lixo em memória
            _timerInatividade.Stop();

            /// 2. Define o usuário
            UsuarioLogado = usuario;

            /// 3. Inicia o timer do zero absoluto
            ResetarTimer();
        }

        /// <summary>
        /// Reinicia o contador de inatividade. Deve ser chamado a cada interação relevante do usuário.
        /// </summary>
        public void ResetarTimer()
        {
            /// No WPF, para garantir o reset real do intervalo de um DispatcherTimer,
            /// o método mais seguro é desativar e reativar a propriedade 'IsEnabled'
            _timerInatividade.Stop();
            _timerInatividade.IsEnabled = false;

            /// Define o intervalo novamente para garantir que ele conte os 15 minutos cheios a partir de AGORA
            _timerInatividade.Interval = TimeSpan.FromMinutes(MinutosInatividade);

            _timerInatividade.IsEnabled = true;
            _timerInatividade.Start();
        }

        /// <summary>
        /// Encerra a sessão: para o timer, limpa os dados do usuário logado.
        /// Deve ser chamado no fluxo de logout manual e no callback de expiração.
        /// </summary>
        public void EncerrarSessao()
        {
            _timerInatividade.Stop();
            UsuarioLogado = null;
            FotoCaminho = null;
        }

        /// <summary>
        /// Força o encerramento imediato da sessão de forma programática.
        /// Útil se uma requisição de API falhar por Token expirado ou nulo fora do tempo do Timer.
        /// </summary>
        public void ForçarExpiracaoSessao()
        {
            EncerrarSessao();
            SessaoExpirada?.Invoke();
        }

        // ─── Callback interno ─────────────────────────────────────────────────────

        /// <summary>
        /// Chamado pelo timer quando o período de inatividade expira.
        /// Encerra a sessão e notifica os assinantes.
        /// </summary>
        private void OnInatividade(object? sender, EventArgs e)
        {
            ForçarExpiracaoSessao();
        }

        // ─── INotifyPropertyChanged ───────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Notifica a interface sobre mudança de propriedade.</summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
