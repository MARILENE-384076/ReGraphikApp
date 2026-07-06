using Microsoft.Win32;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
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

        /// <summary>
        /// Guarda temporariamente o caminho da nova foto selecionada antes de salvar na API
        /// </summary>
        private string _caminhoNovaFotoSelecionada = string.Empty;

        private string? _fotoPerfilCaminho;
        /// <summary>
        /// Obtém ou define o caminho (local ou URL) da foto de perfil para o XAML tratar nativamente.
        /// </summary>
        public string? FotoPerfilCaminho
        {
            get => _fotoPerfilCaminho;
            set
            {
                _fotoPerfilCaminho = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SemFoto));
            }
        }

        /// <summary>
        /// Indica se o usuário não possui foto — exibe a inicial do nome no lugar
        /// </summary>
        public bool SemFoto => string.IsNullOrWhiteSpace(FotoPerfilCaminho);

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

        /// ── Estatísticas ─────────────────────────────────────────

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

        /// ── Mensagens inline ─────────────────────────────────────

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

            /// Define o caminho inicial vindo da API ou do serviço de sessão
            string? fotoInicial = !string.IsNullOrEmpty(_usuarioAtual.FotoPerfil)
                ? _usuarioAtual.FotoPerfil
                : UsuarioSessaoService.Instancia.FotoCaminho;

            FotoPerfilCaminho = fotoInicial;

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

                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir(
                        "Erro",
                        $"Não foi possível carregar as estatísticas!!!",
                        MensagemWindow.TipoMensagem.Erro);
                });

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

        /// <summary>
        /// Método para abrir o diálogo de seleção de arquivo e permitir que o usuário escolha uma nova foto de perfil.
        /// </summary>
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

                _caminhoNovaFotoSelecionada = openFileDialog.FileName;
                FotoPerfilCaminho = _caminhoNovaFotoSelecionada;
            }
            catch (Exception)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro", "Erro ao carregar a foto de perfil!!!", MensagemWindow.TipoMensagem.Erro);
                });
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
            try
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

                string novaSenha = string.Empty;
                if (parameter is PasswordBox passwordBox)
                    novaSenha = passwordBox.Password;

                _usuarioAtual.Nome = Nome;
                _usuarioAtual.Login = Login;
                _usuarioAtual.Email = _emailReal;

                if (!string.IsNullOrWhiteSpace(novaSenha))
                    _usuarioAtual.Senha = novaSenha;

                Ocupado = true;
                bool sucesso = false;

                // Executa a chamada correta dependendo de haver ou não alteração na foto de perfil
                if (!string.IsNullOrEmpty(_caminhoNovaFotoSelecionada) && File.Exists(_caminhoNovaFotoSelecionada))
                {
                    string? novaUrlFoto = await _autorizarService.AtualizarComFotoAsync(_usuarioAtual.Id, _usuarioAtual, _caminhoNovaFotoSelecionada);
                    sucesso = novaUrlFoto != null;

                    if (sucesso)
                    {
                        _usuarioAtual.FotoPerfil = novaUrlFoto;
                        UsuarioSessaoService.Instancia.FotoCaminho = novaUrlFoto;
                        ConfiguracaoLocalService.SalvarFoto(novaUrlFoto);
                        FotoPerfilCaminho = novaUrlFoto;
                        _caminhoNovaFotoSelecionada = string.Empty;
                    }
                }
                else
                {
                    sucesso = await _autorizarService.AtualizarAsync(_usuarioAtual.Id, _usuarioAtual);
                }

                if (sucesso)
                {
                    Email = MascararEmail(_emailReal);
                    OnPropertyChanged(nameof(EmailResumido));
                    OnPropertyChanged(nameof(LoginExibicao));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MensagemWindow.Exibir("Sucesso!", "Os dados do perfil foram atualizados com sucesso.", MensagemWindow.TipoMensagem.Sucesso);
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MensagemWindow.Exibir("Erro", "Erro ao atualizar os dados. Tente novamente mais tarde.", MensagemWindow.TipoMensagem.Erro);
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir(
                        "Erro",
                        $"Erro de conexão!!!",
                        MensagemWindow.TipoMensagem.Erro);
                });
            }
            finally
            {
                Ocupado = false;
            }
        }
    }
}