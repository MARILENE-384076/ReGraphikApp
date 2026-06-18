using ReGraphik.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// ViewModel responsável pelo controle lógico da tela de mapas e gerenciamento dos pontos de coleta.
    /// Realiza a ponte de comunicação assíncrona entre os dados da API remota e o mapa interativo em JavaScript (Leaflet).
    /// </summary>
    public class MapaViewModel : BaseViewModel
    {
        /// <summary>
        /// Instância única e reutilizável do cliente HTTP para evitar o esgotamento de sockets na aplicação.
        /// </summary>
        private readonly HttpClient _http = new();

        /// <summary>
        /// Flag indicativa que sinaliza se a estrutura HTML do mapa já terminou de ser carregada pelo componente visual.
        /// </summary>
        private bool _mapaCarregado = false;

        /// <summary>
        /// Armazenamento interno para o nome da cidade utilizada em consultas textuais diretas.
        /// </summary>
        private string _cidade = string.Empty;

        /// <summary>
        /// Coleção monitorada que armazena os pontos de coleta atualmente visíveis na interface do usuário.
        /// </summary>
        private ObservableCollection<PontosColeta> _pontosAtuais = new();

        /// <summary>
        /// Flag indicativa para controle de estados de carregamento (busy indicator) na interface.
        /// </summary>
        private bool _isCarregando;

        /// <summary>
        /// Evento disparado para solicitar ao Code-Behind que invoque uma função JavaScript focando um marcador específico.
        /// </summary>
        public event Action<int>? SolicitouFocoNoMapa;

        /// <summary>
        /// Evento disparado para transferir a string contendo o código HTML estrutural do mapa diretamente à View.
        /// </summary>
        public event Action<string>? SolicitouHtmlMapa;

        /// <summary>
        /// Obtém ou define o nome da cidade usada na pesquisa síncrona/filtragem.
        /// Dispara notificações de mudança de propriedade para manter o Binding da View atualizado.
        /// </summary>
        public string Cidade
        {
            get => _cidade;
            set { _cidade = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Obtém ou define a lista de pontos ativos exibidos na interface.
        /// Notifica alterações para reavaliar a visibilidade de estados vazios automáticos.
        /// </summary>
        public ObservableCollection<PontosColeta> PontosAtuais
        {
            get => _pontosAtuais;
            set { _pontosAtuais = value; OnPropertyChanged(); OnPropertyChanged(nameof(MostrarEstadoVazio)); }
        }

        /// <summary>
        /// Obtém ou define o estado de processamento de dados atual da tela.
        /// Controla a exibição de barras de progresso e indisponibilidade temporária de componentes.
        /// </summary>
        public bool IsCarregando
        {
            get => _isCarregando;
            set
            {
                _isCarregando = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MostrarEstadoVazio));
            }
        }

        /// <summary>
        /// Propriedade calculada reativa que indica se a lista de pontos está zerada e sem operações pendentes de busca.
        /// </summary>
        public bool MostrarEstadoVazio => !IsCarregando && (PontosAtuais == null || PontosAtuais.Count == 0);

        /// <summary>
        /// Comando vinculado ao botão físico ou lógico de busca manual na interface gráfica.
        /// </summary>
        public ICommand BuscarCommand { get; set; }

        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="MapaViewModel"/> e configura as diretrizes de execução dos comandos.
        /// </summary>
        public MapaViewModel()
        {
            BuscarCommand = new RelayCommand(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await BuscarAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Erro fatal interceptado: " + ex.Message);
                    }
                });
            });
        }

        /// <summary>
        /// Força o carregamento da janela inicial do mapa sem nenhum ponto pré-carregado.
        /// Configura o enquadramento padrão sobre a área geográfica brasileira para livre navegação.
        /// </summary>
        public void InicializarMapaLivre()
        {
            CarregarMapaInicial(new List<PontosColeta>());
        }

        /// <summary>
        /// Executa a busca dinâmica e reativa de pontos de coleta baseando-se no retângulo visível (Bounding Box) fornecido pelo mapa Leaflet.
        /// Este método é invocado em segundo plano assim que o usuário cessa o arraste ou o zoom no navegador.
        /// </summary>
        /// <param name="southWestLat">Latitude do canto inferior esquerdo visível na tela.</param>
        /// <param name="southWestLng">Longitude do canto inferior esquerdo visível na tela.</param>
        /// <param name="northEastLat">Latitude do canto superior direito visível na tela.</param>
        /// <param name="northEastLng">Longitude do canto superior direito visível na tela.</param>
        /// <returns>Uma tarefa assíncrona que representa a operação de filtragem e atualização da UI.</returns>
        public async Task BuscarPorCoordenadasAsync(double southWestLat, double southWestLng, double northEastLat, double northEastLng)
        {
            try
            {
                var todosPontos = await BuscarPontosDoBancoAsync();

                // Filtro geométrico em memória: seleciona registros contidos estritamente nos limites da tela atual
                var visiveisNoMapa = todosPontos.Where(p =>
                    p.Lat >= southWestLat && p.Lat <= northEastLat &&
                    p.Lng >= southWestLng && p.Lng <= northEastLng).ToList();

                // Garante que as mutações na coleção Observable ocorram estritamente na Thread principal de UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PontosAtuais.Clear();
                    foreach (var p in visiveisNoMapa)
                    {
                        PontosAtuais.Add(p);
                    }

                    OnPropertyChanged(nameof(MostrarEstadoVazio));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro na busca por movimento: " + ex.Message);
            }
        }

        /// <summary>
        /// Realiza a rotina tradicional de busca textual enviando um comando de sincronização para a API remota externa.
        /// </summary>
        /// <returns>Uma tarefa assíncrona representando o envio do POST e o processamento de retorno.</returns>
        private async Task BuscarAsync()
        {
            if (string.IsNullOrWhiteSpace(Cidade))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window windowDona = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;
                    MessageBox.Show(windowDona, "Por favor, digite o nome de uma cidade.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
                return;
            }

            IsCarregando = true;

            try
            {
                string urlSincronizar = $"https://webregraphik.runasp.net/api/PontosColeta/sincronizar?cidade={Uri.EscapeDataString(Cidade)}";
                var conteudoVazio = new StringContent("", Encoding.UTF8, "application/json");
                var response = await _http.PostAsync(urlSincronizar, conteudoVazio);

                if (response.IsSuccessStatusCode)
                {
                    var pontosSalvos = await BuscarPontosDoBancoAsync();
                    var filtrados = pontosSalvos
                        .Where(p => p.Cidade != null && p.Cidade.Contains(Cidade, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PontosAtuais.Clear();
                        foreach (var p in filtrados)
                        {
                            PontosAtuais.Add(p);
                        }
                        CarregarMapaInicial(filtrados);
                        IsCarregando = false;
                    });
                }
                else
                {
                    IsCarregando = false;
                }
            }
            catch
            {
                IsCarregando = false;
            }
        }

        /// <summary>
        /// Solicita de maneira reativa o foco visual e a abertura do popup informativo de um ponto específico com base no item selecionado.
        /// </summary>
        /// <param name="ponto">A entidade <see cref="PontosColeta"/> que receberá o foco da câmera do mapa.</param>
        public void FocarNoPonto(PontosColeta ponto)
        {
            if (_mapaCarregado && PontosAtuais != null)
            {
                var idx = PontosAtuais.IndexOf(ponto);
                if (idx >= 0)
                {
                    SolicitouFocoNoMapa?.Invoke(idx);
                }
            }
        }

        /// <summary>
        /// Notificação externa emitida pela camada View indicando que a árvore DOM do mapa Leaflet concluiu sua montagem inicial de forma segura.
        /// </summary>
        public void NotificarMapaCarregado()
        {
            _mapaCarregado = true;
        }

        /// <summary>
        /// Consome a API REST principal via método GET para extrair a listagem universal dos pontos armazenados no banco de dados.
        /// </summary>
        /// <returns>Uma lista tipada contendo os pontos de coleta decodificados do formato JSON.</returns>
        private async Task<List<PontosColeta>> BuscarPontosDoBancoAsync()
        {
            var lista = new List<PontosColeta>();
            try
            {
                var urlApi = "https://webregraphik.runasp.net/api/PontosColeta";
                var resposta = await _http.GetAsync(urlApi);

                if (resposta.IsSuccessStatusCode)
                {
                    var json = await resposta.Content.ReadAsStringAsync();
                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<PontosColeta>>(json, opcoes) ?? lista;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao listar pontos: " + ex.Message);
            }
            return lista;
        }

        /// <summary>
        /// Constrói e injeta a estrutura primária do mapa gerando o código HTML cru e disparando os gatilhos de renderização no WebBrowser.
        /// </summary>
        /// <param name="pontos">Lista de pontos iniciais que devem ser renderizados no carregamento do mapa.</param>
        private void CarregarMapaInicial(List<PontosColeta> pontos)
        {
            var html = GerarHtml(pontos);
            _mapaCarregado = false;
            SolicitouHtmlMapa?.Invoke(html);
        }

        /// <summary>
        /// Serializa uma coleção de objetos <see cref="PontosColeta"/> em uma string formatada em JSON nativo JavaScript.
        /// Garante a sanitização e a formatação correta de coordenadas em conformidade com o padrão internacional InvariantCulture.
        /// </summary>
        /// <param name="pontos">A coleção de pontos a ser convertida em formato textual JavaScript.</param>
        /// <returns>String contendo a estrutura de um vetor JSON no formato "[{...}, {...}]".</returns>
        public string GerarJsonMarcadores(List<PontosColeta> pontos)
        {
            pontos ??= new List<PontosColeta>();
            var marcadoresJs = new StringBuilder("[");
            for (int i = 0; i < pontos.Count; i++)
            {
                var p = pontos[i];
                if (i > 0) marcadoresJs.Append(",");

                string latitude = p.Lat != 0 ? p.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture) : "-23.55052";
                string longitude = p.Lng != 0 ? p.Lng.ToString(System.Globalization.CultureInfo.InvariantCulture) : "-46.633308";

                marcadoresJs.Append($@"{{
                    ""idx"": {i},
                    ""nome"": ""{EscJs(p.NomePonto)}"",
                    ""endereco"": ""{EscJs(p.Cidade)}"",
                    ""tipos"": ""{EscJs(p.ResiduosAceitos)}"",
                    ""lat"": {latitude},
                    ""lng"": {longitude}
                }}");
            }
            marcadoresJs.Append("]");
            return marcadoresJs.ToString();
        }

        /// <summary>
        /// Compõe a string literal inteira do arquivo HTML5 que servirá como motor de mapas da aplicação.
        /// Integra a folha de estilos do Leaflet, scripts remotos e declara a comunicação bidirecional via objeto 'window.external'.
        /// </summary>
        /// <param name="pontos">Conjunto de pontos iniciais passados embutidos no carregamento imediato do HTML.</param>
        /// <returns>Documento HTML completo estruturado pronto para consumo pelo controle WebBrowser do WPF.</returns>
        private string GerarHtml(List<PontosColeta> pontos)
        {
            var marcadoresJson = GerarJsonMarcadores(pontos);

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta http-equiv='X-UA-Compatible' content='IE=edge' />
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <style>
        html, body, #map {{ height: 100%; margin: 0; padding: 0; font-family: sans-serif; }}
        .popup-custom {{ font-size: 14px; line-height: 1.4; min-width: 200px; }}
        .popup-title {{ font-weight: bold; color: #2e7d32; margin-bottom: 8px; font-size: 15px; text-transform: uppercase; }}
        .popup-info {{ margin-bottom: 4px; }}
    </style>
</head>
<body>
    <div id='map'></div>
    <script>
        // Cria a instância do mapa focada por padrão nas coordenadas geográficas centrais do Brasil
        var map = L.map('map').setView([-14.2350, -51.9253], 4);
        
        L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
            attribution: '&copy; OpenStreetMap contributors'
        }}).addTo(map);

        var marcadores = [];

        // Gerencia e plota dinamicamente os marcadores geográficos fornecidos
        function renderizarPontos(listaPontos) {{
            try {{
                if (typeof listaPontos === 'string') {{
                    listaPontos = JSON.parse(listaPontos);
                }}

                if (marcadores && marcadores.length > 0) {{
                    marcadores.forEach(function(m) {{ map.removeLayer(m); }});
                }}
                marcadores = [];

                if (listaPontos && listaPontos.length > 0) {{
                    listaPontos.forEach(function(p) {{
                        var marker = L.marker([p.lat, p.lng]).addTo(map);
                        var conteudo = '<div class=""popup-custom"">' +
                                       '<div class=""popup-title"">' + p.nome + '</div>' +
                                       '<div class=""popup-info""><b>Endereço:</b> ' + p.endereco + '</div>' +
                                       '<div class=""popup-info""><b>Resíduos:</b> ' + p.tipos + '</div>' +
                                       '</div>';

                        marker.bindPopup(conteudo);
                        marcadores.push(marker);
                    }});
                }}
            }} catch(e) {{ }}
        }}

        renderizarPontos({marcadoresJson});

        // Evento contínuo acionado pelo Leaflet no momento imediato em que qualquer deslocamento de mapa é encerrado
        map.on('moveend', function() {{
            try {{
                if (window.external && typeof window.external.NotificarMovimentoMapa !== 'undefined') {{
                    var bounds = map.getBounds();
                    var sw = bounds.getSouthWest();
                    var ne = bounds.getNorthEast();
                    // Interopera em tempo real chamando o método C# e passando o enquadramento geográfico delimitador
                    window.external.NotificarMovimentoMapa(sw.lat, sw.lng, ne.lat, ne.lng);
                }}
            }} catch(err) {{ }}
        }});

        // Direciona a câmera com zoom de proximidade para um ponto selecionado e abre seu respectivo popup descritivo
        function centralizarPonto(index) {{
            if (marcadores[index]) {{
                var m = marcadores[index];
                map.setView(m.getLatLng(), 16);
                m.openPopup();
            }}
        }}
    </script>
</body>
</html>";
        }

        /// <summary>
        /// Sanitiza cadeias de caracteres vindas do banco de dados, escapando aspas e caracteres especiais de controle.
        /// Impede que quebras de linhas ou caracteres inválidos rompam a execução da string JavaScript gerada.
        /// </summary>
        /// <param name="texto">A string crua a ser limpa.</param>
        /// <returns>String tratada e segura para injeção de comandos literais no script.</returns>
        private string EscJs(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            return texto.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
        }
    }
}