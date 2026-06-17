using ReGraphik.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel responsável por buscar sugestões de reaproveitamento no banco de dados
    /// e exibi-las em modo de leitura com base no material selecionado no estoque reverso.
    /// </summary>
    public class SugestaoResiduoViewModel : BaseViewModel
    {
        private static readonly HttpClient _http = new();

        private const string UrlApiResiduos = "https://webregraphik.runasp.net/api/Residuo";
        private const string UrlApiSugestoesBanco = "https://webregraphik.runasp.net/api/Sugestao";

        private List<Sugestao> _todasAsSugestoesDoBanco = new();

    
        /// <summary>
        /// Evento mantido para compatibilidade com o fechamento/atualização mapeado na View.
        /// </summary>
        public event Action SugestaoAplicadaComSucesso;

        /// <summary>
        /// Coleção de resíduos disponíveis em estoque que alimenta o ComboBox da interface.
        /// </summary>
        public ObservableCollection<Residuo> ResiduosEstoque { get; } = new();

        /// <summary>
        /// Coleção filtrada de sugestões compatíveis com o tipo de resíduo selecionado.
        /// </summary>
        public ObservableCollection<Sugestao> SugestoesFiltradas { get; } = new();

        private Residuo _residuoSelecionado;
        /// <summary>
        /// Obtém ou define o resíduo atualmente focado. Dispara a filtragem automática de sugestões ao ser alterado.
        /// </summary>
        public Residuo ResiduoSelecionado
        {
            get => _residuoSelecionado;
            set
            {
                _residuoSelecionado = value;
                OnPropertyChanged();
                FiltrarSugestoesPorMaterial();
            }
        }

        private bool _carregando;
        public bool Carregando
        {
            get => _carregando;
            set { _carregando = value; OnPropertyChanged(); }
        }

        private string _mensagem = "Carregando dados do sistema...";
        public string Mensagem
        {
            get => _mensagem;
            set { _mensagem = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="SugestaoResiduoViewModel"/>.
        /// </summary>
        /// <param name="residuoInicial">Resíduo opcional para pré-seleção ao abrir a janela.</param>
        public SugestaoResiduoViewModel(Residuo residuoInicial = null)
        {
            _ = InicializarDadosAsync(residuoInicial);
        }

        /// <summary>
        /// Realiza a busca paralela dos cadastros de resíduos e das ideias de sugestões existentes na API.
        /// </summary>
        private async Task InicializarDadosAsync(Residuo residuoInicial)
        {
            Carregando = true;
            Mensagem = " Consultando diretório de ideias no banco de dados...";

            try
            {
                var tarefaResiduos = _http.GetAsync(UrlApiResiduos);
                var tarefaSugestoes = _http.GetAsync(UrlApiSugestoesBanco);

                await Task.WhenAll(tarefaResiduos, tarefaSugestoes);

                if (!tarefaResiduos.Result.IsSuccessStatusCode || !tarefaSugestoes.Result.IsSuccessStatusCode)
                {
                    Mensagem = " Erro ao baixar tabelas do banco de dados.";
                    return;
                }

                var jsonResiduos = await tarefaResiduos.Result.Content.ReadAsStringAsync();
                var jsonSugestoes = await tarefaSugestoes.Result.Content.ReadAsStringAsync();

                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var listaResiduos = JsonSerializer.Deserialize<List<Residuo>>(jsonResiduos, opcoes) ?? new List<Residuo>();
                _todasAsSugestoesDoBanco = JsonSerializer.Deserialize<List<Sugestao>>(jsonSugestoes, opcoes) ?? new List<Sugestao>();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ResiduosEstoque.Clear();
                    Residuo focar = null;

                    foreach (var r in listaResiduos)
                    {
                        ResiduosEstoque.Add(r);
                        if (residuoInicial != null && r.Id == residuoInicial.Id) focar = r;
                    }

                    if (focar != null) ResiduoSelecionado = focar;
                    else if (ResiduosEstoque.Count > 0) ResiduoSelecionado = ResiduosEstoque[0];
                });
            }
            catch (Exception ex)
            {
                Mensagem = " Falha de conexão de rede.";
                MessageBox.Show($"Não foi possível carregar os dados: {ex.Message}", "Erro de Sincronização", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Carregando = false;
            }
        }

        /// <summary>
        /// Filtra a lista com busca bidirecional e flexível eliminando problemas de correspondência estrita.
        /// </summary>
        private void FiltrarSugestoesPorMaterial()
        {
            if (ResiduoSelecionado == null) return;

            SugestoesFiltradas.Clear();
            string materialAlvo = ResiduoSelecionado.TipoResiduo?.ToLower().Trim() ?? "";

            foreach (var s in _todasAsSugestoesDoBanco)
            {
                if (string.IsNullOrWhiteSpace(s.TipoResiduoAceito)) continue;

                string tipoAceitoNoBanco = s.TipoResiduoAceito.ToLower().Trim();

                if (tipoAceitoNoBanco.Contains(materialAlvo) || materialAlvo.Contains(tipoAceitoNoBanco))
                {
                    SugestoesFiltradas.Add(s);
                }
            }

            if (SugestoesFiltradas.Count > 0)
            {
                Mensagem = $" Exibindo {SugestoesFiltradas.Count} alternativas mapeadas para {ResiduoSelecionado.TipoResiduo}.";
            }
            else
            {
                Mensagem = $"Nenhuma sugestão direta localizada para {ResiduoSelecionado.TipoResiduo}.";
            }
        }
    }
}