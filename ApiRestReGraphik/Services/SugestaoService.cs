using ApiRestReGraphik.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Text.Json;

namespace ApiRestReGraphik.Services
{
    public class SugestaoService
    {
        /// Logger para registrar informações e erros relacionados ao serviço ReGraphik
        private readonly ILogger<SugestaoService> _logger;
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "sugestoes";

        /// <summary>
        ///  Construtor da classe SugestaoService que recebe as dependências necessárias, para permitir o registro de informações e erros durante a execução dos métodos do serviço.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros</param>
        /// <param name="configuration">Configuração para acessar as variáveis de ambiente</param>
        public SugestaoService(ILogger<SugestaoService> logger, IConfiguration configuration)
        {
            _logger = logger;
            var firebaseUrl = configuration["Firebase:RealtimeDatabaseUrl"];
            _firebaseClient = new FirebaseClient(firebaseUrl);
        }

        /// <summary>
        /// Lista todas as sugestões cadastradas no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao listar os dados</exception>
        public async Task<List<Sugestao>> Listar()
        {
            try
            {
                /// Obtém os dados do Firebase para a coleção de sugestões
                var sugestao = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<Sugestao>();

                /// Mapeia os dados do Firebase para a lista de sugestões
                return sugestao.Select(r => r.Object).ToList();
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, "Falha de comunicação com o Firebase ao carregar dados do ReGraphik sugestões.");
                throw;
            }
            catch (ArgumentException ex)
            {
                /// Captura erros de argumento que podem ocorrer se os dados do Firebase 
                _logger.LogError(ex, "Erro de consistência de dados ao tentar agrupar sugestões por ID.");
                throw new InvalidOperationException("Não foi possível processar a relação entre sugestões devido a dados inconsistentes.", ex);
            }
            catch (JsonException ex)
            {
                /// Captura erros de desserialização que podem ocorrer se os dados armazenados no Firebase
                _logger.LogError(ex, "Erro de desserialização: Estrutura do nó de Sugestões é incompatível.");
                throw new InvalidOperationException("Os dados armazenados no Firebase possuem um formato inválido.", ex);
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, "Erro crítico e não mapeado no serviço ReGraphik.");
                throw;
            }
        }

        /// <summary>
        /// Obtém uma sugestão específica por ID, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID da sugestão a ser obtida</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao obter a sugestão por ID</exception>
        public async Task<Sugestao> ObterPorId(string id)
        {
            try
            {
                /// Obtém a sugestão do Firebase usando o ID fornecido
                var sugestao = await _firebaseClient
                     .Child(NodeName)
                     .Child(id)
                     .OnceSingleAsync<Sugestao>();

                return sugestao;
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro de infraestrutura no Firebase ao obter a sugestão por ID: {id}");
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
                _logger.LogError(ex, $"Erro inesperado ao obter a sugestão por ID: {id}");
                throw;
            }

        }

        /// <summary>
        /// Adiciona uma sugestão ao ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="residuo">O resíduo a ser adicionado</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao adicionar a sugestão</exception>
        public async Task Criar(Sugestao sugestao)
        {
            try
            {
                if (string.IsNullOrEmpty(sugestao.Id))
                {
                    sugestao.Id = Guid.NewGuid().ToString(); /// Garante que temos um ID único string
                }

                /// Adiciona a sugestão ao Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(sugestao.Id)
                    .PutAsync(sugestao);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, "Erro no Firebase ao tentar criar nova sugestão.");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, "Erro inesperado ao adicionar a sugestão.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza uma sugestão existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="sugestao">A sugestão a ser atualizada</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao atualizar a sugestão</exception>
        public async Task Atualizar(string id, Sugestao sugestao)
        {
            try
            {
                /// Garante que o ID da sugestão seja definido corretamente para a atualização
                sugestao.Id = id;

                /// Atualiza a sugestão no Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .PutAsync(sugestao);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar atualizar a sugestão ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao atualizar a sugestão ID: {id}");
                throw;
            }
        }
        
        /// <summary>
        /// Exclui uma sugestão existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID da sugestão a ser excluída</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao excluir a sugestão</exception>
        public async Task Excluir(string id)
        {
            try
            {
                /// Exclui a sugestão do Firebase usando o ID fornecido
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .DeleteAsync();
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar excluir a sugestão ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao excluir a sugestão ID: {id}");
                throw;
            }
        }
    }
}