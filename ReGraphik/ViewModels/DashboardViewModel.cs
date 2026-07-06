using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel responsável por gerenciar os dados e a lógica do Dashboard da aplicação. Ele interage com a interface de 
    /// usuário para exibir informações sobre resíduos, gráficos de status e contadores, além de fornecer 
    /// comandos para atualizar os dados a partir da API.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        /// <summary>
        /// Referência para o serviço de resíduos, que é responsável por obter os dados da API relacionados aos resíduos.
        /// </summary>
        private readonly IResiduoService _residuoService = new Services.ResiduoService();

        private string _nomeUsuario;
        public string NomeUsuario
        {
            get => _nomeUsuario;
            set { _nomeUsuario = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Obtém a imagem de perfil do usuário logado a partir do caminho armazenado no serviço de sessão,
        /// lendo em memória para não trancar arquivos locais do disco.
        /// </summary>
        /// <summary>
        /// Obtém o caminho ou URL da imagem de perfil do usuário logado.
        /// </summary>
        public string? FotoPerfil
        {
            get
            {
                string? caminho = UsuarioSessaoService.Instancia.FotoCaminho;

                if (string.IsNullOrWhiteSpace(caminho))
                    return null;

                /// Se for um arquivo local, verifica se ele realmente existe antes de mandar pro XAML
                if (!caminho.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !File.Exists(caminho))
                    return null;

                return caminho;
            }
        }

        private string _perfil;
        public string Perfil
        {
            get => _perfil;
            set { _perfil = value; OnPropertyChanged(); }
        }


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

        /// <summary>
        /// Função de formatação para os valores do eixo Y do gráfico de barras, garantindo
        /// que os números sejam exibidos com duas casas decimais e a unidade "kg".
        /// </summary>
        public Func<double, string> FormatterTipos { get; set; } = value => value.ToString("N2") + " kg";

        /// <summary>
        /// Comando que aciona a atualização dos dados do dashboard, incluindo gráficos e contadores, a partir da API de resíduos.
        /// </summary>
        public ICommand AtualizarDadosCommand { get; set; }

        /// <summary>
        /// Construtor do ViewModel do Dashboard, responsável por inicializar os gráficos, carregar os dados 
        /// do usuário logado e configurar o comando de atualização de dados.
        /// </summary>
        /// <param name="usuarioLogado"></param>
        public DashboardViewModel(Usuario usuarioLogado)
        {
            /// Inicialização única e fixa dos Models de Gráfico
            GraficoPizzaModel = new PlotModel { Background = OxyColors.Transparent, PlotAreaBorderThickness = new OxyThickness(0), Padding = new OxyThickness(30) };
            GraficoBarrasModel = new PlotModel { Background = OxyColors.Transparent, PlotAreaBorderThickness = new OxyThickness(0) };

            /// Extrai os dados diretamente do objeto de Usuário vindo da API/Sessão
            NomeUsuario = usuarioLogado.Nome;

            /// Tratamento visual para o perfil (ex: se vier "Admin" vira "Administrador")
            Perfil = usuarioLogado.Perfil == "Admin" || usuarioLogado.Perfil == "Administrador"
                ? "Administrador"
                : "Usuário";

            AtualizarDadosCommand = new RelayCommand(async () => await CarregarDadosDaApiAsync());

            /// Vinculando evento de sessão de forma segura
            UsuarioSessaoService.Instancia.PropertyChanged += OnUsuarioSessaoPropertyChanged;

            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(
                new System.Windows.DependencyObject()))
            {
                _ = CarregarDadosDaApiAsync();
            }
        }

        private void OnUsuarioSessaoPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UsuarioSessaoService.FotoCaminho))
                OnPropertyChanged(nameof(FotoPerfil));
        }

        /// <summary>
        /// Carrega os dados da API de resíduos, atualizando as propriedades vinculadas à tela e os gráficos.
        /// </summary>
        /// <returns></returns>
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
                        Especificacao = residuo.Especificacao,
                        Projeto = residuo.Projeto,
                        Condicao = residuo.Condicao,
                        DimensoesCm = residuo.DimensoesCm,
                        DimensoesLm = residuo.DimensoesLm,
                        Observacao = residuo.Observacao,
                        Anexo = residuo.Anexo
                    }).ToList();


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

                    /// Lógica de agrupamento e contagem de resíduos por status, garantindo 
                    /// que cada status tenha uma cor distinta no gráfico de pizza.
                    foreach (var grupo in todosResiduos.GroupBy(r => r.Status))
                    {
                        string statusLabel = string.IsNullOrWhiteSpace(grupo.Key) ? "Não Informado" : grupo.Key;

                        OxyColor corFatia = statusLabel switch
                        {
                            "Disponível" => OxyColor.FromRgb(100, 116, 139),     // #64748B
                            "Reservado" => OxyColor.FromRgb(50, 116, 186),       // #3274ba
                            "Em Análise" => OxyColor.FromRgb(22, 73, 162),       // #1649a2
                            "Coletado" => OxyColor.FromRgb(13, 42, 86),          // #0d2a56
                            "Indisponível" => OxyColor.FromRgb(71, 85, 105),     // #475569
                            _ => OxyColor.FromRgb(203, 213, 225)                 // #CBD5E1
                        };
                        pieSeries.Slices.Add(new PieSlice(statusLabel, grupo.Count()));
                        coresPaleta.Add(corFatia);
                    }

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

                    /// Adiciona os valores de peso total para cada tipo de resíduo na série de barras
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

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        /// Atualiza as propriedades vinculadas à tela
                        ResiduosDb = totalResiduos;
                        Reaproveitar = totalReaproveitar;
                        Estoque = totalEstoque;
                        Total = total;
                        UltimosResiduos = new ObservableCollection<Residuo>(ultimosComIdNumerico);

                        /// Limpa e atualiza os modelos de gráficos existentes (Garante animação e estabilidade de renderização)
                        GraficoPizzaModel.Series.Clear();
                        GraficoPizzaModel.DefaultColors = coresPaleta;
                        GraficoPizzaModel.Series.Add(pieSeries);

                        /// Atualiza Gráfico de Barras limpando eixos antigos
                        GraficoBarrasModel.Series.Clear();
                        GraficoBarrasModel.Axes.Clear();
                        GraficoBarrasModel.Axes.Add(categoryAxis);
                        GraficoBarrasModel.Axes.Add(valueAxis);
                        GraficoBarrasModel.Series.Add(barSeries);

                        /// Força a atualização visual na tela
                        GraficoPizzaModel.InvalidatePlot(true);
                        GraficoBarrasModel.InvalidatePlot(true);
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MensagemWindow.Exibir("Aviso", $"A API conectou, mas a lista de resíduos veio vazia.", MensagemWindow.TipoMensagem.Aviso);
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro de Código", $"Falha de conexão com a API", MensagemWindow.TipoMensagem.Erro);
                });
            }

        }

        /// <summary>
        /// Implementação do padrão Dispose para desvincular do Singleton global
        /// </summary>
        public void Dispose()
        {
            UsuarioSessaoService.Instancia.PropertyChanged -= OnUsuarioSessaoPropertyChanged;
        }
    }
}