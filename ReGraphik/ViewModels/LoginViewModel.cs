using MahApps.Metro.IconPacks;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.Views;
using System;
using System.ComponentModel;
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
    internal class LoginViewModel : BaseViewModel
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
        private bool _formularioEnviado;
        private bool _isTokenValido;
        private string _tokenDigitado;


        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Propriedade para indicar se o processo de login está em andamento, usada para desabilitar o botão de login enquanto a operação está sendo processada
        /// </summary>
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

        public bool FormularioEnviado
        {
            get => _formularioEnviado;
            set { _formularioEnviado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Controla a transição da tela do Token para a animação de Sucesso (✓)
        /// </summary>
        public bool IsTokenValido
        {
            get => _isTokenValido;
            set { _isTokenValido = value; OnPropertyChanged(); }
        }
        public string TokenDigitado
        {
            get => _tokenDigitado;
            set { _tokenDigitado = value; OnPropertyChanged(); }
        }

        public ICommand EntrarCommand { get; }
        public ICommand EsqueciSenhaCommand { get; }
        public ICommand RevelarSenhaCommand { get; }

        public LoginViewModel(IAutorizarService? autorizarService = null)
        {
            _autorizarService = autorizarService ?? new AutorizarService();

            /// Inicializa apenas os comandos pertencentes ao fluxo de Login
            EntrarCommand = new RelayCommand(async (param) => await Entrar(param), CanEntrar);
            RevelarSenhaCommand = new RelayCommand((param) => AlternarVisibilidadeSenha(param));
            EsqueciSenhaCommand = new RelayCommand((param) => AbrirEsqueciSenhaWindow());
        }


        /// <summary>
        /// Método para verificar se o comando de login pode ser executado, 
        /// desabilitando-o quando o processo de login estiver em andamento
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private bool CanEntrar(object parameter) => !Ocupado;

        private async Task Entrar(object parameter)
        {
            LimparMensagensErro();

            if (parameter is not PasswordBox passwordBox)
            {
                MensaSenha = "Erro interno ao processar o campo de senha.";
                return;
            }
            string senhaDigitada = passwordBox.Password;

            /// Captura a senha de forma inteligente se vier o Grid completo
            if (parameter is Grid gridCampos)
            {
                var pb = gridCampos.FindName("TxtSenhaLogin") as PasswordBox;
                var tb = gridCampos.FindName("TxtSenhaVisivel") as TextBox;

                if (pb != null && tb != null)
                {
                    /// Se o PasswordBox estiver visível, pega dele. Se não (olho aberto), pega do TextBox
                    senhaDigitada = pb.IsVisible ? pb.Password : tb.Text;
                }
            }
            /// Mantém a compatibilidade caso venha o PasswordBox direto
            else if (parameter is PasswordBox pbDireto)
            {
                senhaDigitada = pbDireto.Password;
            }
            else
            {
                MensaSenha = "Erro interno ao processar o campo de senha.";
                return;
            }
            bool possuiErro = false;

            if (string.IsNullOrWhiteSpace(Login))
            {
                MensaLogin = "O login é obrigatório!";
                possuiErro = true;
            }

            if (string.IsNullOrWhiteSpace(senhaDigitada))
            {
                MensaSenha = "A senha é obrigatória!";
                possuiErro = true;
            }

            if (possuiErro) return;

            try
            {
                Ocupado = true;

                /// Realiza a autenticação direta pelo serviço
                Usuario? usuario = await _autorizarService.LoginAsync(Login, senhaDigitada);

                if (usuario == null)
                {
                    MensagemErroGeral = "Usuário ou senha incorretos. Tente novamente.";
                    return;
                }

                /// Armazena o usuário logado e dispara o timer de inatividade
                UsuarioSessaoService.Instancia.IniciarSessao(usuario);
                UsuarioSessaoService.Instancia.FotoCaminho = usuario.FotoPerfil;

                /// Abre a tela principal caso o login seja bem-sucedido
                var main = new MainWindow(usuario);
                main.Show();

                FecharJanelaLogin();
            }
            catch (Exception)
            {
                MensagemErroGeral = "Não foi possível conectar ao servidor. Verifique sua internet.";
            }
            finally
            {
                Ocupado = false;
            }
        }

        private void FecharJanelaLogin()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is LoginWindow)
                {
                    window.Close();
                    break;
                }
            }
        }
        private void LimparMensagensErro()
        {
            MensaLogin = string.Empty;
            MensaSenha = string.Empty;
            MensagemErroGeral = string.Empty;
        }

        private void AlternarVisibilidadeSenha(object parameter)
        {
            if (parameter is not Grid gridCampos) return;

            PasswordBox? passwordBox = null;
            TextBox? textBoxVisivel = null;
            PackIconMaterial? iconeOlho = null;

            foreach (var child in gridCampos.Children)
            {
                if (child is PasswordBox pb)
                    passwordBox = pb;
                else if (child is TextBox tb)
                    textBoxVisivel = tb;
                else if (child is Button btn && btn.Content is PackIconMaterial icon)
                    iconeOlho = icon;
            }

            if (passwordBox == null || textBoxVisivel == null) return;

            /// Executa a inversão de visibilidade e cópia de valores
            if (passwordBox.Visibility == Visibility.Visible)
            {
                /// Revelar Senha
                textBoxVisivel.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                textBoxVisivel.Visibility = Visibility.Visible;

                if (iconeOlho != null)
                    iconeOlho.Kind = PackIconMaterialKind.EyeOff;
            }
            else
            {
                /// Ocultar Senha
                passwordBox.Password = textBoxVisivel.Text;
                textBoxVisivel.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;

                if (iconeOlho != null)
                    iconeOlho.Kind = PackIconMaterialKind.Eye;
            }
        }

        /// <summary>
        /// Método para abrir o botão de esqueci a senha 
        /// </summary>
        private void AbrirEsqueciSenhaWindow()
        {
            var esqueciSenhaWindow = new EsqueciSenhaWindow();
            esqueciSenhaWindow.ShowDialog();
        }
    }
}
