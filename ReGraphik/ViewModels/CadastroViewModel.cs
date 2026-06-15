using MahApps.Metro.IconPacks;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    public class CadastroViewModel : BaseViewModel
    {
        /// <summary>
        /// Instancia do serviço de autorização para lidar com a lógica de cadastro
        /// </summary>
        private readonly IAutorizarService _autorizarService;

        private string? _nome;
        private string? _cpf;
        private string? _email;
        private string? _login;
        private bool _ocupado;
        private bool _formularioEnviado;

        /// <summary>
        /// Campo de mensagem de alerta
        /// </summary>
        private string _mensaNome;
        private string _mensaCpf;
        private string _mensaEmail;
        private string _mensaLogin;
        private string _mensaSenha;
        private string _mensagemErroGeral;

        private bool _exibirAlertaToken;
        private string _tokenGeradoAlerta;

        public bool ExibirAlertaToken
        {
            get => _exibirAlertaToken;
            set { _exibirAlertaToken = value; OnPropertyChanged(); }
        }

        public string TokenGeradoAlerta
        {
            get => _tokenGeradoAlerta;
            set { _tokenGeradoAlerta = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Segurança no cadastro
        /// </summary>
        private string _tokenDigitado;
        private bool _isTokenValido;
        private bool _ocupadoToken;
        private string _mensagemErroToken;
        public bool ExibirFormulario => !FormularioEnviado;
        public bool ExibirPainelToken => FormularioEnviado && !IsTokenValido;

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

        /// <summary>
        /// Mensagens de Alertas
        /// </summary>
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

        /// <summary>
        /// Define se o token digitado foi validado com sucesso pela API.
        /// </summary>
        public bool IsTokenValido
        {
            get => _isTokenValido;
            set
            {
                _isTokenValido = value;
                OnPropertyChanged(nameof(IsTokenValido));
                // Notifica o WPF para ocultar o painel do token e exibir o sucesso final
                OnPropertyChanged(nameof(ExibirPainelToken));
            }
        }

        /// <summary>
        /// Token de segurança
        /// </summary>
        public string TokenDigitado
        {
            get => _tokenDigitado;
            set { _tokenDigitado = value; OnPropertyChanged(nameof(TokenDigitado)); }
        }

        /// <summary>
        /// Controla se o formulário deve ser liberado ou não
        /// </summary>
        public bool OcupadoToken
        {
            get => _ocupadoToken;
            set { _ocupadoToken = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Exibe erro caso o token seja inválido
        /// </summary>
        public string MensagemErroToken
        {
            get => _mensagemErroToken;
            set { _mensagemErroToken = value; OnPropertyChanged(nameof(MensagemErroToken)); }
        }

        /// <summary>
        /// Define se a API concluiu o pré-cadastro com sucesso e disparou o token.
        /// </summary>
        public bool FormularioEnviado
        {
            get => _formularioEnviado;
            set
            {
                _formularioEnviado = value;
                OnPropertyChanged(nameof(FormularioEnviado));
                // Notifica o WPF para reavaliar o que deve ser exibido
                OnPropertyChanged(nameof(ExibirFormulario));
                OnPropertyChanged(nameof(ExibirPainelToken));
            }
        }

        public ICommand CadastrarCommand { get; }
        public ICommand ValidarTokenCommand { get; }
        public ICommand RevelarSenhaCadastroCommand { get; }
        public ICommand FecharAlertaTokenCommand { get; }

        public CadastroViewModel(IAutorizarService? autorizarService = null)
        {
            _autorizarService = autorizarService ?? new AutorizarService();

            // Vinculação dos comandos aos métodos internos
            CadastrarCommand = new RelayCommand(async (param) => await Cadastrar(param), CanCadastrar);
            ValidarTokenCommand = new RelayCommand(async (param) => await ValidarToken(), CanValidarToken);
            RevelarSenhaCadastroCommand = new RelayCommand((param) => AlternarVisibilidadeSenha(param));

            FormularioEnviado = false;
            IsTokenValido = false;

            FecharAlertaTokenCommand = new RelayCommand(_ => ExibirAlertaToken = false);
        }

        private bool CanCadastrar(object parameter) => !Ocupado;
        private bool CanValidarToken(object parameter) => !OcupadoToken;

        /// <summary>
        /// Método para cadastrar um novo usuário, que é chamado quando o comando de cadastro é acionado
        /// </summary>
        private async Task Cadastrar(object parameter)
        {
            LimparMensagensErro();
            bool possuiErro = false;

            string senhaCadastro = string.Empty;
            if (parameter is Grid gridCampos)
            {
                PasswordBox? passwordBox = null;
                TextBox? textBoxVisivel = null;

                foreach (var child in gridCampos.Children)
                {
                    if (child is PasswordBox pb) passwordBox = pb;
                    else if (child is TextBox tb) textBoxVisivel = tb;
                }

                if (passwordBox != null && textBoxVisivel != null)
                {
                    // Se o PasswordBox estiver visível, pega dele. Se não, pega do TextBox de texto aberto.
                    senhaCadastro = passwordBox.Visibility == Visibility.Visible
                        ? passwordBox.Password
                        : textBoxVisivel.Text;
                }
            }

            // Validações de campos obrigatórios
            if (string.IsNullOrWhiteSpace(Nome)) { MensaNome = "O nome completo é obrigatório."; possuiErro = true; }

            // Validação de CPF: obrigatório e deve ser válido pelo algoritmo dos dígitos verificadores
            if (string.IsNullOrWhiteSpace(CPF))
            {
                MensaCpf = "O CPF é obrigatório.";
                possuiErro = true;
            }
            else if (!ValidacaoCpfService.Validar(CPF))
            {
                MensaCpf = "CPF inválido. Verifique os dígitos informados.";
                possuiErro = true;
            }

            if (string.IsNullOrWhiteSpace(Email)) { MensaEmail = "O e-mail é obrigatório."; possuiErro = true; }
            if (string.IsNullOrWhiteSpace(Login)) { MensaLogin = "O campo login é obrigatório."; possuiErro = true; }
            if (string.IsNullOrWhiteSpace(senhaCadastro)) { MensaSenha = "A senha é obrigatória."; possuiErro = true; }

            if (possuiErro) return;

            try
            {
                Ocupado = true;

                // Formata o CPF no padrão 000.000.000-00 antes de enviar para a API
                string cpfFormatado = ValidacaoCpfService.Formatar(CPF);

                // Envia os dados para o endpoint unificado "Post" da sua API
                var resultado = await _autorizarService.CadastrarAsync(Nome, cpfFormatado, Email, Login, senhaCadastro);

                if (resultado != null)
                {
                    TokenGeradoAlerta = resultado.Token;
                    ExibirAlertaToken = true;
                    // Libera o container de Token na tela do WPF
                    FormularioEnviado = true;
                }
                else
                {
                    MensagemErroGeral = "Não foi possível gerar o token de cadastro.";
                }
            }
            catch (Exception ex)
            {
                // Captura mensagens amigáveis lançadas pelo HttpClient do Service
                MensagemErroGeral = ex.Message;
            }
            finally
            {
                Ocupado = false;
            }
        }

        /// <summary>
        /// Envia o token digitado pelo usuário para a API validar de forma assíncrona.
        /// </summary>
        private async Task ValidarToken()
        {
            MensagemErroToken = string.Empty;

            if (string.IsNullOrWhiteSpace(TokenDigitado))
            {
                MensagemErroToken = "Por favor, insira o token recebido.";
                return;
            }

            try
            {
                OcupadoToken = true;
                bool sucesso = await _autorizarService.ValidarTokenAsync(Email, TokenDigitado.Trim());

                if (sucesso)
                {
                    // Ativa o ícone de Check com animação de sucesso no seu XAML
                    IsTokenValido = true;
                    LimparMensagensErro();
                }
                else
                {
                    MensagemErroToken = "Token incorreto ou tempo limite expirado.";
                }
            }
            catch (Exception)
            {
                MensagemErroToken = "Falha na comunicação com o servidor de validação.";
            }
            finally
            {
                OcupadoToken = false;
            }
        }

        private void AlternarVisibilidadeSenha(object parameter)
        {
            // O parâmetro enviado pelo CommandParameter no XAML deve ser o Grid que envolve o PasswordBox e o TextBox
            if (parameter is not Grid gridCampos) return;

            PasswordBox passwordBox = null;
            TextBox textBoxVisivel = null;
            PackIconMaterial iconeOlho = null;

            foreach (var child in gridCampos.Children)
            {
                if (child is PasswordBox pb) passwordBox = pb;
                else if (child is TextBox tb) textBoxVisivel = tb;
                else if (child is Button btn)
                {
                    if (btn.Content is PackIconMaterial icon)
                        iconeOlho = icon;
                    else if (btn.Template.FindName("IconeOlho", btn) is PackIconMaterial iconTemplate)
                        iconeOlho = iconTemplate;
                }
            }

            if (passwordBox == null || textBoxVisivel == null) return;

            if (passwordBox.Visibility == Visibility.Visible)
            {
                textBoxVisivel.Text = passwordBox.Password;
                passwordBox.Visibility = Visibility.Collapsed;
                textBoxVisivel.Visibility = Visibility.Visible;

                if (iconeOlho != null) iconeOlho.Kind = PackIconMaterialKind.EyeOff;
            }
            else
            {
                passwordBox.Password = textBoxVisivel.Text;
                textBoxVisivel.Visibility = Visibility.Collapsed;
                passwordBox.Visibility = Visibility.Visible;

                if (iconeOlho != null) iconeOlho.Kind = PackIconMaterialKind.Eye;
            }
        }

        private void LimparMensagensErro()
        {
            MensaNome = string.Empty;
            MensaCpf = string.Empty;
            MensaEmail = string.Empty;
            MensaLogin = string.Empty;
            MensaSenha = string.Empty;
            MensagemErroGeral = string.Empty;
        }
    }
}