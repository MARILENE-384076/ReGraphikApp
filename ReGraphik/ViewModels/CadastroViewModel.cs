using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    public class CadastroViewModel : BaseViewModel
    {
        // Instancia do serviço de autorização para lidar com a lógica de cadastro
        private readonly IAutorizarService _autorizarService = new AutorizarService();

        private string _nome = "";
        private string _cpf = "";
        private string _email = "";
        private string _login = "";

        public string Nome
        {
            get => _nome;
            set { _nome = value; OnPropertyChanged(); }
        }

        public string CPF
        {
            get => _cpf;
            set { _cpf = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public ICommand CadastrarCommand { get; }

        public CadastroViewModel()
        {
            // Inicializa o comando de cadastro, associando-o ao método Cadastrar
            CadastrarCommand = new RelayCommand(Cadastrar);
        }

        // Método para cadastrar um novo usuário, que é chamado quando o comando de cadastro é acionado
        private async Task Cadastrar(object parameter)
        {
            try
            {
                string senhaDigitada = "";

                if (parameter is PasswordBox passwordBox)
                {
                    senhaDigitada = passwordBox.Password;
                }

                var usuario = new
                {
                    id = Guid.NewGuid().ToString(),
                    name = Nome,
                    cpf = CPF,
                    email = Email,
                    login = Login,
                    senha = senhaDigitada,
                    perfil = "Administrador",
                    data_cadastro = DateTime.Now
                };

                // Chama o serviço de autorização para cadastrar o usuário
                bool response = await _autorizarService.CadastrarAsync(usuario);

                if (response)
                {
                    MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimparCampos();
                }
                else
                {
                    MessageBox.Show("Erro ao cadastrar usuário.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cadastrar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LimparCampos()
        {
            Nome = CPF = Email = Login = "";
        }
    }
}