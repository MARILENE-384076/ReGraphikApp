using Firebase.Database;
using Firebase.Database.Query;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Views;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel responsável pela tela de cadastro de resíduos.
    /// Gerencia validação de campos, seleção de arquivo, envio ao Firebase
    /// e carregamento dinâmico dos ComboBoxes a partir do <see cref="AppSettings"/>.
    /// </summary>
    public class ResiduoViewModel : BaseViewModel
    {
        // ─── Infraestrutura ────────────────────────────────────────────────────────
        private string _caminhoArquivoSelecionado = string.Empty;

        // ─── Listas para ComboBoxes (carregadas do appsettings.json) ─────────────

        /// <summary>Tipos de material disponíveis no formulário.</summary>
        public ObservableCollection<string> TiposMaterial { get; }

        /// <summary>Especificações técnicas disponíveis no formulário.</summary>
        public ObservableCollection<string> Especificacoes { get; }

        /// <summary>Origens possíveis do resíduo no formulário.</summary>
        public ObservableCollection<string> Origens { get; }

        /// <summary>Condições físicas do material disponíveis no formulário.</summary>
        public ObservableCollection<string> Condicoes { get; }

        /// <summary>Status possíveis para o resíduo no formulário.</summary>
        public ObservableCollection<string> StatusOpcoes { get; }

        /// <summary>Unidades de medida disponíveis para o campo Quantidade.</summary>
        public ObservableCollection<string> UnidadesMedida { get; }

        // ─── Campos do formulário ─────────────────────────────────────────────────

        private string? _tipoMaterial;
        /// <summary>Tipo de material selecionado pelo usuário.</summary>
        public string? TipoMaterial
        {
            get => _tipoMaterial;
            set { _tipoMaterial = value; OnPropertyChanged(); }
        }

        private string _especificacao = string.Empty;
        /// <summary>Especificação técnica do material.</summary>
        public string Especificacao
        {
            get => _especificacao;
            set { _especificacao = value; OnPropertyChanged(); }
        }

        private string? _origem;
        /// <summary>Origem do resíduo dentro do processo produtivo.</summary>
        public string? Origem
        {
            get => _origem;
            set { _origem = value; OnPropertyChanged(); }
        }

        private string _projetoOrigem = string.Empty;
        /// <summary>Nome do projeto de onde o resíduo originou.</summary>
        public string ProjetoOrigem
        {
            get => _projetoOrigem;
            set { _projetoOrigem = value; OnPropertyChanged(); }
        }

        private double _quantidade;
        /// <summary>Quantidade numérica do resíduo.</summary>
        public double Quantidade
        {
            get => _quantidade;
            set { _quantidade = value; OnPropertyChanged(); }
        }

        private string _unidadeMedida = "kg";
        /// <summary>Unidade de medida da quantidade (ex: kg, g, ton).</summary>
        public string UnidadeMedida
        {
            get => _unidadeMedida;
            set { _unidadeMedida = value; OnPropertyChanged(); }
        }

        private DateTime _data = DateTime.Now;
        /// <summary>Data de cadastro do resíduo.</summary>
        public DateTime Data
        {
            get => _data;
            set { _data = value; OnPropertyChanged(); }
        }

        private string? _condicao;
        /// <summary>Condição física do material.</summary>
        public string? Condicao
        {
            get => _condicao;
            set { _condicao = value; OnPropertyChanged(); }
        }

        private double _comprimento;
        /// <summary>Comprimento do resíduo em centímetros.</summary>
        public double Comprimento
        {
            get => _comprimento;
            set { _comprimento = value; OnPropertyChanged(); }
        }

        private double _largura;
        /// <summary>Largura do resíduo em centímetros.</summary>
        public double Largura
        {
            get => _largura;
            set { _largura = value; OnPropertyChanged(); }
        }

        private string _observacoes = string.Empty;
        /// <summary>Observações adicionais sobre o resíduo.</summary>
        public string Observacoes
        {
            get => _observacoes;
            set { _observacoes = value; OnPropertyChanged(); }
        }

        private string? _status;
        /// <summary>Status atual do resíduo.</summary>
        public string? Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private string _nomeArquivo = "Nenhum arquivo selecionado";
        /// <summary>Nome amigável do arquivo de imagem selecionado.</summary>
        public string NomeArquivo
        {
            get => _nomeArquivo;
            set { _nomeArquivo = value; OnPropertyChanged(); }
        }

        // ─── Mensagens de validação ───────────────────────────────────────────────

        private string _mensaTipoMaterial = string.Empty;
        public string MensaTipoMaterial
        {
            get => _mensaTipoMaterial;
            set { _mensaTipoMaterial = value; OnPropertyChanged(); }
        }

        private string _mensaOrigem = string.Empty;
        public string MensaOrigem
        {
            get => _mensaOrigem;
            set { _mensaOrigem = value; OnPropertyChanged(); }
        }

        private string _mensaQuantidade = string.Empty;
        public string MensaQuantidade
        {
            get => _mensaQuantidade;
            set { _mensaQuantidade = value; OnPropertyChanged(); }
        }

        private string _mensaComprimento = string.Empty;
        public string MensaComprimento
        {
            get => _mensaComprimento;
            set { _mensaComprimento = value; OnPropertyChanged(); }
        }

        private string _mensaLargura = string.Empty;
        public string MensaLargura
        {
            get => _mensaLargura;
            set { _mensaLargura = value; OnPropertyChanged(); }
        }

        private string _mensagemErroGeral = string.Empty;
        public string MensagemErroGeral
        {
            get => _mensagemErroGeral;
            set { _mensagemErroGeral = value; OnPropertyChanged(); }
        }

        private string _mensagemAlerta = string.Empty;
        public string MensagemAlerta
        {
            get => _mensagemAlerta;
            set { _mensagemAlerta = value; OnPropertyChanged(); }
        }

        // ─── Comandos ─────────────────────────────────────────────────────────────

        public ICommand SalvarResiduoCommand { get; }
        public ICommand LimparCommand { get; }
        public ICommand SelecionarArquivoCommand { get; }

        // ─── Construtor ───────────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa o ViewModel: carrega as listas dos ComboBoxes do appsettings.json
        /// e registra os comandos.
        /// </summary>
        public ResiduoViewModel()
        {
            // Carrega listas dos ComboBoxes do arquivo de configuração — sem hard-code
            var dropdowns = AppSettings.Dropdowns;
            TiposMaterial  = new ObservableCollection<string>(dropdowns.TiposMaterial);
            Especificacoes = new ObservableCollection<string>(dropdowns.Especificacoes);
            Origens        = new ObservableCollection<string>(dropdowns.Origens);
            Condicoes      = new ObservableCollection<string>(dropdowns.Condicoes);
            StatusOpcoes   = new ObservableCollection<string>(dropdowns.Status);
            UnidadesMedida = new ObservableCollection<string>(dropdowns.UnidadesMedida);

            SalvarResiduoCommand     = new RelayCommand(async () => await SalvarResiduoAsync());
            SelecionarArquivoCommand = new RelayCommand(async () => await SelecionarArquivoAsync());
            LimparCommand            = new RelayCommand(LimparCampos);
        }

        // ─── Lógica de salvamento ─────────────────────────────────────────────────

        /// <summary>
        /// Valida os campos obrigatórios e persiste o resíduo no Firebase Realtime Database.
        /// O ID do usuário logado é obtido via <see cref="UsuarioSessaoService"/>.
        /// </summary>
        private async Task SalvarResiduoAsync()
        {
            try
            {
                LimparMensagens();
                bool possuiErro = false;

                if (string.IsNullOrWhiteSpace(TipoMaterial))
                {
                    MensaTipoMaterial = "O Tipo de Material é obrigatório!";
                    possuiErro = true;
                }

                if (string.IsNullOrWhiteSpace(Origem))
                {
                    MensaOrigem = "A Origem é obrigatória!";
                    possuiErro = true;
                }

                if (Quantidade <= 0)
                {
                    MensaQuantidade = "A Quantidade deve ser maior que zero!";
                    possuiErro = true;
                }

                if (Largura <= 0)
                {
                    MensaLargura = "A Largura deve ser maior que zero!";
                    possuiErro = true;
                }

                if (Comprimento <= 0)
                {
                    MensaComprimento = "O Comprimento deve ser maior que zero!";
                    possuiErro = true;
                }

                if (possuiErro) return;

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                string base64Payload = string.Empty;

                if (!string.IsNullOrEmpty(_caminhoArquivoSelecionado) &&
                    File.Exists(_caminhoArquivoSelecionado))
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(
                        _caminhoArquivoSelecionado, cts.Token).ConfigureAwait(false);
                    base64Payload = Convert.ToBase64String(fileBytes);
                }

                // ID do usuário obtido da sessão — nunca um placeholder hard-coded
                string? idUsuario = UsuarioSessaoService.Instancia.IdUsuarioLogado;

                if (string.IsNullOrEmpty(idUsuario))
                {
                    MensagemErroGeral = "Sessão expirada. Faça login novamente.";
                    return;
                }

                var novoResiduo = new Residuo
                {
                    Id            = Guid.NewGuid().ToString(),
                    IdUsuario     = idUsuario,
                    TipoResiduo   = TipoMaterial!,
                    Especificacao = Especificacao,
                    Origem        = Origem!,
                    Projeto       = ProjetoOrigem,
                    Quantidade    = Quantidade,
                    UnidadeMedida = UnidadeMedida,
                    DataCadastro  = Data,
                    Condicao      = Condicao ?? string.Empty,
                    DimensoesCm   = Comprimento,
                    DimensoesLm   = Largura,
                    Observacao    = Observacoes,
                    Anexo         = base64Payload,
                    Status        = Status ?? string.Empty
                };

                try
                {
                    // URL do Firebase lida do AppSettings — sem hard-code
                    var firebase = new FirebaseClient(AppSettings.FirebaseDatabaseUrl);

                    await firebase
                        .Child("residuos")
                        .Child(novoResiduo.Id)
                        .PutAsync(novoResiduo);

                    MostrarMensagemSucesso();
                }
                catch (Exception ex)
                {
                    MensagemErroGeral = $"Erro ao salvar os dados no banco: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                MensagemErroGeral = $"Erro inesperado: {ex.Message}";
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        /// <summary>Exibe a janela de confirmação de sucesso e limpa o formulário.</summary>
        private void MostrarMensagemSucesso()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var msgWindow = new MensagemWindow
                {
                    ShowInTaskbar = false,
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                msgWindow.ShowDialog();
            });

            LimparCampos();
        }

        /// <summary>Abre o diálogo de seleção de imagem. Rejeita arquivos maiores que 2 MB.</summary>
        private async Task SelecionarArquivoAsync()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens|*.jpg;*.jpeg;*.png;",
                Title  = "Selecione um arquivo para anexar"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);

                if (fileInfo.Length > 2 * 1024 * 1024)
                {
                    MessageBox.Show(
                        "Arquivos maiores que 2 MB não são suportados para envio direto via JSON.",
                        "Arquivo muito grande",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                NomeArquivo = openFileDialog.SafeFileName;
                _caminhoArquivoSelecionado = openFileDialog.FileName;
            }

            await Task.CompletedTask;
        }

        /// <summary>Redefine todos os campos do formulário para os valores iniciais.</summary>
        private void LimparCampos()
        {
            TipoMaterial   = null;
            Especificacao  = string.Empty;
            Origem         = null;
            ProjetoOrigem  = string.Empty;
            Quantidade     = 0;
            UnidadeMedida  = "kg";
            Data           = DateTime.Now;
            Condicao       = null;
            Comprimento    = 0;
            Largura        = 0;
            Observacoes    = string.Empty;
            Status         = null;
            NomeArquivo    = "Nenhum arquivo selecionado";
            _caminhoArquivoSelecionado = string.Empty;

            LimparMensagens();
        }

        /// <summary>Limpa todas as mensagens de validação e erro do formulário.</summary>
        private void LimparMensagens()
        {
            MensaTipoMaterial = string.Empty;
            MensaOrigem       = string.Empty;
            MensaQuantidade   = string.Empty;
            MensaComprimento  = string.Empty;
            MensaLargura      = string.Empty;
            MensagemErroGeral = string.Empty;
            MensagemAlerta    = string.Empty;
        }
    }
}