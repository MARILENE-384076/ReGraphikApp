using ReGraphik.Models;
using ReGraphik.Views;
using ReGraphik.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel para o controle da tela de Estoque Reverso.
    /// Gerencia o carregamento, filtragem e ações sobre os resíduos em estoque.
    /// </summary>
    public class EstoqueReversoViewModel : BaseViewModel
    {
        private readonly HttpClient _http = new();
        private readonly ObservableCollection<Residuo> _todosResiduos = new();

        /// <summary>
        /// View filtrada vinculada ao DataGrid. O filtro é aplicado sobre esta coleção.
        /// </summary>
        public ICollectionView ResiduosFiltrados { get; }

        /// <summary>
        /// Propriedades de filtro 
        /// </summary>

        private string _filtroTipo = string.Empty;
        public string FiltroTipo
        {
            get => _filtroTipo;
            set { _filtroTipo = value; OnPropertyChanged(); }
        }

        private string _filtroOrigem = string.Empty;
        public string FiltroOrigem
        {
            get => _filtroOrigem;
            set { _filtroOrigem = value; OnPropertyChanged(); }
        }

        private string _filtroStatus = string.Empty;
        public string FiltroStatus
        {
            get => _filtroStatus;
            set { _filtroStatus = value; OnPropertyChanged(); }
        }

        private string _filtroPeriodo = string.Empty;
        public string FiltroPeriodo
        {
            get => _filtroPeriodo;
            set { _filtroPeriodo = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Listas para os ComboBoxes populadas dinamicamente do banco
        /// </summary>

        public ObservableCollection<string> ListaTipos   { get; } = new() { "Todos" };
        public ObservableCollection<string> ListaOrigens { get; } = new() { "Todas" };
        public ObservableCollection<string> ListaStatus  { get; } = new() { "Todos" };

        public List<string> ListaPeriodos { get; } = new()
        {
            "Todos",
            "Últimos 7 dias",
            "Últimos 30 dias",
            "Últimos 90 dias"
        };

        /// <summary>
        /// Comandos 
        /// </summary>

        public ICommand FiltrarCommand       { get; }
        public ICommand LimparFiltrosCommand { get; }
        public ICommand SugestaoCommand      { get; }
        public ICommand ExportarCommand      { get; }

        public EstoqueReversoViewModel()
        {
            ResiduosFiltrados        = CollectionViewSource.GetDefaultView(_todosResiduos);
            ResiduosFiltrados.Filter = AplicarFiltro;

            SugestaoCommand = new RelayCommand(
                (param) => AbrirSugestoes(param),
                (param) => param is Residuo
            );

            ExportarCommand      = new RelayCommand(() => ExportarDadosInternos());
            FiltrarCommand       = new RelayCommand(() => ResiduosFiltrados.Refresh());
            LimparFiltrosCommand = new RelayCommand(() =>
            {
                FiltroTipo    = string.Empty;
                FiltroOrigem  = string.Empty;
                FiltroStatus  = string.Empty;
                FiltroPeriodo = string.Empty;
                ResiduosFiltrados.Refresh();
            });

            _ = CarregarEstoqueDoBancoAsync();
        }

        /// Lógica de filtro 

        /// <summary>
        /// Predicado do CollectionView — combina todos os filtros ativos com lógica AND.
        /// </summary>
        private bool AplicarFiltro(object item)
        {
            if (item is not Residuo r) return false;

            if (!string.IsNullOrWhiteSpace(FiltroTipo) && FiltroTipo != "Todos")
                if (!r.TipoResiduo?.Contains(FiltroTipo, StringComparison.OrdinalIgnoreCase) ?? true)
                    return false;

            if (!string.IsNullOrWhiteSpace(FiltroOrigem) && FiltroOrigem != "Todas")
                if (!r.Origem?.Contains(FiltroOrigem, StringComparison.OrdinalIgnoreCase) ?? true)
                    return false;

            if (!string.IsNullOrWhiteSpace(FiltroStatus) && FiltroStatus != "Todos")
                if (!r.Status?.Contains(FiltroStatus, StringComparison.OrdinalIgnoreCase) ?? true)
                    return false;

            if (!string.IsNullOrWhiteSpace(FiltroPeriodo) && FiltroPeriodo != "Todos")
            {
                int dias = FiltroPeriodo switch
                {
                    "Últimos 7 dias"  => 7,
                    "Últimos 30 dias" => 30,
                    "Últimos 90 dias" => 90,
                    _                 => 0
                };
                if (dias > 0 && r.DataCadastro < DateTime.Now.AddDays(-dias))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Ações 
        /// </summary>

        private void ExportarDadosInternos()
        {
            MessageBox.Show("Comando de exportação acionado internamente!", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AbrirSugestoes(object param)
        {
            if (param is not Residuo residuo) return;
            if (Application.Current.MainWindow is MainWindow mainWin)
                mainWin.Content = new SugestaoResiduoControl(residuo);
        }

        /// Carregamento de dados 

        /// <summary>
        /// Busca resíduos na API e popula os ComboBoxes com valores reais e distintos do banco.
        /// </summary>
        private async Task CarregarEstoqueDoBancoAsync()
        {
            try
            {
                const string urlApi = "https://webregraphik.runasp.net/api/Residuo";
                var resposta = await _http.GetAsync(urlApi);

                if (!resposta.IsSuccessStatusCode) return;

                var json   = await resposta.Content.ReadAsStringAsync();
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var lista  = JsonSerializer.Deserialize<List<Residuo>>(json, opcoes);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _todosResiduos.Clear();
                    ListaTipos.Clear();   ListaTipos.Add("Todos");
                    ListaOrigens.Clear(); ListaOrigens.Add("Todas");
                    ListaStatus.Clear();  ListaStatus.Add("Todos");

                    if (lista == null) return;

                    foreach (var item in lista)
                    {
                        _todosResiduos.Add(item);

                        if (!string.IsNullOrWhiteSpace(item.TipoResiduo) && !ListaTipos.Contains(item.TipoResiduo))
                            ListaTipos.Add(item.TipoResiduo);

                        if (!string.IsNullOrWhiteSpace(item.Origem) && !ListaOrigens.Contains(item.Origem))
                            ListaOrigens.Add(item.Origem);

                        if (!string.IsNullOrWhiteSpace(item.Status) && !ListaStatus.Contains(item.Status))
                            ListaStatus.Add(item.Status);
                    }

                    ResiduosFiltrados.Refresh();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar estoque: {ex.Message}");
            }
        }
    }
}