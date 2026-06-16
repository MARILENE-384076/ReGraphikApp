using MahApps.Metro.IconPacks;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
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
        private bool _solicitacaoEnviada;

        /// <summary>
        /// Campo de mensagem de alerta
        /// </summary>
        private string _mensaNome;
        private string _mensaCpf;
        private string _mensaEmail;
        private string _mensaLogin;
        private string _mensaSenha;
        private string _mensagemErroGeral;

        /// <summary>
        /// Segurança no cadastro
        /// </summary>
        private string _tokenDigitado;
        private bool _isTokenValido;
        private bool _ocupadoToken;
        private string _mensagemErroToken;

        private bool _exibirToken;

        private bool _cadastroFinalizadoComSucesso;
        public bool ExibirSolicitacaoAcesso => !SolicitacaoEnviada;
        public bool ExibirFormularioCompleto => IsTokenValido && !CadastroFinalizadoComSucesso;

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
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExibirFormularioCompleto));
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
        /// Solicita um token de acesso.
        /// </summary>
        public bool SolicitacaoEnviada
        {
            get => _solicitacaoEnviada;
            set
            {
                _solicitacaoEnviada = value;
                OnPropertyChanged();
                // Dispara a atualização explícita de todas as regras de visibilidade
                OnPropertyChanged(nameof(ExibirSolicitacaoAcesso));
                OnPropertyChanged(nameof(ExibirFormularioCompleto));
            }
        }

        public bool CadastroFinalizadoComSucesso
        {
            get => _cadastroFinalizadoComSucesso;
            set
            {
                _cadastroFinalizadoComSucesso = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExibirFormularioCompleto));
            }
        }

        public bool ExibirToken
        {
            get => _exibirToken;
            set { _exibirToken = value; OnPropertyChanged(); }
        }

        public ICommand SolicitarAcessoCommand { get; }
        public ICommand ValidarTokenCommand { get; }
        public ICommand FinalizarCadastroCommand { get; }
        public ICommand RevelarSenhaCadastroCommand { get; }

        public CadastroViewModel(IAutorizarService? autorizarService = null)
        {
            _autorizarService = autorizarService ?? new AutorizarService();

            // Vinculação dos comandos aos métodos internos
            SolicitarAcessoCommand = new RelayCommand(async (_) => await SolicitarAcesso(), _ => !Ocupado);
            ValidarTokenCommand = new RelayCommand(async (_) => await ValidarToken(), _ => !OcupadoToken);
            FinalizarCadastroCommand = new RelayCommand(async (param) => await FinalizarCadastro(param), _ => !Ocupado);
            RevelarSenhaCadastroCommand = new RelayCommand((param) => AlternarVisibilidadeSenha(param));

            SolicitacaoEnviada = false;
            IsTokenValido = false;
            CadastroFinalizadoComSucesso = false;

            MensagemErroToken = string.Empty;
            TokenDigitado = string.Empty;
        }

        /// <summary>
        /// Método para cadastrar um novo usuário, que é chamado quando o comando de cadastro é acionado
        /// </summary>
        private async Task FinalizarCadastro(object parameter)
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

                /// Formata o CPF no padrão 000.000.000-00 antes de enviar para a API
                string cpfFormatado = ValidacaoCpfService.Formatar(CPF);

                /// Envia os dados definitivos e o token provisório para persistência na API
                bool cadastrado = await _autorizarService.FinalizarCadastroAsync(Nome, cpfFormatado, Email, Login, senhaCadastro, TokenDigitado.Trim());

                if (cadastrado)
                {
                    CadastroFinalizadoComSucesso = true;
                    MensagemErroGeral = "Sua conta corporativa foi criada e ativada com sucesso!";
                }
                else
                {
                    MensagemErroGeral = "Erro ao finalizar o cadastro na API. Verifique os dados informados.";
                }
            }
            catch (Exception)
            {
                MensagemErroGeral = "Erro ao conectar com a API.";
            }
            finally
            {
                Ocupado = false;
            }
        }

        /// <summary>
        /// Envia apenas o E-mail corporativo do usuário para a fila de aprovação da API.
        /// </summary>
        /// <returns></returns>
        private async Task SolicitarAcesso()
        {
            LimparMensagensErro();

            if (string.IsNullOrWhiteSpace(Email))
            {
                MensaEmail = "O e-mail é obrigatório.";
                return;
            }

            if (!Email.EndsWith("@regraphik.com.br", StringComparison.OrdinalIgnoreCase))
            {
                MensaEmail = "Somente e-mails corporativos da ReGraphik podem realizar cadastro.";
                return;
            }

            try
            {
                Ocupado = true;
                using (var client = new HttpClient())
                {
                    // Rota principal unificada do Passo 1 (Pública)
                    string url = "https://webregraphik.runasp.net/api/Usuario";

                    // Monta o objeto DTO contendo apenas o e-mail que a API espera
                    var dadosSolicitacao = new { email = Email.Trim() };
                    string jsonBody = JsonSerializer.Serialize(dadosSolicitacao);

                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);
                    string jsonResult = await response.Content.ReadAsStringAsync();

                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    if (response.IsSuccessStatusCode)
                    {
                        // Mapeia a mensagem vinda do servidor ("Solicitação processada com sucesso...")
                        var resultado = JsonSerializer.Deserialize<RespostaToken>(jsonResult, opcoes);

                        SolicitacaoEnviada = true;
                        MensagemErroGeral = resultado?.Mensagem ?? "Solicitação enviada! Aguarde a liberação do seu token.";

                        // Como estamos no Cenário B, o token NÃO vem na resposta do Usuário Comum.
                        // O campo de Debug da tela só será preenchido quando o administrador gerar o token no Swagger/Painel.
                        TokenDigitado = string.Empty;
                        MensagemErroToken = "Aguardando liberação do administrador...";
                        ExibirToken = true;
                    }
                    else
                    {
                        // Captura mensagens de validação vindas da API (Ex: "E-mail já está cadastrado")
                        try
                        {
                            var erroApi = JsonSerializer.Deserialize<RespostaToken>(jsonResult, opcoes);
                            MensagemErroGeral = erroApi?.Mensagem ?? "Erro ao processar requisição corporativa.";
                        }
                        catch
                        {
                            MensagemErroGeral = "E-mail já cadastrado ou domínio recusado.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
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