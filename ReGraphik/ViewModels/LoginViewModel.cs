using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.Views;
using ReGraphik.Views.Pages;
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
        private readonly IAutorizarService _autorizarService = new AutorizarService();

        // Propriedades para armazenar o login e senha, que são vinculadas aos campos de entrada na interface
        private string _login = "";
        private string _senha = "";

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

        public ICommand EntrarCommand { get; }

        public LoginViewModel()
        {
            // Inicializa o comando de login, associando-o ao método Entrar
            EntrarCommand = new RelayCommand(Entrar);
        }

        private async Task Entrar(object parameter)
        {
            try
            {
                string senhaDigitada = "";

                // Obtém a senha digitada no campo de senha, que é passado como parâmetro do comando
                if (parameter is PasswordBox passwordBox)
                {
                    senhaDigitada = passwordBox.Password;
                }
                // Chama o serviço de autorização para tentar fazer o login com as credenciais fornecidas
                Usuario? usuario = await _autorizarService.LoginAsync(Login, senhaDigitada);

                if (usuario != null)
                {
                    // Se o login for bem-sucedido, abre a janela principal do aplicativo, passando o nome do usuário
                    new MainWindow(usuario.Nome ?? "Usuário").Show();

                    // Fecha a janela de login
                    foreach (Window w in Application.Current.Windows)
                    {
                        if (w is LoginWindow)
                        {
                            w.Close();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao conectar: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}