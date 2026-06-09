using MahApps.Metro.SimpleChildWindow;
using MahApps.Metro.SimpleChildWindow;
using ReGraphik.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

        private string _quantidade;
        public string Quantidade
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

        private string _comprimento;
        public string Comprimento
        {
            get => _comprimento;
            set { _comprimento = value; OnPropertyChanged(); }
        }

        private string _largura;
        public string Largura
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

        /// <summary>
        /// Construtor da classe ResiduoViewModel, onde o HttpClient é configurado com a URL base da API e os comandos são inicializados.
        /// </summary>
        public ResiduoViewModel()
        {
            // Configura o HttpClient com a URL base da API
            _httpClient = new HttpClient { BaseAddress = new Uri("https://webregraphik.runasp.net/") };

            // Inicializa os comandos com as ações correspondentes
            SalvarResiduoCommand = new RelayCommand(async () => await SalvarResiduoAsync());
            SelecionarArquivoCommand = new RelayCommand(SelecionarArquivo);
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

                if (string.IsNullOrWhiteSpace(Quantidade))
                {
                    MensaQuantidade = "A Quantidade é obrigatória!";
                    possuiErro = true;
                }

                if (string.IsNullOrWhiteSpace(Largura))
                {
                    MensaLargura= "A Largura é obrigatória!";
                    possuiErro = true;
                }

                if (string.IsNullOrWhiteSpace(Comprimento))
                {
                    MensaComprimento = "O Comprimento é obrigatório!";
                    possuiErro = true;
                }

                if (string.IsNullOrWhiteSpace(Origem) || string.IsNullOrWhiteSpace(Especificacao) ||
                    string.IsNullOrWhiteSpace(ProjetoOrigem) || string.IsNullOrWhiteSpace(Observacoes))
                {
                    MensagemErroGeral = "Preencha todos os campos.";
                    return;
                }


                // Cria o objeto Residuo com os dados do formulário
                var novoResiduo = new Residuo
                {
                    Id = Guid.NewGuid().ToString(), // Gera um ID único para o resíduo
                    TipoResiduo = TipoMaterial,
                    Especificacao = Especificacao,
                    Origem = Origem,
                    Projeto = ProjetoOrigem,
                    Quantidade = double.TryParse(Quantidade, out var quant) ? quant : 0,
                    DataCadastro = Data,
                    Condicao = Condicao,
                    DimensoesCm = double.TryParse(Comprimento, out var comp) ? comp : 0,
                    DimensoesLm = double.TryParse(Largura, out var larg) ? larg : 0,
                    Observacao = Observacoes,
                    Anexo = NomeArquivo, // Aqui você pode implementar a lógica para salvar o arquivo e obter o caminho real
                    Status = "Disponível"
                };

                // Executa o POST na rota da API: api/Residuo
                var response = await _httpClient.PostAsJsonAsync("api/Residuo", novoResiduo);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Salvo com sucesso!!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    LimparCampos();
                }
                else
                {
                    var erroDetalhes = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Falha ao salvar: {response.StatusCode}\n{erroDetalhes}", "Erro API", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro de conexão: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SelecionarArquivo()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens e Vídeos|*.jpg;*.jpeg;*.png;*.bmp;*.mp4;*.mkv;*.avi;*.mov|" +
                 "Imagens (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|" +
                 "Vídeos (*.mp4, *.mkv, *.avi, *.mov)|*.mp4;*.mkv;*.avi;*.mov",
                Title = "Selecione um arquivo para anexar"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                NomeArquivo = openFileDialog.SafeFileName;
            }
        }

        private void LimparCampos()
        {
            TipoMaterial = null;
            Especificacao = string.Empty;
            Origem = null;
            ProjetoOrigem = string.Empty;
            Quantidade = string.Empty;
            Data = DateTime.Now;
            Condicao = null;
            Comprimento = string.Empty;
            Largura = string.Empty;
            Observacoes = string.Empty;
            NomeArquivo = "Nenhum arquivo selecionado";
        }

    }
}

