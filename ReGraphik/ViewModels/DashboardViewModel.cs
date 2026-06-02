using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ReGraphik.Models;
using ReGraphik.Services.Interface;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DrawMargin = LiveChartsCore.Measure.Margin;

namespace ReGraphik.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        // Instância do serviço de resíduos para lidar com a lógica relacionada aos resíduos
        private readonly IResiduoService _residuoService = new Services.ResiduoService();

        public DrawMargin MargemDoGrafico { get; set; }

        // Propriedade para o nome do usuário, que pode ser exibida na interface
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

        // Propriedade para a Tabela 
        private ObservableCollection<Residuo> _ultimosResiduos;
        public ObservableCollection<Residuo> UltimosResiduos
        {
            get => _ultimosResiduos;
            set { _ultimosResiduos = value; OnPropertyChanged(); }
        }

        // Propriedades para os Gráficos
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

        // Propriedades para os rótulos dos gráficos
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

        public Func<double, string> FormatterTipos { get; set; } = value => value.ToString("N2") + " kg";

        // Comando vinculado ao botão "Verificar" do XAML
        public ICommand AtualizarDadosCommand { get; set; }

        // Construtor
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

        // Método para buscar da API e fazer cálculos
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
                    var ultimos = todosResiduos.OrderByDescending(r => r.DataCadastro).Take(5);
                    var ultimosResiduos = new ObservableCollection<Residuo>(ultimos);

                    // Agrupa os resíduos por status para criar o gráfico de pizza
                    var statusAgrupado = todosResiduos.GroupBy(r => r.Status);

                    // Cria uma lista de séries para o gráfico de pizza, usando os grupos de status
                    var tempGraficoStatus = new List<ISeries>();
                    foreach (var grupo in statusAgrupado)
                    {
                        tempGraficoStatus.Add(new PieSeries<int>
                        {
                            Name = string.IsNullOrEmpty(grupo.Key) ? "Não Informado" : grupo.Key,
                            Values = new int[] { grupo.Count() },

                            // Customização visual elegante
                            InnerRadius = 30,             // Aumenta o furo interno (deixa a rosca fina como na imagem)
                            MaxRadialColumnWidth = 20,    // Deixa a espessura do anel bem compacta
                            OuterRadiusOffset = 0,

                            // Esconde os números de dentro das fatias, igual ao modelo
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
        
                            // Customização baseada no modelo azul corporativo elegante
                            MaxBarWidth = 32, // Colunas encorpadas igual à imagem
                            Fill = new SolidColorPaint(new SKColor(43, 108, 176)), // Tom de azul fiel ao modelo (#2B6CB0)
        
                            // Define o texto do valor diretamente no topo de cada coluna
                            DataLabelsFormatter = point => point.Coordinate.PrimaryValue >= 1000000
                                ? $"{(point.Coordinate.PrimaryValue / 1000000):N2}M"
                                : $"{point.Coordinate.PrimaryValue:N0}", // Formata números limpos (Ex: 47M ou 850)
        
                            DataLabelsSize = 11,
                            DataLabelsPaint = new SolidColorPaint(new SKColor(30, 41, 59)), // Cor escura para leitura limpa
                            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top // Força o texto em cima da coluna
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