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
        private string _caminhoArquivoSelecionado = string.Empty;

        /// <summary>
        /// Coleções de opções para os ComboBoxes, carregadas do arquivo de configuração <c>appsettings.json</c>.
        /// </summary>
        public ObservableCollection<string> TiposMaterial { get; }
        public ObservableCollection<string> Especificacoes { get; }
        public ObservableCollection<string> Origens { get; }
        public ObservableCollection<string> Condicoes { get; }
        public ObservableCollection<string> StatusOpcoes { get; }
        public ObservableCollection<string> UnidadesMedida { get; }

        /// <summary>
        /// Propriedades vinculadas aos campos do formulário de cadastro de resíduos.
        /// </summary>
        private string? _tipoMaterial;
        public string? TipoMaterial
        {
            get => _tipoMaterial;
            set { _tipoMaterial = value; OnPropertyChanged(); }
        }

        private string _especificacao = string.Empty;
        public string Especificacao
        {
            get => _especificacao;
            set { _especificacao = value; OnPropertyChanged(); }
        }

        private string? _origem;
        public string? Origem
        {
            get => _origem;
            set { _origem = value; OnPropertyChanged(); }
        }

        private string _projetoOrigem = string.Empty;
        public string ProjetoOrigem
        {
            get => _projetoOrigem;
            set { _projetoOrigem = value; OnPropertyChanged(); }
        }

        private double _quantidade;
        public double Quantidade
        {
            get => _quantidade;
            set { _quantidade = value; OnPropertyChanged(); }
        }

        private string _unidadeMedida = "kg";
        public string UnidadeMedida
        {
            get => _unidadeMedida;
            set { _unidadeMedida = value; OnPropertyChanged(); }
        }

        private DateTime _data = DateTime.Now;
        public DateTime Data
        {
            get => _data;
            set { _data = value; OnPropertyChanged(); }
        }

        private string? _condicao;
        public string? Condicao
        {
            get => _condicao;
            set { _condicao = value; OnPropertyChanged(); }
        }

        private double _comprimento;
        public double Comprimento
        {
            get => _comprimento;
            set { _comprimento = value; OnPropertyChanged(); }
        }

        private double _largura;
        public double Largura
        {
            get => _largura;
            set { _largura = value; OnPropertyChanged(); }
        }

        private string _observacoes = string.Empty;
        public string Observacoes
        {
            get => _observacoes;
            set { _observacoes = value; OnPropertyChanged(); }
        }

        private string? _status;
        public string? Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private string _nomeArquivo = "Nenhum arquivo selecionado";
        public string NomeArquivo
        {
            get => _nomeArquivo;
            set { _nomeArquivo = value; OnPropertyChanged(); }
        }

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

        /// <summary>
        /// Comandos vinculados aos botões da interface, responsáveis por salvar o 
        /// resíduo, limpar os campos e selecionar um arquivo para anexar.
        /// </summary>
        public ICommand SalvarResiduoCommand { get; }
        public ICommand LimparCommand { get; }
        public ICommand SelecionarArquivoCommand { get; }

        /// <summary>
        /// Inicializa uma nova instância do <see cref="ResiduoViewModel"/>, 
        /// carregando as opções dos ComboBoxes a partir do arquivo de configuração e configurando os comandos da interface.
        /// </summary>
        public ResiduoViewModel()
        {
            var dropdowns = AppSettings.Dropdowns;
            TiposMaterial = new ObservableCollection<string>(dropdowns.TiposMaterial);
            Especificacoes = new ObservableCollection<string>(dropdowns.Especificacoes);
            Origens = new ObservableCollection<string>(dropdowns.Origens);
            Condicoes = new ObservableCollection<string>(dropdowns.Condicoes);
            StatusOpcoes = new ObservableCollection<string>(dropdowns.Status);
            UnidadesMedida = new ObservableCollection<string>(dropdowns.UnidadesMedida);

            SalvarResiduoCommand = new RelayCommand(async () => await SalvarResiduoAsync());
            SelecionarArquivoCommand = new RelayCommand(async () => await SelecionarArquivoAsync());
            LimparCommand = new RelayCommand(LimparCampos);
        }

        /// <summary>
        /// Valida os campos do formulário e, se todos estiverem corretos, cria um novo objeto <see cref="Residuo"/> e o salva no Firebase.
        /// </summary>
        /// <returns></returns>
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

                /// Cria um token de cancelamento com timeout de 60 segundos para operações assíncronas, 
                /// garantindo que a leitura do arquivo não bloqueie indefinidamente.
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                string base64Payload = string.Empty;

                /// Se um arquivo foi selecionado, lê o conteúdo do arquivo e converte para Base64.
                if (!string.IsNullOrEmpty(_caminhoArquivoSelecionado) && File.Exists(_caminhoArquivoSelecionado))
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(_caminhoArquivoSelecionado, cts.Token).ConfigureAwait(false);
                    base64Payload = Convert.ToBase64String(fileBytes);
                }

                /// Cria um novo objeto Residuo com os dados do formulário.
                var novoResiduo = new Residuo
                {
                    Id = Guid.NewGuid().ToString(),
                    TipoResiduo = TipoMaterial!,
                    Especificacao = Especificacao,
                    Origem = Origem!,
                    Projeto = ProjetoOrigem,
                    Quantidade = Quantidade,
                    UnidadeMedida = UnidadeMedida,
                    DataCadastro = Data,
                    Condicao = Condicao ?? string.Empty,
                    DimensoesCm = Comprimento,
                    DimensoesLm = Largura,
                    Observacao = Observacoes,
                    Anexo = base64Payload,
                    Status = Status ?? string.Empty
                };

                try
                {
                    var firebase = new FirebaseClient(AppSettings.FirebaseDatabaseUrl);

                    await firebase
                        .Child("residuos")
                        .Child(novoResiduo.Id)
                        .PutAsync(novoResiduo);

                    /// Exibe uma mensagem de sucesso na interface do usuário, garantindo que a chamada seja feita na thread correta.
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MensagemWindow.Exibir("Sucesso!", "O resíduo foi cadastrado com sucesso no sistema.", MensagemWindow.TipoMensagem.Sucesso);
                    });

                    LimparCampos();
                }
                catch (Exception ex)
                {
                    /// Exibe uma mensagem de erro na interface do usuário, garantindo que a chamada seja feita na thread correta.
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MensagemWindow.Exibir("Erro no Banco", $"Erro ao salvar os dados no banco: {ex.Message}", MensagemWindow.TipoMensagem.Erro);
                    });
                }
            }
            catch (Exception ex)
            {
                /// Exibe uma mensagem de erro inesperado na interface do usuário, garantindo que a chamada seja feita na thread correta.
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro Inesperado", ex.Message, MensagemWindow.TipoMensagem.Erro);
                });
            }
        }

        /// <summary>
        /// Abre um diálogo para o usuário selecionar um arquivo de imagem, valida o tamanho do
        /// arquivo e atualiza as propriedades correspondentes no ViewModel.
        /// </summary>
        /// <returns></returns>
        private async Task SelecionarArquivoAsync()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens|*.jpg;*.jpeg;*.png;",
                Title = "Selecione um arquivo para anexar"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);

                if (fileInfo.Length > 2 * 1024 * 1024)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MensagemWindow.Exibir(
                            "Arquivo muito grande",
                            "Arquivos maiores que 2 MB não são suportados para envio direto via JSON.",
                            MensagemWindow.TipoMensagem.Aviso);
                    });
                    return;
                }

                NomeArquivo = openFileDialog.SafeFileName;
                _caminhoArquivoSelecionado = openFileDialog.FileName;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Limpa todos os campos do formulário de cadastro de resíduos,
        /// </summary>
        private void LimparCampos()
        {
            TipoMaterial = null;
            Especificacao = string.Empty;
            Origem = null;
            ProjetoOrigem = string.Empty;
            Quantidade = 0;
            UnidadeMedida = "kg";
            Data = DateTime.Now;
            Condicao = null;
            Comprimento = 0;
            Largura = 0;
            Observacoes = string.Empty;
            Status = null;
            NomeArquivo = "Nenhum arquivo selecionado";
            _caminhoArquivoSelecionado = string.Empty;

            LimparMensagens();
        }

        /// <summary>
        /// Limpa todas as mensagens de erro e alerta exibidas na tela.
        /// </summary>
        private void LimparMensagens()
        {
            MensaTipoMaterial = string.Empty;
            MensaOrigem = string.Empty;
            MensaQuantidade = string.Empty;
            MensaComprimento = string.Empty;
            MensaLargura = string.Empty;
            MensagemErroGeral = string.Empty;
            MensagemAlerta = string.Empty;
        }
    }
}