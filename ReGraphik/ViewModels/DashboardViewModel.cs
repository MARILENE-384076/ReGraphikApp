using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReGraphik.Models;
using ReGraphik.Services.Interface;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;
using DrawMargin = LiveChartsCore.Measure.Margin;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel responsável por gerenciar a lógica e os dados exibidos na Dashboard da aplicação. Ele interage com o serviço de resíduos 
    /// para obter as informações necessárias, processa os dados para exibição nos cards, tabelas e gráficos, e expõe comandos para atualizar os dados da interface.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        // Instância do serviço de resíduos para lidar com a lógica relacionada aos resíduos
        private readonly IResiduoService _residuoService = new Services.ResiduoService();

        public DrawMargin MargemDoGrafico { get; set; }

        /// <summary>
        /// Propriedade para armazenar o nome do usuário logado, que será exibido na interface da Dashboard.
        /// </summary>
        private string _nomeUsuario;
        public string NomeUsuario
        {
            get => _nomeUsuario;
            set { _nomeUsuario = value; OnPropertyChanged(); }
        }

        // Propriedades para os Cards
        private int _residuosDb;
        public int ResiduosDb
        {
            get => _residuosDb;
            set { _residuosDb = value; OnPropertyChanged(); }
        }

        private int _reaproveitar;
        public int Reaproveitar
        {
            get => _reaproveitar;
            set { _reaproveitar = value; OnPropertyChanged(); }
        }

        private int _estoque;
        public int Estoque
        {
            get => _estoque;
            set { _estoque = value; OnPropertyChanged(); }
        }

        private string _total;
        public string Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Propriedade para armazenar os resíduos mais recentes, que serão exibidos na tabela da Dashboard.
        /// </summary>
        private ObservableCollection<Residuo> _ultimosResiduos;
        public ObservableCollection<Residuo> UltimosResiduos
        {
            get => _ultimosResiduos;
            set { _ultimosResiduos = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Propriedade para armazenar as séries de dados do gráfico de status, que serão exibidos na Dashboard. 
        /// </summary>
        private IEnumerable<ISeries> _graficoStatus;
        public IEnumerable<ISeries> GraficoStatus
        {
            get => _graficoStatus;
            set { _graficoStatus = value; OnPropertyChanged(); }
        }

        private IEnumerable<ISeries> _graficoTipos;
        public IEnumerable<ISeries> GraficoTipos
        {
            get => _graficoTipos;
            set { _graficoTipos = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Propriedades para armazenar as configurações dos eixos X e Y do gráfico de tipos, que serão exibidos na Dashboard.
        /// </summary>
        private Axis[] _eixosXTipos;
        public Axis[] EixosXTipos
        {
            get => _eixosXTipos;
            set { _eixosXTipos = value; OnPropertyChanged(); }
        }

        private Axis[] _eixosYTipos;
        public Axis[] EixosYTipos
        {
            get => _eixosYTipos;
            set { _eixosYTipos = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Propriedade para formatar os valores exibidos no gráfico de tipos, convertendo os valores numéricos em uma string formatada com "kg" para indicar o peso total dos resíduos por tipo.
        /// </summary>
        public Func<double, string> FormatterTipos { get; set; } = value => value.ToString("N2") + " kg";

        /// <summary>
        /// Comando para atualizar os dados da Dashboard, que será vinculado a um botão na interface. 
        /// </summary>
        public ICommand AtualizarDadosCommand { get; set; }

        /// <summary>
        /// Construtor do DashboardViewModel, que recebe o nome do usuário logado como parâmetro. Ele inicializa as propriedades dos gráficos, 
        /// define o comando de atualização de dados e carrega os dados da API assim que a ViewModel for criada para exibir as informações imediatamente na Dashboard.
        /// </summary>
        /// <param name="nomeUsuario">Nome do usuário logado, que será exibido na Dashboard.</param>
        public DashboardViewModel(string nomeUsuario)
        {
            MargemDoGrafico = new DrawMargin(45, 15, 15, 60);

            // Define um nome de usuário padrão para exibição
            NomeUsuario = nomeUsuario;

            // Instancia o comando vinculando-o ao método assíncrono
            AtualizarDadosCommand = new RelayCommand(async () => await CarregarDadosDaApiAsync());

            // Carrega os dados da API assim que a ViewModel for criada, para exibir as informações imediatamente
            _ = CarregarDadosDaApiAsync();
        }

        /// <summary>
        /// Método assíncrono responsável por carregar os dados da API, processar as informações e atualizar as propriedades da ViewModel para refletir os dados na interface da Dashboard.
        /// </summary>
        /// <returns></returns>
        private async Task CarregarDadosDaApiAsync()
        {
            try
            {
                // Chama o serviço para obter todos os resíduos do banco de dados
                List<Residuo> todosResiduos = await _residuoService.ObterTodosResiduosAsync();

                // Verifica se a lista retornada não é nula e contém elementos antes de tentar acessar suas propriedades
                if (todosResiduos != null && todosResiduos.Any())
                {

                    // Atualiza as propriedades dos Cards
                    int totalResiduos = todosResiduos.Count;
                    int totalReaproveitar = todosResiduos.Count(r => r.Status == "Reaproveitado");
                    int totalEstoque = todosResiduos.Count(r => r.Status == "Em Estoque");

                    double valorCalculado = todosResiduos.Sum(r => r.Quantidade * 5.50);
                    string total = valorCalculado.ToString("C2");

                    // Atualiza a Tabela com os 5 resíduos mais recentes
                    var ultimoscinco = todosResiduos.OrderByDescending(r => r.DataCadastro).Take(5).ToList();

                    // Atribui IDs numéricos sequenciais para os resíduos mais recentes, começando de 1
                    var ultimosComIdNumerico = ultimoscinco.Select((residuo, index) => new Residuo
                    {
                        Id = (index + 1).ToString(), 
                        TipoResiduo = residuo.TipoResiduo,
                        Origem = residuo.Origem,
                        Quantidade = residuo.Quantidade,
                        DataCadastro = residuo.DataCadastro,
                        Status = residuo.Status,
                        IdUsuario = residuo.IdUsuario,
                        Especificacao = residuo.Especificacao,
                        Projeto = residuo.Projeto,
                        Condicao = residuo.Condicao,
                        DimensoesCm = residuo.DimensoesCm,
                        DimensoesLm = residuo.DimensoesLm,
                        Observacao = residuo.Observacao,
                        Anexo = residuo.Anexo
                    });

                    var ultimosResiduos = new ObservableCollection<Residuo>(ultimosComIdNumerico);

                    // Agrupa os resíduos por status para criar o gráfico de pizza
                    var statusAgrupado = todosResiduos.GroupBy(r => r.Status);

                    // Cria uma série para cada grupo de status, definindo a cor de cada fatia com base no status, igual aos seus Cards/Badges
                    var tempGraficoStatus = new List<ISeries>();
                    foreach (var grupo in statusAgrupado)
                    {
                        // Define a cor da fatia com base no status, igual aos seus Cards/Badges
                        SKColor corDaFatia;
                        switch (grupo.Key)
                        {
                            case "Aguardando CADRI":
                                corDaFatia = SKColor.Parse("#0d2a56"); 
                                break;
                            case "Aguardando Triagem":
                                corDaFatia = SKColor.Parse("#1649a2"); 
                                break;
                            case "Disponível para Coleta":
                                corDaFatia = SKColor.Parse("#3274ba"); 
                                break;
                            case "Liberado para Venda":
                                corDaFatia = SKColor.Parse("#2f80ec"); 
                                break;
                            default:
                                corDaFatia = SKColor.Parse("#64748B"); 
                                break;
                        }

                        tempGraficoStatus.Add(new PieSeries<int>
                        {
                            Name = string.IsNullOrEmpty(grupo.Key) ? "Não Informado" : grupo.Key,
                            Values = new int[] { grupo.Count() },

                            // Define a cor da fatia com base no status, igual aos seus Cards/Badges
                            Fill = new SolidColorPaint(corDaFatia),
                            
                            // Customização visual elegante
                            InnerRadius = 30,
                            MaxRadialColumnWidth = 20,
                            OuterRadiusOffset = 0,

                            // Formatação dos rótulos de dados para exibir o status e a quantidade, com formatação elegante
                            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                            DataLabelsPaint = null
                        });
                    }

                    // Agrupa os resíduos por tipo para criar o gráfico de barras horizontais
                    var tiposAgrupados = todosResiduos.GroupBy(r => r.TipoResiduo)
                                  .Select(g => new { Tipo = g.Key, PesoTotal = g.Sum(r => r.Quantidade) })
                                  .ToList();

                    // Cria uma série para o gráfico de linhas, usando os dados de quantidade por mês (exemplo fictício, ajuste conforme necessário)
                    var tempGraficoColunas = new ISeries[]
                    {
                        new ColumnSeries<double>
                        {
                            Name = "Peso Total",
                            Values = tiposAgrupados.Select(t => t.PesoTotal).ToArray(),

                            MaxBarWidth = 32, 

                            // Customização visual elegante
                            Fill = new SolidColorPaint(new SKColor(30, 58, 138)), 
       
                            // Formatação dos rótulos de dados para exibir o peso total em kg, com formatação elegante
                            DataLabelsFormatter = point => point.Coordinate.PrimaryValue >= 1000000
                                ? $"{(point.Coordinate.PrimaryValue / 1000000):N2}M"
                                : $"{point.Coordinate.PrimaryValue:N0}",

                            DataLabelsSize = 11,
                            DataLabelsPaint = new SolidColorPaint(new SKColor(30, 41, 59)),
                            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
                        }
                    };

                    // Configura as Labels do Eixo X na v2
                    var tempEixosXColunas = new Axis[]
                    {
                        new Axis
                        {
                            Labels = tiposAgrupados.Select(t => string.IsNullOrEmpty(t.Tipo) ? "Outros" : t.Tipo).ToArray(),
                            TextSize = 11,
                            LabelsPaint = new SolidColorPaint(new SKColor(100, 116, 139)),
                            
                        }
                    };

                    // Configura as Labels do Eixo Y na v2
                    var tempEixosYColunas = new Axis[]
                    {
                        new Axis
                        {
                            TextSize = 11,
                            LabelsPaint = new SolidColorPaint(new SKColor(148, 163, 184)),
                            // Linhas de grade horizontais discretas ao fundo
                            SeparatorsPaint = new SolidColorPaint(new SKColor(241, 245, 249)) { StrokeThickness = 1 }
                        }
                    };

                    // Envia os dados criados para a UI thread
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ResiduosDb = totalResiduos;
                        Reaproveitar = totalReaproveitar;
                        Estoque = totalEstoque;
                        Total = total;
                        UltimosResiduos = ultimosResiduos;
                        GraficoStatus = tempGraficoStatus;
                        GraficoTipos = tempGraficoColunas;
                        EixosXTipos = tempEixosXColunas;
                        EixosYTipos = tempEixosYColunas;
                    });

                }
                else
                {
                    System.Windows.MessageBox.Show("A API conectou, mas a lista de resíduos veio vazia.", "Aviso");
                }

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Falha de conexão: {ex.Message}", "Erro de Código");
            }
        }
    }
}