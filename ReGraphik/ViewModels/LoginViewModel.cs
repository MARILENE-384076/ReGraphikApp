using MahApps.Metro.IconPacks;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.Views;
using System;
using System.Data.Common;
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
    /// <summary>
    /// ViewModel responsável por gerenciar a lógica de login da aplicação. Ele interage com a interface de usuário (LoginWindow) 
    /// para obter as credenciais do usuário, valida-las e chamar o serviço de autorização para autenticar o usuário.
    /// </summary>
    public class LoginViewModel : BaseViewModel
    {
        /// <summary>
        /// Referência para o serviço de autorização, que é responsável por realizar a autenticação do usuário.
        /// </summary>
        private readonly IAutorizarService _autorizarService;

        /// <summary>
        /// Propriedades para armazenar o login, senha e mensagens de erro que serão exibidas na interface de usuário.
        /// </summary>
        private string _login;
        private bool _ocupado;
        private string _mensaLogin;
        private string _mensaSenha;
        private string _mensagemErroGeral;


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

        public ICommand EntrarCommand { get; }
        public ICommand RevelarSenhaCommand { get; }

        public ICommand EsqueciSenhaCommand { get; }

        public LoginViewModel()
        {
            _autorizarService = new AutorizarService();
            // Inicializa o comando de login, associando-o ao método Entrar
            EntrarCommand = new RelayCommand(async (param) => await Entrar(param), CanEntrar);

            RevelarSenhaCommand = new RelayCommand(RevelarSenha);

            EsqueciSenhaCommand = new RelayCommand(EsqueciSenha);
        }

        // Método para verificar se o comando de login pode ser executado, desabilitando-o quando o processo de login estiver em andamento
        private bool CanEntrar(object parameter) => !Ocupado;

        private async Task Entrar(object parameter)
        {
            MensaLogin = string.Empty;
            MensaSenha = string.Empty;
            MensagemErroGeral = string.Empty;

            // Verifica se o parâmetro é do tipo PasswordBox, que é necessário para obter a senha digitada pelo usuário
            if (parameter is not PasswordBox passwordBox)
            {
                MensaSenha = "Erro interno ao processar o campo de senha.";
                return;
            }
            // Obtém a senha digitada pelo usuário a partir do PasswordBox
            string senhaDigitada = passwordBox.Password;
            bool possuiErro = false;

            // Validação de login
            if (string.IsNullOrWhiteSpace(Login))
            {
                MensaLogin = "O login é obrigatório!";
                possuiErro = true;
            }

            // Validação de senha
            if (string.IsNullOrWhiteSpace(senhaDigitada))
            {
                MensaSenha = "A senha é obrigatória!";
                possuiErro = true;
            }


            if (possuiErro) return;

            try
            {
                /// Indicar que o processo de login está em andamento, o que pode ser usado para desabilitar o botão de login na interface
                Ocupado = true;

                /// Chama o serviço de autorização para tentar fazer o login com as credenciais fornecidas
                Usuario? usuario = await _autorizarService.LoginAsync(Login, senhaDigitada);

                if (usuario == null)
                {
                    MensagemErroGeral = "Usuário ou senha incorretos. Tente novamente.";
                    return;
                }


                var main = new MainWindow(usuario);
                main.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is LoginWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MensagemErroGeral = "Não foi possível conectar ao servidor. Verifique sua internet.";
            }
            finally { Ocupado = false; }

        }
        private async Task RevelarSenha(object parameter)
        {

            if (parameter is not Grid gridContainer)
            {
                MensaSenha = "Erro interno ao processar o campo de senha.";
                return;
            }

            // Buscando os componentes de dentro do Grid através do nome ou tipo
            var txtSenhaLogin = gridContainer.Children.OfType<PasswordBox>().FirstOrDefault(x => x.Name == "TxtSenhaLogin");
            var txtSenhaVisivelLogin = gridContainer.Children.OfType<TextBox>().FirstOrDefault(x => x.Name == "TxtSenhaVisivelLogin");
            var btnRevelar = gridContainer.Children.OfType<Button>().FirstOrDefault(x => x.Name == "BtnRevelarSenha");

            if (txtSenhaLogin == null || txtSenhaVisivelLogin == null || btnRevelar == null) return;

            var iconeOlho = btnRevelar.Content as MahApps.Metro.IconPacks.PackIconMaterial;
            if (iconeOlho == null) return;

            if (txtSenhaLogin.Visibility == Visibility.Visible)
            {
                txtSenhaVisivelLogin.Text = txtSenhaLogin.Password;
                txtSenhaLogin.Visibility = Visibility.Collapsed;
                txtSenhaVisivelLogin.Visibility = Visibility.Visible;

                iconeOlho.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.EyeOff;
            }
            else
            {
                
                txtSenhaLogin.Password = txtSenhaVisivelLogin.Text;
                txtSenhaVisivelLogin.Visibility = Visibility.Collapsed;
                txtSenhaLogin.Visibility = Visibility.Visible;

                iconeOlho.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Eye;
            }
        }

        private async Task EsqueciSenha()
        {
            RecuperarSenhaWindow recuperacaoTela = new RecuperarSenhaWindow();
            recuperacaoTela.Owner = Application.Current.MainWindow; 
            recuperacaoTela.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            recuperacaoTela.ShowDialog();
        }

    }
}