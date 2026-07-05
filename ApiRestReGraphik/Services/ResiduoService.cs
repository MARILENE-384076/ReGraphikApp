using ApiRestReGraphik.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Text.Json;

namespace ApiRestReGraphik.Services
{
    public class ResiduoService
    {
        /// <summary>
        /// Logger para registrar informações e erros relacionados ao serviço ReGraphik
        /// </summary>
        private readonly ILogger<ResiduoService> _logger;
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "residuos";

        /// <summary>
        ///  Construtor da classe ResiduoService que recebe as dependências necessárias, para permitir o registro de informações e erros durante a execução dos métodos do serviço.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros</param>
        /// <param name="configuration">Configuração para acessar as variáveis de ambiente</param>
        public ResiduoService(ILogger<ResiduoService> logger, IConfiguration configuration) 
        {
            _logger = logger;

            var firebaseUrl = configuration["Firebase:RealtimeDatabaseUrl"];
            _firebaseClient = new FirebaseClient(firebaseUrl);
        }

        /// <summary>
        /// Lista todos os resíduos cadastrados no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao listar os dados</exception>
        public async Task<List<Residuo>> Listar()
        {
            try
            {
                var residuosFirebase = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<Residuo>();

                /// Mapeia os dados do Firebase para a lista de resíduos, garantindo que o ID seja definido corretamente
                return residuosFirebase.Select(r =>
                {
                    var model = r.Object;
                    if (model != null)
                    {
                        model.Id = r.Key;
                    }
                    return model;
                }).Where(m => m != null).ToList()!;
            }
            /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
            catch (FirebaseException ex)
            {
                _logger.LogError(ex, "Falha de comunicação com o Firebase ao carregar dados de Resíduos.");
                throw;
            }
            /// Captura erros de consistência de dados que podem ocorrer se os dados armazenados no Firebase
            /// estiverem em um formato inesperado ou corrompido
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Erro de consistência de dados ao tentar mapear resíduos.");
                throw new InvalidOperationException("Não foi possível processar a lista de resíduos devido a dados inconsistentes.", ex);
            }
            /// Captura erros de desserialização que podem ocorrer se os dados armazenados no Firebase
            /// estiverem em um formato inesperado ou corrompido
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro de desserialização: Estrutura do nó de Resíduos é incompatível.");
                throw new InvalidOperationException("Os dados armazenados no Firebase possuem um formato inválido.", ex);
            }
            /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico e não mapeado no serviço de Resíduos.");
                throw;
            }
        }

        /// <summary>
        /// Obtém um resíduo específico por ID, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID do resíduo a ser obtido</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao obter o resíduo por ID</exception>
        public async Task<Residuo> ObterPorId(string id)
        {
            try
            {
                /// Obtém o resíduo do Firebase usando o ID fornecido
                var residuo = await _firebaseClient
                     .Child(NodeName)
                     .Child(id)
                     .OnceSingleAsync<Residuo>();

                /// Garante que o ID do resíduo seja definido corretamente, caso o objeto retornado seja nulo
                if (residuo != null)
                {
                    residuo.Id = id; 
                }

                return residuo;
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro de infraestrutura no Firebase ao obter o resíduo por ID: {id}");
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
                _logger.LogError(ex, $"Erro inesperado ao obter o resíduo por ID: {id}");
                throw;
            }

        }

        /// <summary>
        /// Adiciona um novo resíduo ao ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="residuo">O resíduo a ser adicionado</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao adicionar o resíduo</exception>
        public async Task Criar(Residuo residuo)
        {
            try
            {
                if (string.IsNullOrEmpty(residuo.Id))
                {
                    residuo.Id = Guid.NewGuid().ToString(); /// Garante que temos um ID único string
                }

                /// Adiciona o resíduo ao Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(residuo.Id)
                    .PutAsync(residuo);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, "Erro no Firebase ao tentar criar novo resíduo.");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, "Erro inesperado ao adicionar o resíduo.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um resíduo existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="residuo">O resíduo a ser atualizado</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao atualizar o resíduo</exception>
        public async Task Atualizar(string id, Residuo residuo)
        {
            try
            {
                residuo.Anexo = null;
                /// Garante que o ID do resíduo seja definido corretamente para a atualização
                residuo.Id = id;

                /// Atualiza o resíduo no Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .PutAsync(residuo);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar atualizar o resíduo ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao atualizar o resíduo ID: {id}");
                throw;
            }
        }
        
        /// <summary>
        /// Exclui um resíduo existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID do resíduo a ser excluído</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao excluir o resíduo</exception>
        public async Task Excluir(string id)
        {
            try
            {
                /// Exclui o resíduo do Firebase usando o ID fornecido
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .DeleteAsync();
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar excluir o resíduo ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao excluir o resíduo ID: {id}");
                throw;
            }
        }
    }
}