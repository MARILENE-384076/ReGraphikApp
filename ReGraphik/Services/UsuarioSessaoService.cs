using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReGraphik.Services
{
    /// <summary>
    /// Serviço singleton que mantém o estado do usuário logado durante a sessão,
    /// incluindo a foto de perfil compartilhada entre as views.
    /// </summary>
    public class UsuarioSessaoService : INotifyPropertyChanged
    {
        /// <summary>
        /// Instância única compartilhada por toda a aplicação
        /// </summary>
        public static readonly UsuarioSessaoService Instancia = new();

        private string? _fotoCaminho;
        public string? FotoCaminho
        {
            get => _fotoCaminho;
            set { _fotoCaminho = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}