using ReGraphik.Models;
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
        private readonly IAutorizarService _autorizarService;

        private string _nome = "";
        private string _cpf = "";
        private string _email = "";
        private string _login = "";
        private bool _ocupado;

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

        public bool Ocupado
        {
            get => _ocupado;
            set { _ocupado = value; OnPropertyChanged(); }
        }

        public ICommand CadastrarCommand { get; }

        public CadastroViewModel()
        {
            _autorizarService = new AutorizarService();
            // Inicializa o comando de cadastro, associando-o ao método Cadastrar
            CadastrarCommand = new RelayCommand(async (param) => await Cadastrar(param), CanCadastrar);
        }
        private bool CanCadastrar(object parameter) => !Ocupado;

        // Método para cadastrar um novo usuário, que é chamado quando o comando de cadastro é acionado
        private async Task Cadastrar(object parameter)
        {
            // O parâmetro é esperado ser um PasswordBox para obter a senha digitada pelo usuário
            if (parameter is not PasswordBox passwordBox)
            {
                MessageBox.Show("Erro técnico ao processar o campo de senha.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Obtém a senha digitada no PasswordBox
            string senhaDigitada = passwordBox.Password;

            // Validação simples de campos vazios
            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(CPF) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Login) ||
                string.IsNullOrWhiteSpace(senhaDigitada))
            {
                MessageBox.Show("Por favor, preencha todos os campos obrigatórios.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Ocupado = true; // Indica que o processo de cadastro está em andamento

                // Cria um objeto anônimo representando o usuário a ser cadastrado, com as informações fornecidas e um ID gerado aleatoriamente
                var novoUsuario = new Usuario
                {
                    Id = Guid.NewGuid().ToString(),
                    Nome = Nome,
                    CPF = CPF, 
                    Email = Email,
                    Login = Login,
                    Senha = senhaDigitada,
                    Perfil = "Administrador",
                    DataCadastro = DateTime.Now
                };

                // Chama o serviço de autorização para cadastrar o usuário
                bool response = await _autorizarService.CadastrarAsync(novoUsuario);

                if (response)
                {
                    MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    LimparCampos();
                    
                }
                else
                {
                    MessageBox.Show("Erro ao cadastrar usuário.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                passwordBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao cadastrar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LimparCampos()
        {
            Nome = "";
            CPF = "";
            Email = "";
            Login = "";
        }
    }
}