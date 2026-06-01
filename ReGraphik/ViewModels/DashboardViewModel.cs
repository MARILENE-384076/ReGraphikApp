using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input; // Necessário para o ICommand
using ReGraphik.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace ReGraphik.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient = new HttpClient();

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
        private SeriesCollection _graficoStatus;
        public SeriesCollection GraficoStatus
        {
            get => _graficoStatus;
            set { _graficoStatus = value; OnPropertyChanged(); }
        }

        private SeriesCollection _graficoTipos;
        public SeriesCollection GraficoTipos
        {
            get => _graficoTipos;
            set { _graficoTipos = value; OnPropertyChanged(); }
        }

        private string[] _labelsTipos;
        public string[] LabelsTipos
        {
            get => _labelsTipos;
            set { _labelsTipos = value; OnPropertyChanged(); }
        }

        public Func<double, string> FormatterTipos { get; set; } = value => value.ToString("N2") + " kg";

        // Comando vinculado ao botão "Verificar" do XAML
        public ICommand AtualizarDadosCommand { get; set; }

        // Construtor
        public DashboardViewModel()
        {
            // Configura a base address do HttpClient para facilitar as requisições
            _httpClient.BaseAddress = new Uri("https://webregraphik.runasp.net/api/usuario/");

            // Instancia o comando vinculando-o ao método assíncrono
            AtualizarDadosCommand = new RelayCommand(async () => await CarregarDadosDaApiAsync());

            // Carrega os dados na inicialização
            Task.Run(async () => await CarregarDadosDaApiAsync());
        }

        // Método para buscar da API e fazer cálculos
        private async Task CarregarDadosDaApiAsync()
        {
            try
            {
                // Faz a requisição GET para o endpoint de Residuos
                HttpResponseMessage response = await _httpClient.GetAsync("Residuo");

                if (response.IsSuccessStatusCode)
                {
                    string jsonResult = await response.Content.ReadAsStringAsync();

                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    List<Residuo> todosResiduos = JsonSerializer.Deserialize<List<Residuo>>(jsonResult, opcoes);

                    if (todosResiduos != null && todosResiduos.Any())
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            // 1. Atualizando os Cards básicos
                            ResiduosDb = todosResiduos.Count;
                            Reaproveitar = todosResiduos.Count(r => r.Status == "Reaproveitado");
                            Estoque = todosResiduos.Count(r => r.Status == "Em Estoque");

                            double valorCalculado = todosResiduos.Sum(r => r.Quantidade * 5.50);
                            Total = valorCalculado.ToString("C2");

                            // 2. Atualizando a DataGrid (Últimos 5 cadastrados)
                            var ultimos = todosResiduos.OrderByDescending(r => r.DataCadastro).Take(5);
                            UltimosResiduos = new ObservableCollection<Residuo>(ultimos);

                            // 3. Alimentando o Gráfico de Pizza (Status dos Resíduos)
                            // Agrupa por status e conta a quantidade de registros em cada um
                            var statusAgrupado = todosResiduos.GroupBy(r => r.Status);
                            GraficoStatus = new SeriesCollection();
                            foreach (var grupo in statusAgrupado)
                            {
                                GraficoStatus.Add(new PieSeries
                                {
                                    Title = string.IsNullOrEmpty(grupo.Key) ? "Não Informado" : grupo.Key,
                                    Values = new ChartValues<int> { grupo.Count() },
                                    DataLabels = true
                                });
                            }

                            // 4. Alimentando o Gráfico de Barras (Tipo de Resíduos por Peso)
                            // Agrupa pelo Material/Tipo e soma o peso acumulado (Quantidade)
                            var tiposAgrupados = todosResiduos.GroupBy(r => r.TipoResiduo)
                                                              .Select(g => new { Tipo = g.Key, PesoTotal = g.Sum(r => r.Quantidade) })
                                                              .ToList();

                            LabelsTipos = tiposAgrupados.Select(t => string.IsNullOrEmpty(t.Tipo) ? "Outros" : t.Tipo).ToArray();

                            GraficoTipos = new SeriesCollection
                            {
                                new ColumnSeries
                                {
                                    Title = "Quantidade Acumulada",
                                    Values = new ChartValues<double>(tiposAgrupados.Select(t => t.PesoTotal))
                                }
                            };
                        });
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("A API conectou, mas a lista de resíduos veio vazia.", "Aviso");
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show($"A API retornou um erro: {response.StatusCode}", "Erro HTTP");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Falha de conexão: {ex.Message}", "Erro de Código");
            }
        }
    }

    // Classe auxiliar simples para implementação do Command caso não possua no seu projeto
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Action _execute;

        public RelayCommand(Func<Task> executeAsync, Func<object, bool> value) => _executeAsync = executeAsync;
        public RelayCommand(Action execute) => _execute = execute;

        public event EventHandler CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object parameter) => true;
        public async void Execute(object parameter)
        {
            if (_executeAsync != null) await _executeAsync();
            else _execute?.Invoke();
        }
    }
}