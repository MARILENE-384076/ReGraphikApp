using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.Views;
using System;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        // Instancia do serviço de autorização para lidar com a lógica de login
        private readonly IAutorizarService _autorizarService;

        // Propriedades para armazenar o login e senha, que são vinculadas aos campos de entrada na interface
        private string _login = "";
        private bool _ocupado;

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        // Propriedade para indicar se o processo de login está em andamento, usada para desabilitar o botão de login enquanto a operação está sendo processada
        public bool Ocupado
        {
            get => _ocupado;
            set { _ocupado = value; OnPropertyChanged(); }
        }

        public ICommand EntrarCommand { get; }

        public LoginViewModel()
        {
            _autorizarService = new AutorizarService();
            // Inicializa o comando de login, associando-o ao método Entrar
            EntrarCommand = new RelayCommand(async (param) => await Entrar(param), CanEntrar);
        }

        // Método para verificar se o comando de login pode ser executado, desabilitando-o quando o processo de login estiver em andamento
        private bool CanEntrar(object parameter) => !Ocupado;

        private async Task Entrar(object parameter)
        {
            // Verifica se o parâmetro é do tipo PasswordBox, que é necessário para obter a senha digitada pelo usuário
            if (parameter is not PasswordBox passwordBox)
            {
                MessageBox.Show("Erro ao digitar a senha.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // Obtém a senha digitada pelo usuário a partir do PasswordBox
            string senhaDigitada = passwordBox.Password;

            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(senhaDigitada))
            {
                MessageBox.Show("Por favor, preencha todos os campos.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                /// Indicar que o processo de login está em andamento, o que pode ser usado para desabilitar o botão de login na interface
                Ocupado = true;

                /// Chama o serviço de autorização para tentar fazer o login com as credenciais fornecidas
                Usuario? usuario = await _autorizarService.LoginAsync(Login, senhaDigitada);

                if (usuario == null)
                {
                    MessageBox.Show("Usuário ou senha incorretos.", "Erro de Autenticação", MessageBoxButton.OK, MessageBoxImage.Error);
                    Ocupado = false; /// Importante liberar a tela caso dê erro
                    return; /// Para a execução aqui e não deixa abrir a MainWindow
                }

                if (usuario != null)
                {
                    // 1. Cria e mostra a tela principal com o usuário logado
                    var main = new MainWindow(usuario);
                    main.Show();

                    // 2. Procura especificamente a LoginWindow que está aberta no sistema e fecha ela
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is LoginWindow)
                        {
                            window.Close();
                            break; // Para o laço assim que fechar
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Ocupado = false;
                MessageBox.Show($"Erro ao conectar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}