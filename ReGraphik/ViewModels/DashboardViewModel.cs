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
using ReGraphik.Services.Interface;

namespace ReGraphik.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        // Instância do serviço de resíduos para lidar com a lógica relacionada aos resíduos
        private readonly IResiduoService _residuoService = new Services.ResiduoService();

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
        public DashboardViewModel(string nomeUsuario)
        {
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
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        // Atualiza as propriedades dos Cards
                        ResiduosDb = todosResiduos.Count;
                        Reaproveitar = todosResiduos.Count(r => r.Status == "Reaproveitado");
                        Estoque = todosResiduos.Count(r => r.Status == "Em Estoque");

                        double valorCalculado = todosResiduos.Sum(r => r.Quantidade * 5.50);
                        Total = valorCalculado.ToString("C2");

                        // Atualiza a Tabela com os 5 resíduos mais recentes
                        var ultimos = todosResiduos.OrderByDescending(r => r.DataCadastro).Take(5);
                        UltimosResiduos = new ObservableCollection<Residuo>(ultimos);

                        // Agrupa os resíduos por status para criar o gráfico de pizza
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

                        // Agrupa os resíduos por tipo para criar o gráfico de colunas
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
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Falha de conexão: {ex.Message}", "Erro de Código");
            }
        }
    }
}