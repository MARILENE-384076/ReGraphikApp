using ReGraphik.Models;
using ReGraphik.Services.Interface;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

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
        /// Propriedade para armazenar os dados do gráfico de pizza, que será atualizado com 
        /// as informações obtidas da API e exibido na interface da Dashboard.
        /// </summary>
        private PlotModel _graficoPizzaModel;
        public PlotModel GraficoPizzaModel
        {
            get => _graficoPizzaModel;
            set
            {
                _graficoPizzaModel = value;
                OnPropertyChanged();
            }
        }

        private PlotModel _graficoBarrasModel;
        public PlotModel GraficoBarrasModel
        {
            get => _graficoBarrasModel;
            set
            {
                _graficoBarrasModel = value;
                OnPropertyChanged();
            }
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
            GraficoPizzaModel = new PlotModel();
            GraficoBarrasModel = new PlotModel();

            NomeUsuario = nomeUsuario;

            AtualizarDadosCommand =
                new RelayCommand(async () => await CarregarDadosDaApiAsync());

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(
                new System.Windows.DependencyObject()))
            {
                _ = CarregarDadosDaApiAsync();
            }
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

                    // Cria uma série para cada grupo de status, definindo a cor de cada fatia com base no status, igual aos seus Cards/Badges
                    var pizzaModel = new PlotModel();

                    // Configura o estilo do gráfico de pizza para uma aparência mais moderna e elegante
                    var pieSeries = new PieSeries
                    {
                        StrokeThickness = 2,
                        InsideLabelPosition = 0.8,
                        AngleSpan = 360,
                        StartAngle = 0,
                        InsideLabelFormat = "{1}: {0}"
                    };

                    // Define as cores para cada status, usando uma paleta de cores consistente com os Cards/Badges da aplicação
                    foreach (var grupo in todosResiduos.GroupBy(r => r.Status))
                    {
                        pieSeries.Slices.Add(
                            new PieSlice(
                                string.IsNullOrWhiteSpace(grupo.Key)
                                    ? "Não Informado"
                                    : grupo.Key,
                                grupo.Count()
                            )
                        );
                    }

                    // Configura as cores das fatias do gráfico de pizza para corresponder aos status
                    // dos resíduos, usando uma paleta de cores consistente com os Cards/Badges da aplicação
                    pizzaModel.Series.Add(pieSeries);

                    // Agrupa os resíduos por tipo para criar o gráfico de barras horizontais
                    var tiposAgrupados = todosResiduos.GroupBy(r => r.TipoResiduo)
                                  .Select(g => new { Tipo = g.Key, PesoTotal = g.Sum(r => r.Quantidade) })
                                  .ToList();

                    // Cria uma série para o gráfico de linhas, usando os dados de quantidade por mês (exemplo fictício, ajuste conforme necessário)
                    var barrasModel = new PlotModel();

                    var barSeries = new BarSeries();

                    foreach (var item in tiposAgrupados)
                    {
                        barSeries.Items.Add(
                            new BarItem((double)item.PesoTotal)
                        );
                    }

                    barrasModel.Series.Add(barSeries);

                    var categoryAxis = new CategoryAxis
                    {
                        Position = AxisPosition.Bottom
                    };

                    foreach (var item in tiposAgrupados)
                    {
                        categoryAxis.Labels.Add(
                            string.IsNullOrWhiteSpace(item.Tipo)
                                ? "Outros"
                                : item.Tipo
                        );
                    }

                    barrasModel.Axes.Add(categoryAxis);

                    barrasModel.Axes.Add(
                        new LinearAxis
                        {
                            Position = AxisPosition.Left,
                            MinimumPadding = 0,
                            AbsoluteMinimum = 0
                        });

                    pizzaModel.InvalidatePlot(true);
                    barrasModel.InvalidatePlot(true);

                    // Atualiza as propriedades da ViewModel na thread da UI para garantir que os gráficos e os cards sejam atualizados corretamente
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ResiduosDb = totalResiduos;
                        Reaproveitar = totalReaproveitar;
                        Estoque = totalEstoque;
                        Total = total;
                        UltimosResiduos = ultimosResiduos;
                        GraficoPizzaModel = pizzaModel;
                        GraficoBarrasModel = barrasModel;

                        // Notifica a UI sobre as mudanças nas propriedades para atualizar os gráficos e os cards
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