using ControlzEx.Standard;
using Firebase.Database;
using Firebase.Database.Query;
using MahApps.Metro.SimpleChildWindow;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel responsável por gerenciar a lógica de cadastro de resíduos na aplicação. Ele interage com a interface de usuário 
    /// (ResiduoView) para obter os dados do resíduo, validar as informações e enviar os dados para a API através de uma requisição HTTP POST.
    /// </summary>
    public class ResiduoViewModel : BaseViewModel
    {
        /// <summary>
        /// Instância do HttpClient, que é usada para realizar as requisições HTTP para a API. Ele é configurado com a URL base da API para facilitar as chamadas aos endpoints.
        /// </summary>
        private readonly HttpClient _httpClient;

        
        private string _caminhoArquivoSelecionado = string.Empty;

        // Opções de serialização reutilizadas para evitar overhead de JIT e reflexão em chamadas repetidas
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Propriedades para armazenar as credenciais de login e mensagens de erro, caso seja necessário autenticar o usuário antes de realizar o cadastro do resíduo.
        /// </summary>
        private string _mensaTipoMaterial;
        private string _mensaOrigem;
        private string _mensaData;
        private string _mensaQuantidade;
        private string _mensaComprimento;
        private string _mensaLargura;
        private string _mensagemErroGeral;

        /// <summary>
        /// Propriedades que representam os campos do formulário de cadastro de resíduos, como tipo de material, 
        /// especificação, origem, projeto de origem, quantidade, data, condição, dimensões e observações.
        /// </summary>
        private string _tipoMaterial;
        public string TipoMaterial
        {
            get => _tipoMaterial;
            set { _tipoMaterial = value; OnPropertyChanged(); }
        }

        private string _especificacao;
        public string Especificacao
        {
            get => _especificacao;
            set { _especificacao = value; OnPropertyChanged(); }
        }

        private string _origem;
        public string Origem
        {
            get => _origem;
            set { _origem = value; OnPropertyChanged(); }
        }

        private string _projetoOrigem;
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

        private DateTime _data = DateTime.Now;
        public DateTime Data
        {
            get => _data;
            set { _data = value; OnPropertyChanged(); }
        }

        private string _condicao;
        public string Condicao
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

        private string _observacoes;
        public string Observacoes
        {
            get => _observacoes;
            set { _observacoes = value; OnPropertyChanged(); }
        }

        private string _nomeArquivo = "Nenhum arquivo selecionado";
        public string NomeArquivo
        {
            get => _nomeArquivo;
            set { _nomeArquivo = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Propriedades para armazenar mensagens de erro específicas para cada campo do formulário, 
        /// que podem ser usadas para fornecer feedback ao usuário sobre validações ou erros de entrada.
        /// </summary>
        public string MensaTipoMaterial
        {
            get => _mensaTipoMaterial;
            set { _mensaTipoMaterial = value; OnPropertyChanged(); }
        }

        public string MensaOrigem
        {
            get => _mensaOrigem;
            set { _mensaOrigem = value; OnPropertyChanged(); }
        }

        public string MensaData
        {
            get => _mensaData;
            set { _mensaData = value; OnPropertyChanged(); }
        }

        public string MensaQuantidade
        {
            get => _mensaQuantidade;
            set { _mensaQuantidade = value; OnPropertyChanged(); }
        }

        public string MensaComprimento
        {
            get => _mensaComprimento;
            set { _mensaComprimento = value; OnPropertyChanged(); }
        }

        public string MensaLargura
        {
            get => _mensaLargura;
            set { _mensaLargura = value; OnPropertyChanged(); }
        }

        public string MensagemErroGeral
        {
            get => _mensagemErroGeral;
            set { _mensagemErroGeral = value; OnPropertyChanged(); }
        }

        private string _mensagemAlerta;
        public string MensagemAlerta
        {
            get => _mensagemAlerta;
            set { _mensagemAlerta = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comandos que são vinculados aos botões na interface de usuário para executar ações como salvar o resíduo, 
        /// limpar os campos do formulário e selecionar um arquivo para anexar.
        /// </summary>
        public ICommand SalvarResiduoCommand { get; }
        public ICommand LimparCommand { get; }

        public ICommand SelecionarArquivoCommand {  get; }

        public ICommand FecharDialogCommand {  get; }

        /// <summary>
        /// Construtor da classe ResiduoViewModel, onde o HttpClient é configurado com a URL base da API e os comandos são inicializados.
        /// </summary>
        public ResiduoViewModel()
        {
            /// Configura o HttpClient com a URL base da API
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://webregraphik.runasp.net/"),
                Timeout = TimeSpan.FromSeconds(60)
            };

            /// Inicializa os comandos com as ações correspondentes
            SalvarResiduoCommand = new RelayCommand(async () => await SalvarResiduoAsync());
            SelecionarArquivoCommand = new RelayCommand(async () => await SelecionarArquivoAsync());
            LimparCommand = new RelayCommand(LimparCampos);
        }

        /// <summary>
        /// Método assíncrono responsável por validar os dados do formulário, criar um objeto Residuo com as informações fornecidas 
        /// pelo usuário e enviar uma requisição HTTP POST para a API para salvar o resíduo. 
        /// </summary>
        /// <returns></returns>
        private async Task SalvarResiduoAsync()
        {
            MensaTipoMaterial = string.Empty;
            MensaOrigem = string.Empty;
            MensaData = string.Empty;
            MensaQuantidade = string.Empty;
            MensaComprimento = string.Empty;
            MensaLargura = string.Empty;
            MensagemErroGeral = string.Empty;

            try
            {
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

                if (string.IsNullOrWhiteSpace(Data.ToString()))
                {
                    MensaData = "A Data é obrigatória!";
                    possuiErro = true;
                }

                if (Quantidade <= 0)
                {
                    MensaQuantidade = "A Quantidade é obrigatória!";
                    possuiErro = true;
                }

                if (Largura <= 0)
                {
                    MensaLargura= "A Largura é obrigatória!";
                    possuiErro = true;
                }

                if (Comprimento <= 0)
                {
                    MensaComprimento = "O Comprimento é obrigatório!";
                    possuiErro = true;
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                string base64Payload = string.Empty;

                if (!string.IsNullOrEmpty(_caminhoArquivoSelecionado) && File.Exists(_caminhoArquivoSelecionado))
                {
                    // Leitura assíncrona por streams evita alocar buffers síncronos imensos no heap comum
                    byte[] fileBytes = await File.ReadAllBytesAsync(_caminhoArquivoSelecionado, cts.Token).ConfigureAwait(false);
                    base64Payload = Convert.ToBase64String(fileBytes);
                }

                /// Cria o objeto Residuo com os dados do formulário
                var novoResiduo = new Residuo
                {
                    Id = Guid.NewGuid().ToString(),

                    /// O Id do usuário logado deve ser obtido a partir do contexto de autenticação da aplicação,
                    IdUsuario = "Id_Do_Usuario_Logado",

                    TipoResiduo = TipoMaterial,
                    Especificacao = Especificacao, 
                    Origem = Origem,
                    Projeto = ProjetoOrigem,
                    Quantidade = Quantidade,
                    DataCadastro = Data,
                    Condicao = Condicao,
                    DimensoesCm = Comprimento,
                    DimensoesLm = Largura,
                    Observacao = Observacoes,
                    Anexo = base64Payload,
                    Status = "Disponível"
                };

                try
                {
                    /// Inicializa o cliente do Realtime Database
                    var firebase = new FirebaseClient("https://regraphikfirebase-default-rtdb.firebaseio.com/");

                    /// Salva o objeto na tabela/nó "residuos" usando o Id como chave
                    await firebase
                        .Child("residuos")
                        .Child(novoResiduo.Id)
                        .PutAsync(novoResiduo);

                    MostrarMensagemSucesso();
                }
                catch (Exception ex)
                {
                    MensagemErroGeral = "Erro ao salvar os dados no Realtime Database.";
                    MostrarMensagemErro();
                }
            }
            catch (Exception ex)
            {
                MostrarMensagemErro();
            }
        }

        private void MostrarMensagemSucesso()
        {
            var tela = new MensagemWindow();

            tela.TxtIcone.Text = "✓";
            tela.TxtTitulo.Text = "SUCESSO!";
            tela.TxtMensagem.Text = "Resíduo cadastrado com sucesso!";

            tela.ShowDialog();

            LimparCampos();
        }

        private void MostrarMensagemErro()
        {
            var tela = new MensagemWindow();
            
            tela.TxtIcone.Text = "X";
            tela.TxtTitulo.Text = "ERRO!";
            tela.TxtMensagem.Text = "Ocorreu um erro ao cadastrar o resíduo.";

            tela.ShowDialog();
        }


        /// <summary>
        /// Método para abrir um diálogo de seleção de arquivos, permitindo que o usuário escolha um arquivo de imagem ou vídeo para anexar ao resíduo.
        /// </summary>
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

                // Reduzido para 2MB para evitar estouros de string JSON HTTP clássicos
                if (fileInfo.Length > 2 * 1024 * 1024)
                {
                    MessageBox.Show("Arquivos maiores que 2MB não são suportados para envio direto via JSON.", "Arquivo muito grande", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                NomeArquivo = openFileDialog.SafeFileName;
                _caminhoArquivoSelecionado = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Método para limpar os campos do formulário após o cadastro de um resíduo ou quando o usuário desejar reiniciar o preenchimento.
        /// </summary>
        private void LimparCampos()
        {
            TipoMaterial = null;
            Especificacao = string.Empty;
            Origem = null;
            ProjetoOrigem = string.Empty;
            Quantidade = 0;
            Data = DateTime.Now;
            Condicao = null;
            Comprimento = 0;
            Largura = 0;
            Observacoes = string.Empty;
            NomeArquivo = "Nenhum arquivo selecionado";
            _caminhoArquivoSelecionado = string.Empty;
        }

    }
}

