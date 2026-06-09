using System.Windows.Input;
using ReGraphik.Services;

namespace ReGraphik.ViewModels
{
    public class UsuarioViewModel : BaseViewModel
    {
        public string? ImgFoto
        {
            get => UsuarioSessaoService.Instancia.FotoCaminho;
            set
            {
                UsuarioSessaoService.Instancia.FotoCaminho = value;
                OnPropertyChanged();
            }
        }

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

        public ICommand SelecionarFotoCommand { get; }

        public UsuarioViewModel()
        {
            SelecionarFotoCommand = new RelayCommand(SelecionarFoto);

            // Carrega foto salva do disco ao iniciar
            var fotoSalva = ConfiguracaoLocalService.CarregarFoto();
            if (fotoSalva != null)
                UsuarioSessaoService.Instancia.FotoCaminho = fotoSalva;

            UsuarioSessaoService.Instancia.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UsuarioSessaoService.FotoCaminho))
                    OnPropertyChanged(nameof(ImgFoto));
            };
        }

        private void SelecionarFoto()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Selecione uma foto de perfil"
            };

            if (dialog.ShowDialog() == true)
            {
                ImgFoto = dialog.FileName;
                // Salva no disco para persistir entre sessões
                ConfiguracaoLocalService.SalvarFoto(dialog.FileName);
            }
        }
    }
}