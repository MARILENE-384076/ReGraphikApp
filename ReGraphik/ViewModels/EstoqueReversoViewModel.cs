using Firebase.Database;
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
        private static readonly HttpClient _http = new();
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
        public ObservableCollection<string> ListaTipos { get; } = new() { "Todos" };
        public ObservableCollection<string> ListaOrigens { get; } = new() { "Todas" };
        public ObservableCollection<string> ListaStatus { get; } = new() { "Todos" };

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
        public ICommand FiltrarCommand { get; }
        public ICommand LimparFiltrosCommand { get; }
        public ICommand SugestaoCommand { get; }
        public ICommand ExportarCommand { get; }

        public EstoqueReversoViewModel()
        {
            ResiduosFiltrados = CollectionViewSource.GetDefaultView(_todosResiduos);
            ResiduosFiltrados.Filter = AplicarFiltro;

            SugestaoCommand = new RelayCommand((param) => AbrirSugestoes(param as Residuo));

            ExportarCommand = new RelayCommand(() => ExportarDadosInternos());
            FiltrarCommand = new RelayCommand(() => ResiduosFiltrados.Refresh());
            LimparFiltrosCommand = new RelayCommand(() =>
            {
                FiltroTipo = string.Empty;
                FiltroOrigem = string.Empty;
                FiltroStatus = string.Empty;
                FiltroPeriodo = string.Empty;
                ResiduosFiltrados.Refresh();
            });

            _ = CarregarEstoqueDoBancoAsync();
        }

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
                    "Últimos 7 dias" => 7,
                    "Últimos 30 dias" => 30,
                    "Últimos 90 dias" => 90,
                    _ => 0
                };
                if (dias > 0 && r.DataCadastro < DateTime.Now.AddDays(-dias))
                    return false;
            }

            return true;
        }

        private void ExportarDadosInternos()
        {
            MessageBox.Show("Comando de exportação acionado internamente!", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// O método recebe o objeto Residuo específico da linha selecionada
        /// </summary>
        /// <param name="residuo"></param>
        private void AbrirSugestoes(Residuo? residuo)
        {
            if (residuo == null)
            {
                MessageBox.Show("Não foi possível identificar o resíduo selecionado.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            /// Cria a janela moldura 
            var tela = new SugestaoResiduoWindow(residuo);
            tela.Owner = Application.Current.MainWindow;

            tela.ShowDialog();
        }

        /// <summary>
        /// Busca resíduos na API e popula os ComboBoxes com valores reais e distintos do banco.
        /// </summary>
        public async Task CarregarEstoqueDoBancoAsync()
        {
            try
            {
                var firebase = new FirebaseClient("https://regraphikfirebase-default-rtdb.firebaseio.com/");

                firebase
                .Child("residuos")
                .AsObservable<Residuo>()
                .Subscribe(subsecao =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var residuoDoFirebase = subsecao.Object;
                        if (residuoDoFirebase == null) return;

                        /// Garante que o ID interno bata com a chave do nó se necessário
                        if (string.IsNullOrEmpty(residuoDoFirebase.Id))
                        {
                            residuoDoFirebase.Id = subsecao.Key;
                        }

                        if (subsecao.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                        {
                            /// Remove uma versão antiga caso seja uma edição para não duplicar no grid
                            var itemExistente = _todosResiduos.FirstOrDefault(r => r.Id == residuoDoFirebase.Id);
                            if (itemExistente != null)
                            {
                                _todosResiduos.Remove(itemExistente);
                            }

                            /// Adiciona o novo resíduo na lista
                            _todosResiduos.Add(residuoDoFirebase);

                            /// Popula os ComboBoxes de filtros dinamicamente usando a variável certa
                            if (!string.IsNullOrWhiteSpace(residuoDoFirebase.TipoResiduo) && !ListaTipos.Contains(residuoDoFirebase.TipoResiduo))
                                ListaTipos.Add(residuoDoFirebase.TipoResiduo);

                            if (!string.IsNullOrWhiteSpace(residuoDoFirebase.Origem) && !ListaOrigens.Contains(residuoDoFirebase.Origem))
                                ListaOrigens.Add(residuoDoFirebase.Origem);

                            if (!string.IsNullOrWhiteSpace(residuoDoFirebase.Status) && !ListaStatus.Contains(residuoDoFirebase.Status))
                                ListaStatus.Add(residuoDoFirebase.Status);
                        }

                        else if (subsecao.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                        {
                            var itemExistente = _todosResiduos.FirstOrDefault(r => r.Id == subsecao.Key);
                            if (itemExistente != null)
                            {
                                _todosResiduos.Remove(itemExistente);
                            }
                        }

                        /// Força o DataGrid a se reorganizar aplicando os filtros ativos
                        ResiduosFiltrados.Refresh();
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar estoque: {ex.Message}");
            }
        }
    }
}