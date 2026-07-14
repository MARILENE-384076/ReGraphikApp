using ReGraphik.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Firebase.Database;
using Firebase.Database.Query;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel responsável por gerenciar a lógica de negócios da tela de Sugestões de Reaproveitamento.
    /// Implementa um padrão de carregamento Híbrido (Offline-First) para zerar a latência de interface.
    /// </summary>
    public class SugestaoResiduoViewModel : BaseViewModel
    {
        private static readonly HttpClient _http = new();
        private const string UrlApiResiduos = "https://webregraphik.runasp.net/api/Residuo";
        private const string UrlApiSugestoesBanco = "https://webregraphik.runasp.net/api/Sugestao";

        private readonly string ArquivoCacheResiduos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache_residuos.json");
        private readonly string ArquivoCacheSugestoes = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache_sugestoes.json");

        /// <summary>
        /// Evento disparado para notificar a View ou outras ViewModels que uma sugestão foi processada ou aplicada com sucesso.
        /// </summary>
        public event Action? SugestaoAplicadaComSucesso;

        /// <summary>
        /// Coleção principal que armazena os resíduos disponíveis para seleção no ComboBox.
        /// </summary>
        public ObservableCollection<Residuo> ResiduosEstoque { get; } = new();

        /// <summary>
        /// Coleção base que armazena todas as sugestões baixadas (não é exibida diretamente na tela).
        /// </summary>
        public ObservableCollection<Sugestao> TodasAsSugestoes { get; } = new();

        /// <summary>
        /// Visão filtrada e otimizada da coleção de sugestões. É nela que o ListBox do XAML faz o Binding.
        /// </summary>
        public ICollectionView SugestoesFiltradasView { get; private set; }

        private Residuo _residuoSelecionado;

        /// <summary>
        /// Obtém ou define o resíduo atualmente selecionado pelo usuário no ComboBox.
        /// Ao ser alterado, aciona automaticamente a filtragem das sugestões.
        /// </summary>
        public Residuo ResiduoSelecionado
        {
            get => _residuoSelecionado;
            set
            {
                try
                {
                    _residuoSelecionado = value;
                    OnPropertyChanged();
                    SugestoesFiltradasView?.Refresh();
                    AtualizarMensagemQuantidade();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Erro UI] Falha ao atualizar seleção: {ex.Message}");
                }
            }
        }

        private string _mensagem = "";

        /// <summary>
        /// Mensagem de status exibida para o usuário na interface (ex: quantidade de itens, erros ou status de conexão).
        /// </summary>
        public string Mensagem
        {
            get => _mensagem;
            set { _mensagem = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="SugestaoResiduoViewModel"/>.
        /// Configura os filtros visuais e dispara o carregamento assíncrono.
        /// </summary>
        /// <param name="residuoInicial">O resíduo que deve vir pré-selecionado ao abrir a tela (opcional).</param>
        public SugestaoResiduoViewModel(Residuo residuoInicial = null)
        {
            try
            {
                SugestoesFiltradasView = CollectionViewSource.GetDefaultView(TodasAsSugestoes);
                SugestoesFiltradasView.Filter = ExecutarFiltroSugestao;

                _ = IniciarCarregamentoHibridoAsync(residuoInicial);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erro Crítico] Falha na inicialização da ViewModel: {ex.Message}");
                Mensagem = "Erro ao iniciar o módulo de sugestões.";
            }
        }

        /// <summary>
        /// Gerencia o fluxo de carregamento duplo: primeiro lê do disco local instantaneamente, depois sincroniza com a nuvem em background.
        /// </summary>
        private async Task IniciarCarregamentoHibridoAsync(Residuo residuoInicial)
        {
            try
            {
                await CarregarDoCacheLocalAsync(residuoInicial);
                await SincronizarComNuvemAsync(residuoInicial);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erro Fluxo Híbrido] {ex.Message}");
            }
        }

        /// <summary>
        /// Lê os arquivos JSON salvos localmente na última sessão para exibir dados em 0 milissegundos sem depender de internet.
        /// </summary>
        private async Task CarregarDoCacheLocalAsync(Residuo residuoInicial)
        {
            try
            {
                if (File.Exists(ArquivoCacheResiduos) && File.Exists(ArquivoCacheSugestoes))
                {
                    string jsonResiduos = await File.ReadAllTextAsync(ArquivoCacheResiduos);
                    string jsonSugestoes = await File.ReadAllTextAsync(ArquivoCacheSugestoes);

                    ProcessarEPreencherListas(jsonResiduos, jsonSugestoes, residuoInicial);
                }
                else
                {
                    Mensagem = "Sincronizando com a nuvem pela primeira vez...";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erro Cache] Falha ao ler local: {ex.Message}");
            }
        }

        /// <summary>
        /// Realiza a busca de Resíduos direto do Firebase e Sugestões da API na nuvem, atualizando a interface e o cache local.
        /// </summary>
        private async Task SincronizarComNuvemAsync(Residuo residuoInicial)
        {
            try
            {
                /// Inicia a busca das Sugestões na API
                var tarefaSugestoes = _http.GetAsync(UrlApiSugestoesBanco);

                ///Busca os Resíduos DIRETAMENTE DO FIREBASE
                var firebase = new FirebaseClient("https://regraphikfirebase-default-rtdb.firebaseio.com/");
                var residuosDoFirebase = await firebase.Child("residuos").OnceAsync<Residuo>();

                /// Extrai apenas a lista de objetos do retorno do Firebase
                var listaResiduos = residuosDoFirebase.Select(item => item.Object).ToList();

                /// Serializa a lista do Firebase de volta para JSON para compatibilidade com o seu método ProcessarEPreencherListas e Cache
                var jsonResiduos = JsonSerializer.Serialize(listaResiduos, new JsonSerializerOptions { PropertyNamingPolicy = null });

                /// Aguarda a resposta da API de Sugestões terminar
                var rSugestoes = await tarefaSugestoes;

                if (!rSugestoes.IsSuccessStatusCode)
                {
                    if (ResiduosEstoque.Count == 0) Mensagem = "Serviço de sugestões indisponível no momento.";
                    return;
                }

                var jsonSugestoes = await rSugestoes.Content.ReadAsStringAsync();

                
                ProcessarEPreencherListas(jsonResiduos, jsonSugestoes, residuoInicial);

                try
                {
                    /// Salva os arquivos de cache para uso offline 
                    await File.WriteAllTextAsync(ArquivoCacheResiduos, jsonResiduos);
                    await File.WriteAllTextAsync(ArquivoCacheSugestoes, jsonSugestoes);
                }
                catch (Exception fileEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[Aviso] Não foi possível salvar o cache: {fileEx.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erro Nuvem] Falha na sincronização: {ex.Message}");
                if (ResiduosEstoque.Count == 0) Mensagem = "Erro ao sincronizar informações com a internet.";
            }
        }

        /// <summary>
        /// Converte as strings JSON em objetos, agrupa itens duplicados e injeta de forma segura na Thread de UI (Dispatcher).
        /// </summary>
        private void ProcessarEPreencherListas(string jsonResiduos, string jsonSugestoes, Residuo residuoInicial)
        {
            Task.Run(() =>
            {
                try
                {
                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    /// Deserializa os JSONs em listas de objetos, garantindo que não sejam nulos
                    var listaResiduosRaw = JsonSerializer.Deserialize<List<Residuo>>(jsonResiduos, opcoes) ?? new List<Residuo>();
                    var listaSugestoesRaw = JsonSerializer.Deserialize<List<Sugestao>>(jsonSugestoes, opcoes) ?? new List<Sugestao>();

                    var listaResiduosFiltrada = listaResiduosRaw
                        .Where(r => r != null && !string.IsNullOrWhiteSpace(r.TipoResiduo))
                        .GroupBy(r => r.TipoResiduo.Trim().ToLower())
                        .Select(g => g.First())
                        .ToList();

                    /// Atualiza a interface de forma segura na Thread de UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            if (ResiduosEstoque.Count != listaResiduosFiltrada.Count)
                            {
                                ResiduosEstoque.Clear();
                                foreach (var r in listaResiduosFiltrada) ResiduosEstoque.Add(r);
                            }

                            if (TodasAsSugestoes.Count != listaSugestoesRaw.Count)
                            {
                                TodasAsSugestoes.Clear();
                                foreach (var s in listaSugestoesRaw) TodasAsSugestoes.Add(s);
                            }

                            if (residuoInicial != null)
                            {
                                var preSelecionado = ResiduosEstoque.FirstOrDefault(r =>
                                    r.TipoResiduo.Trim().Equals(residuoInicial.TipoResiduo?.Trim(), StringComparison.OrdinalIgnoreCase));

                                if (preSelecionado != null) ResiduoSelecionado = preSelecionado;
                            }

                            if (ResiduoSelecionado == null && ResiduosEstoque.Count > 0)
                            {
                                ResiduoSelecionado = ResiduosEstoque[0];
                            }
                        }
                        catch (Exception exUI)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Erro Dispatcher] Erro ao injetar dados na tela: {exUI.Message}");
                        }
                    });
                }
                catch (Exception exProcess)
                {
                    System.Diagnostics.Debug.WriteLine($"[Erro Processamento] Falha inesperada: {exProcess.Message}");
                }
            });
        }

        /// <summary>
        /// Predicado nativo executado pelo CollectionViewSource do WPF. 
        /// Define se uma sugestão deve aparecer ou ser ocultada no ListBox baseando-se no Resíduo Selecionado.
        /// </summary>
        /// <param name="item">O objeto do tipo Sugestao sendo avaliado.</param>
        /// <returns>Verdadeiro se a sugestão atende aos critérios do material alvo; caso contrário, Falso.</returns>
        private bool ExecutarFiltroSugestao(object item)
        {
            try
            {
                if (ResiduoSelecionado == null) return false;
                if (item is not Sugestao s || string.IsNullOrWhiteSpace(s.TipoResiduoAceito)) return false;

                string materialAlvo = ResiduoSelecionado.TipoResiduo?.ToLower().Trim() ?? "";
                string tipoAceitoNoBanco = s.TipoResiduoAceito.ToLower().Trim();

                return tipoAceitoNoBanco.Contains(materialAlvo) || materialAlvo.Contains(tipoAceitoNoBanco);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Atualiza o texto informativo da interface indicando o volume de sugestões retornadas pelo filtro.
        /// </summary>
        private void AtualizarMensagemQuantidade()
        {
            try
            {
                int contagem = SugestoesFiltradasView?.Cast<object>().Count() ?? 0;
                if (contagem > 0)
                    Mensagem = $"Exibindo {contagem} alternativas mapeadas para {ResiduoSelecionado?.TipoResiduo}.";
                else
                    Mensagem = $"Nenhuma sugestão localizada para {ResiduoSelecionado?.TipoResiduo}.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erro Mensagem] Falha ao contar itens filtrados: {ex.Message}");
            }
        }
    }
}