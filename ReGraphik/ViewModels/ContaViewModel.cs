using Microsoft.Win32;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ReGraphik.ViewModels
{
    public class ContaViewModel : BaseViewModel
    {
        private readonly Usuario _usuarioAtual;
        private readonly IAutorizarService _autorizarService;
        private readonly UsuarioViewModel _viewModel;
        private string _emailReal = string.Empty;

        private BitmapImage? _imgFoto;
        public BitmapImage? ImgFoto
        {
            get => _imgFoto;
            set { _imgFoto = value; OnPropertyChanged(); }
        }

        private string? _nome;
        public string Nome
        {
            get => _nome;
            set { _nome = value; OnPropertyChanged(); }
        }

        private string? _cpf;
        public string CPF
        {
            get => _cpf;
            set { _cpf = value; OnPropertyChanged(); }
        }

        private string? _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string? _login;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        private bool _ocupado;
        public bool Ocupado
        {
            get => _ocupado;
            set { _ocupado = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mensagem de erro inline para o campo de e-mail, exibida abaixo do campo sem MessageBox
        /// </summary>
        private string _mensagemErroEmail = string.Empty;
        public string MensagemErroEmail
        {
            get => _mensagemErroEmail;
            set { _mensagemErroEmail = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mensagem de sucesso inline exibida abaixo do botão Salvar, sem MessageBox
        /// </summary>
        private string _mensagemSucesso = string.Empty;
        public string MensagemSucesso
        {
            get => _mensagemSucesso;
            set { _mensagemSucesso = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mensagem de erro geral inline exibida abaixo do botão Salvar
        /// </summary>
        private string _mensagemErroGeral = string.Empty;
        public string MensagemErroGeral
        {
            get => _mensagemErroGeral;
            set { _mensagemErroGeral = value; OnPropertyChanged(); }
        }

        // Comandos da ViewModel
        public ICommand SalvarCommand { get; }
        public ICommand EmailGotFocusCommand { get; }
        public ICommand EmailLostFocusCommand { get; }
        public ICommand SelecionarFotoCommand { get; }

        public ContaViewModel(Usuario usuario, IAutorizarService autorizarService)
        {
            _usuarioAtual = usuario;
            _autorizarService = autorizarService;
            _viewModel = new UsuarioViewModel();

            CarregarDadosNaTela();

            /// Inicialização dos comandos
            SalvarCommand = new RelayCommand(async (param) => await SalvarPerfilAsync(param));
            EmailGotFocusCommand = new RelayCommand(EmailGotFocus);
            EmailLostFocusCommand = new RelayCommand(EmailLostFocus);
            SelecionarFotoCommand = new RelayCommand((_) => MudarFoto());
        }

        private void CarregarDadosNaTela()
        {
            Nome = _usuarioAtual.Nome ?? string.Empty;
            Login = _usuarioAtual.Login ?? string.Empty;

            /// CPF: mascarado e bloqueado — não pode ser editado pelo usuário
            CPF = MascararCpf(_usuarioAtual.CPF);

            /// Email: mascarado mas editável — revela ao focar e mascara ao sair
            _emailReal = _usuarioAtual.Email ?? string.Empty;
            Email = MascararEmail(_emailReal);
        }

        /// <summary>
        /// Email: mostra o valor real ao focar no campo para permitir edição
        /// </summary>
        public void EmailGotFocus()
        {
            MensagemErroEmail = string.Empty;
            Email = _emailReal;
        }

        /// <summary>
        /// Email: salva o valor digitado e reaplica a máscara ao sair do campo
        /// </summary>
        public void EmailLostFocus()
        {
            if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains('@'))
            {
                MensagemErroEmail = "E-mail inválido. Verifique o endereço informado.";
            }
            else
            {
                MensagemErroEmail = string.Empty;
                _emailReal = Email ?? string.Empty;
                _usuarioAtual.Email = _emailReal;
            }

            Email = MascararEmail(_emailReal);
        }

        private async void MudarFoto()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Selecionar Foto de Perfil",
                    Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string caminhoDoArquivo = openFileDialog.FileName;

                    /// Carrega a imagem em memória para exibição imediata na tela
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(caminhoDoArquivo);
                    bitmap.EndInit();

                    ImgFoto = bitmap;

                    /// Persiste o caminho da foto no modelo e no serviço de sessão compartilhado
                    _usuarioAtual.FotoPerfil = caminhoDoArquivo;
                    UsuarioSessaoService.Instancia.FotoCaminho = caminhoDoArquivo;

                    /// Salva localmente para persistir entre sessões
                    ConfiguracaoLocalService.SalvarFoto(caminhoDoArquivo);

                    Ocupado = true;
                    await _autorizarService.AtualizarAsync(_usuarioAtual.Id, _usuarioAtual);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar ou salvar a foto: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Ocupado = false;
            }
        }

        /// <summary>
        /// Máscaras de dados sensíveis para exibição na tela
        /// </summary>
        private static string MascararCpf(string? cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return string.Empty;
            var d = Regex.Replace(cpf, @"\D", "");
            return d.Length >= 3 ? d[..3] + ".***.***-**" : cpf;
        }

        private static string MascararEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return string.Empty;
            var at = email.IndexOf('@');
            if (at <= 2) return email;
            return email[..2] + new string('*', at - 2) + email[at..];
        }

        /// <summary>
        /// Salva as alterações do perfil do usuário na API após validação dos campos
        /// </summary>
        private async Task SalvarPerfilAsync(object? parameter)
        {
            MensagemSucesso = string.Empty;
            MensagemErroGeral = string.Empty;

            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Login))
            {
                MensagemErroGeral = "Nome e Login são obrigatórios.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(MensagemErroEmail))
            {
                MensagemErroGeral = "Corrija os erros antes de salvar.";
                return;
            }

            /// Captura da senha de forma segura vinda da View via CommandParameter
            string novaSenha = string.Empty;
            if (parameter is PasswordBox passwordBox)
                novaSenha = passwordBox.Password;

            /// Atualiza o objeto com os dados da tela
            _usuarioAtual.Nome = Nome;
            _usuarioAtual.Login = Login;
            _usuarioAtual.Email = _emailReal;

            if (!string.IsNullOrWhiteSpace(novaSenha))
                _usuarioAtual.Senha = novaSenha;

            try
            {
                Ocupado = true;

                bool sucesso = await _autorizarService.AtualizarAsync(_usuarioAtual.Id, _usuarioAtual);

                if (sucesso)
                {
                    /// Informa o sistema inteiro sobre os novos dados
                    UsuarioSessaoService.Instancia.Nome = Nome;
                    UsuarioSessaoService.Instancia.Email = _emailReal;

                    /// Exibe mensagem de sucesso inline sem MessageBox
                    MensagemSucesso = "✔ Dados atualizados com sucesso!";
                    Email = MascararEmail(_emailReal);

                    /// Limpa a mensagem de sucesso após 3 segundos
                    await Task.Delay(3000);
                    MensagemSucesso = string.Empty;
                }
                else
                {
                    MensagemErroGeral = "Erro ao atualizar os dados. Tente novamente.";
                }
            }
            catch (Exception ex)
            {
                MensagemErroGeral = $"Erro de conexão: {ex.Message}";
            }
            finally
            {
                Ocupado = false;
            }
        }
    }
}