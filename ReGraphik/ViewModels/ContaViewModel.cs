using Microsoft.Win32;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly IResiduoService _residuoService;
        private string _emailReal = string.Empty;

        private BitmapImage? _imgFoto;
        public BitmapImage? ImgFoto
        {
            get => _imgFoto;
            set { _imgFoto = value; OnPropertyChanged(); OnPropertyChanged(nameof(SemFoto)); }
        }

        /// <summary>
        /// Indica se o usuário não possui foto — exibe a inicial do nome no lugar
        /// </summary>
        public bool SemFoto => ImgFoto == null;

        /// <summary>
        /// Inicial do nome para exibir no avatar quando não há foto
        /// </summary>
        public string InicialNome => string.IsNullOrWhiteSpace(Nome) ? "?" : Nome[..1].ToUpper();

        /// <summary>
        /// Login formatado com @ para exibição no card do perfil
        /// </summary>
        public string LoginExibicao => string.IsNullOrWhiteSpace(Login) ? string.Empty : $"@{Login}";

        /// <summary>
        /// E-mail mascarado para exibição no resumo da conta
        /// </summary>
        public string EmailResumido => MascararEmail(_emailReal);

        private string? _nome;
        public string Nome
        {
            get => _nome;
            set { _nome = value; OnPropertyChanged(); OnPropertyChanged(nameof(InicialNome)); }
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
            set { _login = value; OnPropertyChanged(); OnPropertyChanged(nameof(LoginExibicao)); }
        }

        private string? _perfil;
        public string Perfil
        {
            get => _perfil;
            set { _perfil = value; OnPropertyChanged(); }
        }

        private bool _ocupado;
        public bool Ocupado
        {
            get => _ocupado;
            set { _ocupado = value; OnPropertyChanged(); }
        }

        // ── Estatísticas ─────────────────────────────────────────

        /// <summary>
        /// Total de resíduos cadastrados pelo usuário logado
        /// </summary>
        private int _totalResiduos;
        public int TotalResiduos
        {
            get => _totalResiduos;
            set { _totalResiduos = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Total de resíduos reaproveitados pelo usuário logado
        /// </summary>
        private int _totalReaproveitados;
        public int TotalReaproveitados
        {
            get => _totalReaproveitados;
            set { _totalReaproveitados = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Valor econômico total gerado pelo usuário, formatado em moeda
        /// </summary>
        private string _valorEconomico = "R$ 0,00";
        public string ValorEconomico
        {
            get => _valorEconomico;
            set { _valorEconomico = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Todos os resíduos cadastrados pelo usuário, do mais recente ao mais antigo
        /// </summary>
        private ObservableCollection<Residuo> _ultimosResiduos = new();
        public ObservableCollection<Residuo> UltimosResiduos
        {
            get => _ultimosResiduos;
            set { _ultimosResiduos = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Controla a animação de giro do botão atualizar enquanto os dados carregam
        /// </summary>
        private bool _carregandoEstatisticas;
        public bool CarregandoEstatisticas
        {
            get => _carregandoEstatisticas;
            set { _carregandoEstatisticas = value; OnPropertyChanged(); }
        }

        // ── Mensagens inline ─────────────────────────────────────

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

        /// Comandos da ViewModel
        public ICommand SalvarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand EmailGotFocusCommand { get; }
        public ICommand EmailLostFocusCommand { get; }
        public ICommand SelecionarFotoCommand { get; }
        public ICommand AtualizarEstatisticasCommand { get; }

        public ContaViewModel(Usuario usuario, IAutorizarService autorizarService)
        {
            _usuarioAtual = usuario;
            _autorizarService = autorizarService;
            _residuoService = new ResiduoService();

            CarregarDadosNaTela();

            /// Inicialização dos comandos
            SalvarCommand = new RelayCommand(async (param) => await SalvarPerfilAsync(param));
            CancelarCommand = new RelayCommand(_ => CarregarDadosNaTela());
            EmailGotFocusCommand = new RelayCommand(EmailGotFocus);
            EmailLostFocusCommand = new RelayCommand(EmailLostFocus);
            SelecionarFotoCommand = new RelayCommand(_ => MudarFoto());
            AtualizarEstatisticasCommand = new RelayCommand(async _ => await CarregarEstatisticasAsync());

            /// Carrega foto persistida do disco ao abrir a tela
            var fotoSalva = ConfiguracaoLocalService.CarregarFoto();
            if (fotoSalva != null)
                CarregarBitmapDoCaminho(fotoSalva);

            /// Carrega as estatísticas do usuário em background ao abrir a tela
            _ = CarregarEstatisticasAsync();
        }

        private void CarregarDadosNaTela()
        {
            Nome = _usuarioAtual.Nome ?? string.Empty;
            Login = _usuarioAtual.Login ?? string.Empty;
            Perfil = _usuarioAtual.Perfil ?? "Usuário";

            /// CPF: mascarado e bloqueado — não pode ser editado pelo usuário
            CPF = MascararCpf(_usuarioAtual.CPF);

            /// Email: mascarado mas editável — revela ao focar e mascara ao sair
            _emailReal = _usuarioAtual.Email ?? string.Empty;
            Email = MascararEmail(_emailReal);

            OnPropertyChanged(nameof(EmailResumido));
        }

        /// <summary>
        /// Carrega as estatísticas do usuário logado a partir da API de resíduos.
        /// Também é chamado pelo botão de atualizar com animação de giro.
        /// </summary>
        private async Task CarregarEstatisticasAsync()
        {
            try
            {
                CarregandoEstatisticas = true;

                ///  Obtém todos os resíduos do usuário logado a partir do serviço de resíduos
                var todos = await _residuoService.ObterTodosResiduosAsync();

                var meus = todos.ToList();

                /// Calcula o total de resíduos e o total de reaproveitados
                TotalResiduos = meus.Count;
                TotalReaproveitados = meus.Count(r => r.Status == "Reaproveitado");

                /// Calcula o valor econômico total considerando que cada unidade de resíduo vale R$ 5,50
                double somaValores = meus.Sum(r => r.Quantidade * 5.50);
                ValorEconomico = somaValores.ToString("C2");

                var todos_ordenados = meus
                    .Where(r => r.DataCadastro != null)
                    .OrderByDescending(r => r.DataCadastro)
                    .ToList();

                /// Atualiza a coleção de resíduos na thread da UI para evitar problemas de threading
                Application.Current.Dispatcher.Invoke(() =>
                    UltimosResiduos = new ObservableCollection<Residuo>(todos_ordenados));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar estatísticas: {ex.Message}");
                TotalResiduos = 0;
                TotalReaproveitados = 0;
                ValorEconomico = "R$ 0,00";
            }
            finally
            {
                CarregandoEstatisticas = false;
            }
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
            /// Valida o formato básico do e-mail ao sair do campo
            if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains('@'))
            {
                MensagemErroEmail = "E-mail inválido. Verifique o endereço informado.";
            }
            else
            {
                MensagemErroEmail = string.Empty;
                _emailReal = Email ?? string.Empty;
                _usuarioAtual.Email = _emailReal;
                OnPropertyChanged(nameof(EmailResumido));
            }

            Email = MascararEmail(_emailReal);
        }

        private void MudarFoto()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Selecionar Foto de Perfil",
                    Filter = "Imagens (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
                };

                if (openFileDialog.ShowDialog() != true) return;

                string caminho = openFileDialog.FileName;

                /// Carrega a imagem em memória para exibição imediata na tela
                CarregarBitmapDoCaminho(caminho);

                /// Persiste o caminho da foto no modelo e no serviço de sessão compartilhado
                _usuarioAtual.FotoPerfil = caminho;
                UsuarioSessaoService.Instancia.FotoCaminho = caminho;

                /// Salva localmente para persistir entre sessões
                ConfiguracaoLocalService.SalvarFoto(caminho);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar a foto: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Carrega um BitmapImage a partir de um caminho local de forma segura
        /// </summary>
        private void CarregarBitmapDoCaminho(string caminho)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(caminho);
            bitmap.EndInit();
            ImgFoto = bitmap;
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
            /// Limpa mensagens anteriores
            MensagemSucesso = string.Empty;
            MensagemErroGeral = string.Empty;

            if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Login))
            {
                MensagemErroGeral = "Nome e Login são obrigatórios.";
                return;
            }

            /// Bloqueia o salvamento se o e-mail estiver inválido
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
                    /// Exibe mensagem de sucesso inline sem MessageBox
                    MensagemSucesso = "✔ Dados atualizados com sucesso!";
                    Email = MascararEmail(_emailReal);
                    OnPropertyChanged(nameof(EmailResumido));
                    OnPropertyChanged(nameof(LoginExibicao));

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