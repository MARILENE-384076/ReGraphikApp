using ReGraphik.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RestReGraphik.Models;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel da janela de sugestões de reaproveitamento de um resíduo.
    /// Busca as sugestões compatíveis via API, exibe-as e registra a aplicação
    /// gravando um SugestaoResiduo no Firebase através da API REST.
    /// </summary>
    public class SugestaoResiduoViewModel : BaseViewModel
    {
        /// <summary>
        /// Infraestrutura 
        /// </summary>
        private static readonly HttpClient _http = new();
        private const string UrlApiSugestoes = "https://webregraphik.runasp.net/api/Sugestao";
        private const string UrlApiSugestaoResiduo = "https://webregraphik.runasp.net/api/SugestaoResiduos";

        ///Evento disparado após aplicar sugestão com sucesso
        /// <summary>
        /// Disparado quando a sugestão é registrada com sucesso.
        /// A View pode assinar este evento para fechar a janela automaticamente.
        /// </summary>
        public event Action SugestaoAplicadaComSucesso;

        /// <summary>
        /// Resíduo alvo 
        /// </summary>
        public Residuo Residuo { get; }

        /// <summary>
        /// Lista de sugestões compatíveis com o tipo do resíduo 
        /// </summary>
        public ObservableCollection<Sugestao> Sugestoes { get; } = new();

        /// <summary>
        /// Sugestão selecionada pelo usuário na lista 
        /// </summary>
        private Sugestao _sugestaoSelecionada;
        public Sugestao SugestaoSelecionada
        {
            get => _sugestaoSelecionada;
            set
            {
                _sugestaoSelecionada = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Controle de carregamento 
        /// </summary>
        private bool _carregando;
        public bool Carregando
        {
            get => _carregando;
            set { _carregando = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Mensagem de feedback ao usuário 
        /// </summary>
        private string _mensagem = "Carregando sugestões...";
        public string Mensagem
        {
            get => _mensagem;
            set { _mensagem = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Comandos 
        /// </summary>
        public ICommand AplicarSugestaoCommand { get; }

        /// <summary>
        /// Construtor 
        /// </summary>
        /// <param name="residuo"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SugestaoResiduoViewModel()
        {
            AplicarSugestaoCommand = new RelayCommand(
                () => _ = AplicarSugestaoAsync(),
                _ => SugestaoSelecionada != null && !Carregando
            );

            /// Carrega as sugestões assim que o ViewModel for criado
            _ = CarregarSugestoesAsync();
        }

        /// <summary>
        /// Busca sugestões compatíveis com o tipo do resíduo
        /// </summary>
        /// <returns></returns>
        private async Task CarregarSugestoesAsync()
        {
            Carregando = true;
            Mensagem = "Buscando sugestões...";

            try
            {
                var resposta = await _http.GetAsync(UrlApiSugestoes);

                if (!resposta.IsSuccessStatusCode)
                {
                    Mensagem = "Não foi possível buscar as sugestões. Verifique sua conexão.";
                    return;
                }

                var json = await resposta.Content.ReadAsStringAsync();
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var todas = JsonSerializer.Deserialize<List<Sugestao>>(json, opcoes) ?? new List<Sugestao>();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Sugestoes.Clear();
                    foreach (var s in todas)
                    {
                        bool semFiltro = string.IsNullOrWhiteSpace(s.TipoResiduoAceito);
                        bool tipoCompat = !string.IsNullOrWhiteSpace(Residuo.TipoResiduo) &&
                                          s.TipoResiduoAceito != null &&
                                          s.TipoResiduoAceito.Contains(
                                              Residuo.TipoResiduo,
                                              StringComparison.OrdinalIgnoreCase);
                        if (semFiltro || tipoCompat)
                            Sugestoes.Add(s);
                    }

                    Mensagem = Sugestoes.Count > 0
                        ? $"{Sugestoes.Count} sugestão(ões) encontrada(s) para \"{Residuo.TipoResiduo}\"."
                        : $"Nenhuma sugestão encontrada para o tipo \"{Residuo.TipoResiduo}\".";
                });
            }
            catch (Exception ex)
            {
                Mensagem = $"Erro ao carregar sugestões: {ex.Message}";
                System.Diagnostics.Debug.WriteLine("Erro ao carregar sugestões: " + ex.Message);
            }
            finally
            {
                Carregando = false;
            }
        }

        /// <summary>
        /// Registra a sugestão aplicada via POST em api/SugestaoResiduos 
        /// </summary>
        /// <returns></returns>
        private async Task AplicarSugestaoAsync()
        {
            if (SugestaoSelecionada == null) return;

            Carregando = true;
            Mensagem = "Aplicando sugestão...";

            try
            {
                var payload = new
                {
                    id = Guid.NewGuid().ToString(),
                    id_cadastro_residuo = Residuo.Id,
                    id_sugestao = SugestaoSelecionada.Id,
                    data_aplicacao = DateTime.UtcNow
                };

                var body = JsonSerializer.Serialize(payload);
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var resposta = await _http.PostAsync(UrlApiSugestaoResiduo, content);

                if (resposta.IsSuccessStatusCode)
                {
                    Mensagem = $"✅ Sugestão aplicada com sucesso ao resíduo \"{Residuo.TipoResiduo}\"!";

                    MessageBox.Show(
                        $"Sugestão aplicada com sucesso!\n\n" +
                        $"Resíduo : {Residuo.TipoResiduo}\n" +
                        $"Sugestão: {SugestaoSelecionada.DescricaoSugestao}",
                        "Sugestão Aplicada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    /// Notifica a View para fechar a janela
                    SugestaoAplicadaComSucesso?.Invoke();
                }
                else
                {
                    var erro = await resposta.Content.ReadAsStringAsync();
                    Mensagem = $"Erro ao aplicar sugestão: {resposta.StatusCode}";

                    MessageBox.Show(
                        $"Não foi possível aplicar a sugestão.\n" +
                        $"Código  : {resposta.StatusCode}\n" +
                        $"Detalhe : {erro}",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Mensagem = $"Erro ao aplicar sugestão: {ex.Message}";
                System.Diagnostics.Debug.WriteLine("Erro ao aplicar sugestão: " + ex.Message);

                MessageBox.Show(
                    $"Erro inesperado ao aplicar a sugestão:\n{ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Carregando = false;
            }
        }
    }
}
