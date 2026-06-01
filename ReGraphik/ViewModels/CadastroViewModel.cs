using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System.Windows;
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
        private string _senha = "";

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

        public string Senha
        {
            get => _senha;
            set { _senha = value; OnPropertyChanged(); }
        }

        public ICommand CadastrarCommand { get; }

        public CadastroViewModel()
        {
            CadastrarCommand = new RelayCommand(async () => await Cadastrar());
        }

        // Método para cadastrar um novo usuário, que é chamado quando o comando de cadastro é acionado
        private async Task Cadastrar()
        {
            try
            {
                var usuario = new
                {
                    id = Guid.NewGuid().ToString(),
                    name = Nome,
                    cpf = CPF,
                    email = Email,
                    login = Login,
                    senha = Senha,
                    perfil = "",
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
            Nome = CPF = Email = Login = Senha = "";
        }
    }
}