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

        private string _nome;
        private string _cpf;
        private string _email;
        private string _login;
        private bool _ocupado;
        private string _mensaNome;
        private string _mensaCpf;
        private string _mensaEmail;
        private string _mensaLogin;
        private string _mensaSenha;
        private string _mensagemErroGeral;

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

        // Mensagens de Alertas
        public string MensaNome
        {
            get => _mensaNome;
            set { _mensaNome = value; OnPropertyChanged(); }
        }

        public string MensaCpf
        {
            get => _mensaCpf;
            set { _mensaCpf = value; OnPropertyChanged(); }
        }

        public string MensaEmail
        {
            get => _mensaEmail;
            set { _mensaEmail = value; OnPropertyChanged(); }
        }


        public string MensaLogin
        {
            get => _mensaLogin;
            set { _mensaLogin = value; OnPropertyChanged(); }
        }

        public string MensaSenha
        {
            get => _mensaSenha;
            set { _mensaSenha = value; OnPropertyChanged(); }
        }

        public string MensagemErroGeral
        {
            get => _mensagemErroGeral;
            set { _mensagemErroGeral = value; OnPropertyChanged(); }
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
                MensaSenha = "Erro interno ao processar o campo de senha.";
                return;
            }
            // Obtém a senha digitada no PasswordBox
            string senhaDigitada = passwordBox.Password;
            bool possuiErro = false;

            // Validação simples de campos vazios
            if (string.IsNullOrWhiteSpace(CPF))
            {
                MensaCpf = "O CPF é obrigatório!";
                possuiErro = true;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                MensaEmail = "O Email é obrigatório!";
                possuiErro = true;
            }

            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(CPF) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Login) ||
                string.IsNullOrWhiteSpace(senhaDigitada))
            {
                MensagemErroGeral = "Preencha todos os campos. Tente novamente.";
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