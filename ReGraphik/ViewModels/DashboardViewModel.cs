using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace ReGraphik.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IResiduoService _residuoService = new Services.ResiduoService();

        private string _nomeUsuario;
        public string NomeUsuario
        {
            get => _nomeUsuario;
            set { _nomeUsuario = value; OnPropertyChanged(); }
        }

        // ← ADICIONE
        public string? FotoPerfil => UsuarioSessaoService.Instancia.FotoCaminho;

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

        private PlotModel _graficoPizzaModel;
        public PlotModel GraficoPizzaModel
        {
            get => _graficoPizzaModel;
            set { _graficoPizzaModel = value; OnPropertyChanged(); }
        }

        private PlotModel _graficoBarrasModel;
        public PlotModel GraficoBarrasModel
        {
            get => _graficoBarrasModel;
            set { _graficoBarrasModel = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Residuo> _ultimosResiduos;
        public ObservableCollection<Residuo> UltimosResiduos
        {
            get => _ultimosResiduos;
            set { _ultimosResiduos = value; OnPropertyChanged(); }
        }

        public Func<double, string> FormatterTipos { get; set; } = value => value.ToString("N2") + " kg";

        public ICommand AtualizarDadosCommand { get; set; }

        public DashboardViewModel(string nomeUsuario)
        {
            GraficoPizzaModel = new PlotModel();
            GraficoBarrasModel = new PlotModel();
            NomeUsuario = nomeUsuario;

            AtualizarDadosCommand = new RelayCommand(async () => await CarregarDadosDaApiAsync());

            
            UsuarioSessaoService.Instancia.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UsuarioSessaoService.FotoCaminho))
                    OnPropertyChanged(nameof(FotoPerfil));
            };

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(
                new System.Windows.DependencyObject()))
            {
                _ = CarregarDadosDaApiAsync();
            }
        }

        private async Task CarregarDadosDaApiAsync()
        {
            try
            {
                List<Residuo> todosResiduos = await _residuoService.ObterTodosResiduosAsync();

                if (todosResiduos != null && todosResiduos.Any())
                {
                    int totalResiduos = todosResiduos.Count;
                    int totalReaproveitar = todosResiduos.Count(r => r.Status == "Reaproveitado");
                    int totalEstoque = todosResiduos.Count(r => r.Status == "Em Estoque");

                    double valorCalculado = todosResiduos.Sum(r => r.Quantidade * 5.50);
                    string total = valorCalculado.ToString("C2");

                    var ultimoscinco = todosResiduos.OrderByDescending(r => r.DataCadastro).Take(5).ToList();

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

                    var pizzaModel = new PlotModel
                    {
                        Background = OxyColors.Transparent,
                        PlotAreaBorderThickness = new OxyThickness(0),
                        Padding = new OxyThickness(30)
                    };

                    var pieSeries = new PieSeries
                    {
                        StrokeThickness = 2,
                        Stroke = OxyColors.White,
                        InsideLabelPosition = 0.6,
                        InsideLabelFormat = "{2:0.0}%",
                        InsideLabelColor = OxyColors.White,
                        OutsideLabelFormat = "{1}",
                        TickDistance = 12,
                        TickRadialLength = 6,
                        Font = "Segoe UI",
                        FontSize = 11
                    };

                    var coresPaleta = new List<OxyColor>();

                    foreach (var grupo in todosResiduos.GroupBy(r => r.Status))
                    {
                        string statusLabel = string.IsNullOrWhiteSpace(grupo.Key) ? "Não Informado" : grupo.Key;

                        OxyColor corFatia = statusLabel switch
                        {
                            "Aguardando CADRI" => OxyColor.Parse("#0d2a56"),
                            "Aguardando Triagem" => OxyColor.Parse("#1649a2"),
                            "Disponível" => OxyColor.Parse("#64748B"),
                            "Disponível para Coleta" => OxyColor.Parse("#3274ba"),
                            "Liberado para Venda" => OxyColor.Parse("#2f80ec"),
                            _ => OxyColor.Parse("#cbd5e1")
                        };

                        pieSeries.Slices.Add(new PieSlice(statusLabel, grupo.Count()));
                        coresPaleta.Add(corFatia);
                    }

                    pizzaModel.DefaultColors = coresPaleta;
                    pizzaModel.Series.Add(pieSeries);

                    var barrasModel = new PlotModel
                    {
                        Background = OxyColors.Transparent,
                        PlotAreaBorderThickness = new OxyThickness(0)
                    };

                    var tiposAgrupados = todosResiduos.GroupBy(r => r.TipoResiduo)
                        .Select(g => new { Tipo = g.Key, PesoTotal = g.Sum(r => r.Quantidade) })
                        .OrderBy(x => x.PesoTotal)
                        .ToList();

                    var barSeries = new BarSeries
                    {
                        FillColor = OxyColor.FromRgb(22, 73, 162),
                        TextColor = OxyColor.FromRgb(30, 41, 59),
                        Font = "Segoe UI",
                        FontSize = 11,
                        LabelFormatString = "{0:N0} kg",
                        LabelPlacement = LabelPlacement.Outside,
                        BarWidth = 0.6
                    };

                    foreach (var item in tiposAgrupados)
                        barSeries.Items.Add(new BarItem((double)item.PesoTotal));

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
                        categoryAxis.Labels.Add(string.IsNullOrWhiteSpace(item.Tipo) ? "Outros" : item.Tipo);

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

                    barrasModel.Axes.Add(categoryAxis);
                    barrasModel.Axes.Add(valueAxis);
                    barrasModel.Series.Add(barSeries);

                    pizzaModel.InvalidatePlot(true);
                    barrasModel.InvalidatePlot(true);

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