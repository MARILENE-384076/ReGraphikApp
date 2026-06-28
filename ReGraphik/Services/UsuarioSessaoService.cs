using ReGraphik.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace ReGraphik.Services
{
    /// <summary>
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
        /// Ajuste conforme necessidade do projeto.
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
            set { _usuarioLogado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// ID do usuário logado. Atalho para <see cref="UsuarioLogado"/>?.Id.
        /// Usado pelos ViewModels para associar registros ao usuário da sessão.
        /// </summary>
        public string? IdUsuarioLogado => _usuarioLogado?.Id;

        // ─── Evento de logout por inatividade ────────────────────────────────────
        /// <summary>
        /// Disparado quando o sistema detecta inatividade superior a <see cref="MinutosInatividade"/> minutos.
        /// Assine este evento no MainViewModel para executar o fluxo de saída.
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
            UsuarioLogado = usuario;
            ResetarTimer();
        }

        /// <summary>
        /// Reinicia o contador de inatividade. Deve ser chamado a cada interação relevante do usuário
        /// (clique de menu, abertura de tela, etc.) para evitar logout acidental.
        /// </summary>
        public void ResetarTimer()
        {
            _timerInatividade.Stop();
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

        // ─── Callback interno ─────────────────────────────────────────────────────

        /// <summary>
        /// Chamado pelo timer quando o período de inatividade expira.
        /// Encerra a sessão e notifica os assinantes.
        /// </summary>
        private void OnInatividade(object? sender, EventArgs e)
        {
            EncerrarSessao();
            SessaoExpirada?.Invoke();
        }

        // ─── INotifyPropertyChanged ───────────────────────────────────────────────
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Notifica a interface sobre mudança de propriedade.</summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
