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
                    var pizzaModel = new PlotModel
                    {
                        Background = OxyColors.Transparent,
                        PlotAreaBorderThickness = new OxyThickness(0),
                        Padding = new OxyThickness(30)
                    };

                    // Configura o estilo do gráfico de pizza para uma aparência mais moderna e elegante
                    var pieSeries = new PieSeries
                    {
                        StrokeThickness = 2,
                        Stroke = OxyColors.White,

                        // Configura os rótulos de DENTRO (Porcentagem)
                        InsideLabelPosition = 0.6,
                        InsideLabelFormat = "{2:0.0}%", // Calcula a % automaticamente
                        InsideLabelColor = OxyColors.White,

                        // Configura os rótulos de FORA (Tópicos/Status)
                        OutsideLabelFormat = "{1}", // {1} é o nome do Status
                        TickDistance = 12,          // Linha que liga a fatia ao texto
                        TickRadialLength = 6,

                        Font = "Segoe UI",
                        FontSize = 11
                    };

                    var coresPaleta = new List<OxyColor>();

                    // Define as cores para cada status, usando uma paleta de cores consistente com os Cards/Badges da aplicação
                    foreach (var grupo in todosResiduos.GroupBy(r => r.Status))
                    {
                        string statusLabel = string.IsNullOrWhiteSpace(grupo.Key) ? "Não Informado" : grupo.Key;

                        // Mapeamento exato das cores fornecidas
                        OxyColor corFatia = statusLabel switch
                        {
                            "Aguardando CADRI" => OxyColor.Parse("#0d2a56"), // Azul Escuro
                            "Aguardando Triagem" => OxyColor.Parse("#1649a2"), // Azul Médio
                            "Disponível" => OxyColor.Parse("#64748B"), // Cinza
                            "Disponível para Coleta" => OxyColor.Parse("#3274ba"), // Azul Claro
                            "Liberado para Venda" => OxyColor.Parse("#2f80ec"), // Azul Muito Claro
                            _ => OxyColor.Parse("#cbd5e1")  // Fallback
                        };

                        // Adiciona a fatia associando seu rótulo externo
                        pieSeries.Slices.Add(new PieSlice(statusLabel, grupo.Count()));
                        coresPaleta.Add(corFatia);
                    }

                    pizzaModel.DefaultColors = coresPaleta;
                    pizzaModel.Series.Add(pieSeries);

                    // Cria o gráfico de barras horizontais para mostrar a quantidade total de resíduos por tipo,
                    // usando uma paleta de cores consistente com os Cards/Badges da aplicação
                    var barrasModel = new PlotModel
                    {
                        Background = OxyColors.Transparent,
                        PlotAreaBorderThickness = new OxyThickness(0) 
                    };

                    // Agrupa os resíduos por tipo para criar o gráfico de barras horizontais
                    var tiposAgrupados = todosResiduos.GroupBy(r => r.TipoResiduo)
                                                      .Select(g => new { Tipo = g.Key, PesoTotal = g.Sum(r => r.Quantidade) })
                                                      .OrderBy(x => x.PesoTotal) 
                                                      .ToList();

                    // Configura a série de barras, definindo a cor de preenchimento, a formatação dos rótulos e a posição
                    // dos rótulos para uma aparência mais moderna e elegante
                    var barSeries = new BarSeries
                    {
                        FillColor = OxyColor.FromRgb(22, 73, 162), // Azul ReGraphik
                        TextColor = OxyColor.FromRgb(30, 41, 59),
                        Font = "Segoe UI",
                        FontSize = 11,
                        LabelFormatString = "{0:N0} kg",
                        LabelPlacement = LabelPlacement.Outside,
                        BarWidth = 0.6
                    };

                    foreach (var item in tiposAgrupados)
                    {
                        barSeries.Items.Add(
                            new BarItem((double)item.PesoTotal)
                        );
                    }

                    var categoryAxis = new CategoryAxis
                    {
                        Position = AxisPosition.Left,
                        AxislineStyle = LineStyle.None,
                        TickStyle = TickStyle.None,
                        Font = "Segoe UI",
                        FontSize = 12,
                        TextColor = OxyColor.FromRgb(71, 85, 105),

                        IsZoomEnabled = false,
                        IsPanEnabled = false
                    };

                    foreach (var item in tiposAgrupados)
                    {
                        categoryAxis.Labels.Add(string.IsNullOrWhiteSpace(item.Tipo) ? "Outros" : item.Tipo);
                    }

                    // Configura o eixo de valores para o gráfico de barras, definindo a formatação dos rótulos,
                    // a cor das linhas de grade e a aparência geral para uma estética mais moderna e elegante
                    var valueAxis = new LinearAxis
                    {
                        Position = AxisPosition.Bottom,
                        MinimumPadding = 0,
                        MaximumPadding = 0.15,
                        AbsoluteMinimum = 0,
                        MajorGridlineStyle = LineStyle.Solid,
                        MajorGridlineColor = OxyColor.FromRgb(226, 232, 240), 
                        TickStyle = TickStyle.None,
                        Font = "Segoe UI",
                        TextColor = OxyColor.FromRgb(148, 163, 184),

                        IsZoomEnabled = false,
                        IsPanEnabled = false
                    };

                    // Adiciona os eixos e a série ao modelo do gráfico de barras
                    barrasModel.Axes.Add(categoryAxis);
                    barrasModel.Axes.Add(valueAxis);
                    barrasModel.Series.Add(barSeries);

                    // Invalida os gráficos para garantir que eles sejam redesenhados com os novos dados
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