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
        private static UsuarioSessaoService? _instancia;
        public static UsuarioSessaoService Instancia => _instancia ??= new UsuarioSessaoService();

        private string? _fotoCaminho;

        public string? FotoCaminho
        {
            get => _fotoCaminho;
            set { _fotoCaminho = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private UsuarioSessaoService() { } // Construtor privado para garantir o Singleton
    }
}
