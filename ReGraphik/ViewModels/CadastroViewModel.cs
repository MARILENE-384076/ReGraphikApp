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
        private readonly IAutorizarService _autorizarService;

        /// <summary>
        /// Serviço de convites direto no Firebase 
        /// </summary>
        private readonly ConviteService _conviteService;

        /// <summary>
        /// Campos 
        /// </summary>
        private string? _nome;
        private string? _cpf;
        private string? _email;
        private string? _login;
        private bool _ocupado;
        private bool _solicitacaoEnviada;
        private bool _isTokenValido;
        private bool _ocupadoToken;
        private string _tokenDigitado = string.Empty;
        private string _mensaNome = string.Empty;
        private string _mensaCpf = string.Empty;
        private string _mensaEmail = string.Empty;
        private string _mensaLogin = string.Empty;
        private string _mensaSenha = string.Empty;
        private string _mensagemErroGeral = string.Empty;
        private string _mensagemErroToken = string.Empty;
        private bool _cadastroFinalizadoComSucesso;

        /// <summary>
        /// Visibilidade das etapas + botão "Solicitar Acesso"
        /// </summary> 
        public bool ExibirSolicitacaoAcesso => !SolicitacaoEnviada;

        /// <summary>
        /// Formulário completo de dados do usuário.
        /// </summary>
        public bool ExibirFormularioCompleto => IsTokenValido && !CadastroFinalizadoComSucesso;

        public string? Nome { get => _nome; set { _nome = value; OnPropertyChanged(); } }
        public string? CPF { 
            get => _cpf;   
            set {
                if (_cpf == value) return;

                /// Remove tudo o que não for número
                string apenasNumeros = value != null
                    ? System.Text.RegularExpressions.Regex.Replace(value, @"[^\d]", "")
                    : string.Empty;

                /// Limita a 11 dígitos para não quebrar a formatação
                if (apenasNumeros.Length > 11)
                    apenasNumeros = apenasNumeros.Substring(0, 11);

                /// Aplica a máscara dinamicamente baseado na quantidade de números
                string cpfFormatado = apenasNumeros;

                if (apenasNumeros.Length > 3 && apenasNumeros.Length <= 6)
                    cpfFormatado = $"{apenasNumeros.Substring(0, 3)}.{apenasNumeros.Substring(3)}";
                else if (apenasNumeros.Length > 6 && apenasNumeros.Length <= 9)
                    cpfFormatado = $"{apenasNumeros.Substring(0, 3)}.{apenasNumeros.Substring(3, 3)}.{apenasNumeros.Substring(6)}";
                else if (apenasNumeros.Length > 9)
                    cpfFormatado = $"{apenasNumeros.Substring(0, 3)}.{apenasNumeros.Substring(3, 3)}.{apenasNumeros.Substring(6, 3)}-{apenasNumeros.Substring(9)}";

                _cpf = cpfFormatado;
                OnPropertyChanged();
            } 
        }
        public string? Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string? Login { get => _login; set { _login = value; OnPropertyChanged(); } }

        public bool Ocupado
        {
            get => _ocupado;
            set { _ocupado = value; OnPropertyChanged(); }
        }

        public bool OcupadoToken
        {
            get => _ocupadoToken;
            set { _ocupadoToken = value; OnPropertyChanged(); }
        }

        public bool SolicitacaoEnviada
        {
            get => _solicitacaoEnviada;
            set
            {
                _solicitacaoEnviada = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ExibirSolicitacaoAcesso));
                OnPropertyChanged(nameof(ExibirFormularioCompleto));
            }
        }

        public bool IsTokenValido
        {
            get => _isTokenValido;
            set
            {
                _isTokenValido = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SolicitacaoEnviada));
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

        public string TokenDigitado
        {
            get => _tokenDigitado;
            set { _tokenDigitado = value; OnPropertyChanged(); }
        }

        public string MensaNome { get => _mensaNome; set { _mensaNome = value; OnPropertyChanged(); } }
        public string MensaCpf { get => _mensaCpf; set { _mensaCpf = value; OnPropertyChanged(); } }
        public string MensaEmail { get => _mensaEmail; set { _mensaEmail = value; OnPropertyChanged(); } }
        public string MensaLogin { get => _mensaLogin; set { _mensaLogin = value; OnPropertyChanged(); } }
        public string MensaSenha { get => _mensaSenha; set { _mensaSenha = value; OnPropertyChanged(); } }
        public string MensagemErroGeral { get => _mensagemErroGeral; set { _mensagemErroGeral = value; OnPropertyChanged(); } }
        public string MensagemErroToken { get => _mensagemErroToken; set { _mensagemErroToken = value; OnPropertyChanged(); } }

        /// <summary>
        /// commands
        /// </summary>
        public ICommand SolicitarAcessoCommand { get; }
        public ICommand ValidarTokenCommand { get; }
        public ICommand FinalizarCadastroCommand { get; }
        public ICommand RevelarSenhaCadastroCommand { get; }

        /// <summary>
        /// construtor
        /// </summary>
        /// <param name="autorizarService"></param>
        public CadastroViewModel(IAutorizarService? autorizarService = null)
        {
            _autorizarService = autorizarService ?? new AutorizarService();
            _conviteService = new ConviteService();

            SolicitarAcessoCommand = new RelayCommand(async _ => await SolicitarAcessoAsync(), _ => !Ocupado);
            ValidarTokenCommand = new RelayCommand(async _ => await ValidarTokenAsync(), _ => !OcupadoToken);
            FinalizarCadastroCommand = new RelayCommand(async p => await FinalizarCadastroAsync(p), _ => !Ocupado);
            RevelarSenhaCadastroCommand = new RelayCommand(p => AlternarVisibilidadeSenha(p));
        }

        /// ETAPA 1: verificar e-mail 
        /// <summary>
        /// Verifica APENAS se o e-mail informado possui um convite válido
        /// pendente no Firebase. Não gera token — o Administrador já gerou.
        /// Se houver convite válido, avança para a etapa 2 (digitação do token).
        /// </summary>
        private async Task SolicitarAcessoAsync()
        {
            LimparMensagensErro();

            if (string.IsNullOrWhiteSpace(Email))
            {
                MensaEmail = "O e-mail é obrigatório.";
                return;
            }

            // Validação básica de formato
            if (!Email.Contains('@') || !Email.Contains('.'))
            {
                MensaEmail = "Informe um endereço de e-mail válido.";
                return;
            }

            try
            {
                Ocupado = true;

                /// Verifica no Firebase se existe pelo menos um convite
                /// para este e-mail que ainda não foi usado e não expirou.
                bool temConvite = await _conviteService.ExisteConvitePendenteAsync(Email.Trim());

                if (temConvite)
                {
                    /// Avança para a etapa 2: digitar o token enviado pelo Administrador
                    SolicitacaoEnviada = true;
                    MensagemErroGeral = "E-mail localizado. Digite o token que o Administrador enviou a você.";
                }
                else
                {
                    /// E-mail não tem convite — bloqueia sem dar informação útil a invasores
                    MensaEmail = "Este e-mail não possui um convite de acesso válido. Entre em contato com o Administrador.";
                }
            }
            catch
            {
                MensagemErroGeral = "Erro ao verificar acesso. Tente novamente.";
            }
            finally
            {
                Ocupado = false;
            }
        }

        /// ETAPA 2: validar token
        /// <summary>
        /// Valida o token digitado pelo usuário consultando o Firebase.
        /// O token deve:
        ///   • Existir no nó convites/
        ///   • Pertencer ao e-mail informado na etapa 1
        ///   • Estar dentro do prazo de validade (48h)
        ///   • Não ter sido usado anteriormente
        /// </summary>
        private async Task ValidarTokenAsync()
        {
            MensagemErroToken = string.Empty;

            if (string.IsNullOrWhiteSpace(TokenDigitado))
            {
                MensagemErroToken = "Por favor, insira o token de ativação.";
                return;
            }

            try
            {
                OcupadoToken = true;

                bool valido = await _conviteService.ValidarTokenAsync(
                    Email!.Trim(),
                    TokenDigitado.Trim().ToUpper());

                if (valido)
                {
                    IsTokenValido = true;
                    MensagemErroToken = string.Empty;
                    MensagemErroGeral = string.Empty;
                }
                else
                {
                    MensagemErroToken = "Token inválido, expirado ou já utilizado. Solicite um novo convite ao Administrador.";
                }
            }
            catch
            {
                MensagemErroToken = "Falha na comunicação. Tente novamente.";
            }
            finally
            {
                OcupadoToken = false;
            }
        }

        /// ETAPA 3: finalizar cadastro
        /// <summary>
        /// Cria a conta do usuário na API após todas as validações passarem.
        /// Ao finalizar, marca o token como usado no Firebase (uso único).
        /// </summary>
        private async Task FinalizarCadastroAsync(object parameter)
        {
            LimparMensagensErro();
            bool possuiErro = false;
            string senha = string.Empty;

            if (parameter is Grid grid)
            {
                PasswordBox? pb = null;
                TextBox? tb = null;
                foreach (var child in grid.Children)
                {
                    if (child is PasswordBox p) pb = p;
                    else if (child is TextBox t) tb = t;
                }
                if (pb != null && tb != null)
                    senha = pb.Visibility == Visibility.Visible ? pb.Password : tb.Text;
            }

            if (string.IsNullOrWhiteSpace(Nome)) { MensaNome = "O nome completo é obrigatório."; possuiErro = true; }
            if (string.IsNullOrWhiteSpace(Login)) { MensaLogin = "O login é obrigatório."; possuiErro = true; }
            if (string.IsNullOrWhiteSpace(senha)) { MensaSenha = "A senha é obrigatória."; possuiErro = true; }

            if (string.IsNullOrWhiteSpace(CPF))
            {
                MensaCpf = "O CPF é obrigatório.";
                possuiErro = true;
            }
            else
            {
                /// Cria uma variável limpa, apenas com números
                string cpfLimpo = System.Text.RegularExpressions.Regex.Replace(CPF, @"[^\d]", "");

                /// Passa o CPF SEM pontos e SEM hífen para o validador
                if (!ValidacaoCpfService.Validar(cpfLimpo))
                {
                    MensaCpf = "CPF inválido. Verifique os dígitos informados.";
                    possuiErro = true;
                }
            }

            if (possuiErro) return;

            try
            {
                Ocupado = true;

                string cpfFormatado = ValidacaoCpfService.Formatar(CPF!);

                /// Cria a conta na API com o e-mail já validado pelo convite
                bool cadastrado = await _autorizarService.FinalizarCadastroAsync(
                    Nome!, cpfFormatado, Email!, Login!, senha,
                    TokenDigitado.Trim().ToUpper());

                if (cadastrado)
                {
                    /// Marca o token como usado — impede reuso imediato
                    await _conviteService.MarcarComoUsadoAsync(TokenDigitado.Trim().ToUpper());

                    CadastroFinalizadoComSucesso = true;
                    MensagemErroGeral = "Conta criada com sucesso! Faça login para continuar.";
                }
                else
                {
                    MensagemErroGeral = "Erro ao criar a conta. Verifique os dados e tente novamente.";
                }
            }
            catch
            {
                MensagemErroGeral = "Erro ao conectar com o servidor. Tente novamente.";
            }
            finally
            {
                Ocupado = false;
            }
        }

        /// <summary>
        ///  Muda visibilidade da senha
        /// </summary>
        /// <param name="parameter"></param>
        private void AlternarVisibilidadeSenha(object parameter)
        {
            if (parameter is not Grid grid) return;

            PasswordBox? pb = null;
            TextBox? tb = null;
            PackIconMaterial? icon = null;

            foreach (var child in grid.Children)
            {
                if (child is PasswordBox p) pb = p;
                else if (child is TextBox t) tb = t;
                else if (child is Button btn && btn.Content is PackIconMaterial ic) icon = ic;
            }

            if (pb == null || tb == null) return;

            if (pb.Visibility == Visibility.Visible)
            {
                tb.Text = pb.Password;
                pb.Visibility = Visibility.Collapsed;
                tb.Visibility = Visibility.Visible;
                if (icon != null) icon.Kind = PackIconMaterialKind.EyeOff;
            }
            else
            {
                pb.Password = tb.Text;
                tb.Visibility = Visibility.Collapsed;
                pb.Visibility = Visibility.Visible;
                if (icon != null) icon.Kind = PackIconMaterialKind.Eye;
            }
        }

        private void LimparMensagensErro()
        {
            MensaNome = MensaCpf = MensaEmail = MensaLogin =
            MensaSenha = MensagemErroGeral = MensagemErroToken = string.Empty;
        }
    }
}
