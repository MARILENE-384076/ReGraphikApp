using System.Windows.Input;
using ReGraphik.Services;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel para gerenciar informações do usuário, incluindo foto de perfil, nome, CPF, email e login.
    /// </summary>
    public class UsuarioViewModel : BaseViewModel
    {
        /// <summary>
        /// Caminho da foto do usuário, que é atualizado e notificado para a interface de usuário quando alterado.
        /// </summary>
        public string? ImgFoto
        {
            get => UsuarioSessaoService.Instancia.FotoCaminho;
            set
            {
                UsuarioSessaoService.Instancia.FotoCaminho = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Nome do usuário, que é atualizado e notificado para a interface de usuário quando alterado.
        /// </summary>
        private string _nome = string.Empty;
        public string Nome
        {
            get => _nome;
            set { _nome = value; OnPropertyChanged(); }
        }

        private string _cpf = string.Empty;
        public string Cpf
        {
            get => _cpf;
            set { _cpf = value; OnPropertyChanged(); }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comando que permite ao usuário selecionar uma foto de perfil, abrindo um diálogo de seleção de arquivo.
        /// </summary>
        public ICommand SelecionarFotoCommand { get; }

        /// <summary>
        /// Inicializa uma nova instância do ViewModel de usuário, configurando o comando de seleção de foto e carregando a foto salva do disco, se disponível.
        /// </summary>
        public UsuarioViewModel()
        {
            SelecionarFotoCommand = new RelayCommand(SelecionarFoto);

            /// Carrega foto salva do disco ao iniciar
            var fotoSalva = ConfiguracaoLocalService.CarregarFoto();
            if (fotoSalva != null)
                UsuarioSessaoService.Instancia.FotoCaminho = fotoSalva;

            UsuarioSessaoService.Instancia.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UsuarioSessaoService.FotoCaminho))
                    OnPropertyChanged(nameof(ImgFoto));
            };
        }

        /// <summary>
        /// Abre um diálogo para o usuário selecionar uma foto de perfil e salva o caminho da foto selecionada.
        /// </summary>
        private void SelecionarFoto()
        {
            /// Abre um diálogo para o usuário selecionar uma foto de perfil
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Selecione uma foto de perfil"
            };

            if (dialog.ShowDialog() == true)
            {
                ImgFoto = dialog.FileName;
                /// Salva no disco para persistir entre sessões
                ConfiguracaoLocalService.SalvarFoto(dialog.FileName);
            }
        }
    }
}