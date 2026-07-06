using Firebase.Database;
using ReGraphik.Models;
using ReGraphik.Views;
using ReGraphik.Views.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; 
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
        /// Listas de opções para os filtros de Tipo, Origem e Status. Inicializadas com valores padrão "Todos" ou "Todas".
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
        /// Comandos para ações de filtragem, limpeza de filtros, abertura de sugestões e exportação de dados.
        /// </summary>
        public ICommand FiltrarCommand { get; }
        public ICommand LimparFiltrosCommand { get; }
        public ICommand SugestaoCommand { get; }
        public ICommand ExportarCommand { get; }

        public ICommand AbrirDetalhesCommand { get; }

        /// <summary>
        /// Inicializa uma nova instância do ViewModel, configurando a coleção filtrada e os comandos.
        /// </summary>
        public EstoqueReversoViewModel()
        {
            ResiduosFiltrados = CollectionViewSource.GetDefaultView(_todosResiduos);
            ResiduosFiltrados.Filter = AplicarFiltro;

            SugestaoCommand = new RelayCommand((param) => AbrirSugestoes(param as Residuo));

            /// Comando de exportação de dados internos (simulado)
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

            /// Carrega os resíduos do Firebase Realtime Database de forma assíncrona
            _ = CarregarEstoqueDoBancoAsync();

            AbrirDetalhesCommand = new RelayCommand((param) => AbrirDetalhes(param as Residuo));
        }

        /// <summary>
        /// Aplica os filtros definidos pelo usuário sobre cada item da coleção de resíduos.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Simula a exportação de dados internos, exibindo uma mensagem de aviso ao usuário.
        /// </summary>
        private void ExportarDadosInternos()
        {
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                MensagemWindow.Exibir("Aviso", "Comando de exportação acionado internamente!", MensagemWindow.TipoMensagem.Aviso);
            });
        }

        /// <summary>
        /// Abre a janela de sugestões para o resíduo selecionado. Se o resíduo for nulo, exibe uma mensagem de aviso.
        /// </summary>
        /// <param name="residuo"></param>
        private void AbrirSugestoes(Residuo? residuo)
        {
            if (residuo == null)
            {
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Aviso", "Não foi possível identificar o resíduo selecionado.", MensagemWindow.TipoMensagem.Aviso);
                });
                return;
            }

            var tela = new SugestaoResiduoWindow(residuo);
            tela.Owner = Application.Current.MainWindow;
            tela.ShowDialog();
        }

        /// <summary>
        /// Abre a janela de detalhes para o resíduo selecionado. Se o resíduo for nulo, exibe uma mensagem de aviso.
        /// </summary>
        /// <param name="residuo"></param>
        private void AbrirDetalhes(Residuo? residuo)
        {
            if (residuo == null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Aviso", "Não foi possível identificar o resíduo selecionado.", MensagemWindow.TipoMensagem.Aviso);
                });
                return;
            }

            // Altere 'DetalhesResiduoWindow' para o nome exato da sua Window/Tela de detalhes
            var tela = new DetalhesResiduoWindow(residuo);
            tela.Owner = Application.Current.MainWindow;
            tela.ShowDialog();
        }

        /// <summary>
        /// Carrega os resíduos do Firebase Realtime
        /// </summary>
        /// <returns></returns>
        public async Task CarregarEstoqueDoBancoAsync()
        {
            try
            {
                /// Inicializa o cliente do Firebase com a URL do Realtime Database
                var firebase = new FirebaseClient("https://regraphikfirebase-default-rtdb.firebaseio.com/");

                /// Observa as alterações na coleção de resíduos no Firebase e atualiza a coleção local de forma thread-safe
                firebase
                .Child("residuos")
                .AsObservable<Residuo>()
                .Subscribe(subsecao =>
                {
                    /// Atualiza a coleção de resíduos na thread da interface do usuário para evitar problemas de threading
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        /// Obtém o objeto de resíduo do evento do Firebase. Se for nulo, retorna sem fazer nada.
                        var residuoDoFirebase = subsecao.Object;
                        if (residuoDoFirebase == null) return;

                        if (string.IsNullOrEmpty(residuoDoFirebase.Id))
                        {
                            residuoDoFirebase.Id = subsecao.Key;
                        }

                        /// Trata os eventos de inserção/atualização e exclusão, atualizando a coleção local e as listas de filtros conforme necessário
                        if (subsecao.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                        {
                            var itemExistente = _todosResiduos.FirstOrDefault(r => r.Id == residuoDoFirebase.Id);
                            if (itemExistente != null)
                            {
                                _todosResiduos.Remove(itemExistente);
                            }

                            _todosResiduos.Add(residuoDoFirebase);

                            if (!string.IsNullOrWhiteSpace(residuoDoFirebase.TipoResiduo) && !ListaTipos.Contains(residuoDoFirebase.TipoResiduo))
                                ListaTipos.Add(residuoDoFirebase.TipoResiduo);

                            if (!string.IsNullOrWhiteSpace(residuoDoFirebase.Origem) && !ListaOrigens.Contains(residuoDoFirebase.Origem))
                                ListaOrigens.Add(residuoDoFirebase.Origem);

                            if (!string.IsNullOrWhiteSpace(residuoDoFirebase.Status) && !ListaStatus.Contains(residuoDoFirebase.Status))
                                ListaStatus.Add(residuoDoFirebase.Status);
                        }

                        /// Trata o evento de exclusão, removendo o item da coleção local se ele existir
                        else if (subsecao.EventType == Firebase.Database.Streaming.FirebaseEventType.Delete)
                        {
                            var itemExistente = _todosResiduos.FirstOrDefault(r => r.Id == subsecao.Key);
                            if (itemExistente != null)
                            {
                                _todosResiduos.Remove(itemExistente);
                            }
                        }

                        ResiduosFiltrados.Refresh();
                    });
                });
            }
            catch (Exception ex)
            {
                /// Loga o erro no console de depuração para análise posterior
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar estoque: {ex.Message}");

                /// Exibe uma mensagem de erro ao usuário na thread da interface do usuário
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir("Erro de Conexão", $"Não foi possível buscar os dados de estoque!!", MensagemWindow.TipoMensagem.Erro);
                });
            }
        }
    }
}