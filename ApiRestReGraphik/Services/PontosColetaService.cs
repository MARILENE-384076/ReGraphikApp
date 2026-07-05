using ApiRestReGraphik.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Google.Apis.Auth.OAuth2;
using System.Text.Json;

namespace ApiRestReGraphik.Services
{
    public class PontosColetaService
    {
        private readonly FirebaseClient _firebaseClient;
        private readonly ILogger<PontosColetaService> _logger;
        private readonly IConfiguration _configuration;
        private const string NodeName = "pontos_coleta";

        /// <summary>
        /// Construtor da classe PontosColetaService, responsável por inicializar o cliente do Firebase e o logger para a classe,
        /// permitindo a comunicação com o Realtime Database do Firebase e o registro de logs para monitoramento e depuração.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros.</param>
        /// <param name="configuration">Configurações da aplicação, incluindo credenciais do Firebase e valores padrão de sincronização.</param>
        public PontosColetaService(ILogger<PontosColetaService> logger, IConfiguration configuration)
        {
            _logger        = logger;
            _configuration = configuration;

            var dbUrl = configuration["Firebase:RealtimeDatabaseUrl"];
            var credentialsFileName = configuration["Firebase:CredentialFilePath"] ?? "ReGraphikFirebaseKey.json";

            if (string.IsNullOrEmpty(dbUrl))
            {
                _logger.LogError("Erro crítico: URL do Realtime Database não encontrada no appsettings.json");
                throw new Exception("Configurações do Firebase ausentes.");
            }

            try
            {
                /// Obtém o caminho físico correto onde a API está rodando no servidor
                var caminhoBase = AppContext.BaseDirectory;
                var caminhoCompletoChave = Path.Combine(caminhoBase, credentialsFileName);

                if (!File.Exists(caminhoCompletoChave))
                {
                    _logger.LogError($"Arquivo de credenciais não encontrado em: {caminhoCompletoChave}");
                    throw new FileNotFoundException($"O arquivo {credentialsFileName} precisa estar na raiz da API.");
                }

                /// Inicializa a credencial usando diretamente o arquivo .json do Firebase
                GoogleCredential credenciais;
                using (var stream = new FileStream(caminhoCompletoChave, FileMode.Open, FileAccess.Read))
                {
                    credenciais = GoogleCredential.FromStream(stream)
                        .CreateScoped(new[] {
                    "https://www.googleapis.com/auth/userinfo.email",
                    "https://www.googleapis.com/auth/firebase.database"
                        });
                }

                _firebaseClient = new FirebaseClient(
                    dbUrl,
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = async () =>
                        {
                            /// Obtém o token de acesso de forma assíncrona e segura
                            var token = await credenciais.UnderlyingCredential.GetAccessTokenForRequestAsync();
                            return token;
                        }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Falha fatal ao inicializar o FirebaseService: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// Lista todos os pontos de coleta cadastrados no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao listar os dados</exception>
        public async Task<List<PontosColeta>> Listar()
        {
            try
            {
                /// Obtém os pontos de coleta do Firebase
                var pontos = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<PontosColeta>();

                /// Mapeia os dados do Firebase para a lista de PontosColeta
                return pontos.Select(p => p.Object).ToList();
            }
            catch (FirebaseException ex)
            {
                /// Erro específico relacionado ao Firebase, como problemas de autenticação ou comunicação
                _logger.LogError(ex, "Erro de comunicação ou permissão no Firebase ao listar pontos de coleta.");
                throw;
            }
            catch (JsonException ex)
            {
                /// Erro de desserialização, indicando que os dados no Firebase estão em um formato inesperado ou corrompido
                _logger.LogError(ex, "Erro de desserialização. Os dados no Firebase estão em formato inválido.");
                throw new InvalidOperationException("Os dados recuperados do banco estão corrompidos.", ex);
            }
            catch (Exception ex)
            {
                /// Qualquer outro erro inesperado que possa ocorrer durante a operação de listagem
                _logger.LogError(ex, "Erro inesperado na camada de serviço ao listar pontos de coleta.");
                throw;
            }
        }

        /// <summary>
        /// Obtém um ponto de coleta específico por ID, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID do ponto de coleta a ser obtido</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao obter o ponto de coleta por ID</exception>
        public async Task<PontosColeta> ObterPorId(string id)
        {
            try
            {
                /// Obtém o ponto de coleta do Firebase usando o ID fornecido
                var ponto = await _firebaseClient
                     .Child(NodeName)
                     .Child(id)
                     .OnceSingleAsync<PontosColeta>();

                return ponto;
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro de infraestrutura no Firebase ao obter o ponto de coleta por ID: {id}");
                throw;
            }
            catch (JsonException ex)
            {
                /// Captura erros de desserialização que podem ocorrer se os dados armazenados no Firebase estiverem em um formato inesperado ou corrompido
                _logger.LogError(ex, $"Erro de desserialização. Os nós relacionados ao ID {id} possuem dados inválidos.");
                throw new InvalidOperationException("Os dados obtidos do Firebase estão corrompidos ou em formato inválido.", ex);
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao obter o ponto de coleta por ID: {id}");
                throw;
            }

        }

        /// <summary>
        /// Adiciona um novo ponto de coleta ao ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="pontosColeta">O ponto de coleta a ser adicionado</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao adicionar o ponto de coleta</exception>
        public async Task Criar(PontosColeta pontosColeta)
        {
            try
            {
                pontosColeta.Id = null;

                /// Adiciona o ponto de coleta ao Firebase e obtém o resultado, que inclui a chave gerada para o novo ponto de coleta
                var resultado = await _firebaseClient
                                .Child(NodeName)
                                .PostAsync(pontosColeta);

                /// Atribui o ID gerado pelo Firebase ao ponto de coleta
                pontosColeta.Id = resultado.Key;

                await _firebaseClient
            .Child(NodeName)
            .Child(resultado.Key)
            .PutAsync(pontosColeta);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, "Erro no Firebase ao tentar criar novo ponto de coleta.");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, "Erro inesperado ao adicionar o ponto de coleta.");
                throw;
            }
        }

        /// <summary>
        ///  Sincroniza os pontos de coleta do ReGraphik com os dados do Google Maps, utilizando o repositório para acessar os dados e registrando.
        /// </summary>
        /// <param name="cidade">Nome da cidade para a qual os pontos de coleta serão sincronizados</param>
        /// <param name="apiKey">Chave da API do Google Maps</param>
        /// <param name="httpClient">Instância de HttpClient para realizar chamadas HTTP</param>
        /// <returns></returns>
        public async Task<(int salvos, int ignorados)> SincronizarComGoogleMapsAsync(string cidade, string apiKey, HttpClient httpClient)
        {
            try
            {
                /// Obtém os pontos de coleta existentes no banco de dados para a cidade especificada, garantindo que tenhamos uma lista atualizada para comparação
                var pontosNoBanco = (await Listar())?.ToList() ?? new List<PontosColeta>();

                /// Cria um HashSet para armazenar as coordenadas existentes, permitindo uma busca ultra rápida
                var coordenadasExistentes = new HashSet<(double, double)>(
                    pontosNoBanco.Select(p => (p.Lat, p.Lng))
                );

                /// Monta a URL de consulta para a API do Google Maps, utilizando o nome da cidade e a chave da API
                var query = Uri.EscapeDataString($"ponto de coleta reciclagem {cidade}");
                var url = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={query}&key={apiKey}";

                /// Faz a chamada para a API do Google Maps e obtém a resposta JSON
                var json = await httpClient.GetStringAsync(url);
                using var doc = System.Text.Json.JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("status", out var statusProp))
                {
                    string statusApi = statusProp.GetString() ?? "";
                    if (statusApi != "OK" && statusApi != "ZERO_RESULTS")
                    {
                        _logger.LogWarning($"API do Google Maps retornou um status de erro: {statusApi}");
                        return (0, 0);
                    }
                }

                if (!doc.RootElement.TryGetProperty("results", out var results))
                {
                    return (0, 0);
                }

                int totalSalvo = 0;
                int totalIgnorado = 0;

                foreach (var item in results.EnumerateArray())
                {
                    var nome = item.TryGetProperty("name", out var n) ? n.GetString() : "Sem nome";

                    /// O Google pode retornar resultados sem coordenadas, então precisamos verificar isso antes de tentar acessar os valores
                    double lat = 0, lng = 0;
                    if (item.TryGetProperty("geometry", out var geo) && geo.TryGetProperty("location", out var loc))
                    {
                        lat = loc.TryGetProperty("lat", out var la) ? la.GetDouble() : 0;
                        lng = loc.TryGetProperty("lng", out var ln) ? ln.GetDouble() : 0;
                    }

                    /// Verifica se as coordenadas já existem no banco de dados usando o HashSet, o que é extremamente rápido
                    if (coordenadasExistentes.Contains((lat, lng)))
                    {
                        totalIgnorado++;
                        continue;
                    }

                    var novoPonto = new PontosColeta
                    {
                        NomePonto     = nome,
                        Cidade        = cidade,
                        Estado          = _configuration["Sincronizacao:EstadoPadrao"] ?? "BR",
                        CEP               = _configuration["Sincronizacao:CepPadrao"]    ?? "—",
                        ResiduosAceitos = _configuration["Sincronizacao:ResiduosAceitos"] ?? "Reciclável",
                        Lat = lat,
                        Lng = lng
                    };

                    await Criar(novoPonto);

                    /// Adiciona as novas coordenadas ao HashSet para garantir que não sejam adicionadas novamente
                    coordenadasExistentes.Add((lat, lng));
                    totalSalvo++;
                }

                return (totalSalvo, totalIgnorado);
            }
            catch (FirebaseException ex)
            {
                /// Erro específico relacionado ao Firebase, como problemas de autenticação ou comunicação
                _logger.LogError(ex, "Erro de comunicação ou permissão no Firebase ao buscar pontos de coleta.");
                throw;
            }
            catch (JsonException ex)
            {
                /// Erro de desserialização, indicando que os dados no Firebase estão em um formato inesperado ou corrompido
                _logger.LogError(ex, "Erro de desserialização. Os dados no Firebase estão em formato inválido.");
                throw new InvalidOperationException("Os dados recuperados do banco estão corrompidos.", ex);
            }
            catch (Exception ex)
            {
                /// Qualquer outro erro inesperado que possa ocorrer durante a operação de busca
                _logger.LogError(ex, "Erro inesperado na camada de serviço ao buscar pontos de coleta.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um ponto de coleta existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="pontosColeta">O ponto de coleta a ser atualizado</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao atualizar o ponto de coleta</exception>
        public async Task Atualizar(string id, PontosColeta pontosColeta)
        {
            try
            {
                pontosColeta.Id = id;

                /// Atualiza o ponto de coleta no Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .PutAsync(pontosColeta);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar atualizar o ponto de coleta ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao atualizar o ponto de coleta ID: {id}");
                throw;
            }
        }
        
        /// <summary>
        /// Exclui um ponto de coleta existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID do ponto de coleta a ser excluído</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao excluir o ponto de coleta</exception>
        public async Task Excluir(string id)
        {
            try
            {
                /// Exclui o ponto de coleta do Firebase usando o ID fornecido
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .DeleteAsync();
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar excluir o ponto de coleta ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao excluir o ponto de coleta ID: {id}");
                throw;
            }
        }
    }
}