using ApiRestReGraphik.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Text.Json;

namespace ApiRestReGraphik.Services
{
    public class SugestaoResiduosService
    {
        /// <summary>
        /// Logger para registrar informações e erros relacionados ao serviço ReGraphik
        /// </summary>
        private readonly ILogger<SugestaoResiduosService> _logger;
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "sugestoes_residuos";
        private const string ResiduosNodeName = "residuos";
        private const string SugestoesNodeName = "sugestoes";

        /// <summary>
        ///  Construtor da classe SugestaoResiduosService que recebe as dependências necessárias, para permitir o registro de informações e erros durante a execução dos métodos do serviço.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros</param>
        /// <param name="configuration">Configuração para acessar as variáveis de ambiente</param>
        public SugestaoResiduosService(ILogger<SugestaoResiduosService> logger, IConfiguration configuration)
        {
            _logger = logger;
            var firebaseUrl = configuration["Firebase:RealtimeDatabaseUrl"];
            _firebaseClient = new FirebaseClient(firebaseUrl);
        }

        /// <summary>
        /// Lista todas as sugestões de resíduos cadastradas no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao listar os dados</exception>
        public async Task<List<SugestaoResiduo>> Listar()
        {
            try
            {
                /// Obtém todas as sugestões de resíduos do Firebase
                var sugestaoResiduos = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<SugestaoResiduo>();

                /// Garante que cada sugestão de resíduo tenha seu ID definido corretamente, 
                /// utilizando a chave do Firebase como ID quando necessário
                var listaSugestaoResiduos = sugestaoResiduos
                    .Select(r =>
                    {
                        var obj = r.Object;
                        if (obj != null && string.IsNullOrEmpty(obj.Id))
                        {
                            obj.Id = r.Key; /// Garante que o ID receba a chave do Firebase
                        }
                        return obj;
                    })
                    .Where(r => r != null)
                    .ToList();

                return listaSugestaoResiduos;
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, "Falha de comunicação com o Firebase ao carregar dados do ReGraphik sugestões de resíduos.");
                throw;
            }
            catch (ArgumentException ex)
            {
                /// Captura erros de argumento que podem ocorrer se os dados do Firebase 
                _logger.LogError(ex, "Erro de consistência de dados ao tentar agrupar sugestões de resíduos por ID.");
                throw new InvalidOperationException("Não foi possível processar a relação entre sugestões de resíduos devido a dados inconsistentes.", ex);
            }
            catch (JsonException ex)
            {
                /// Captura erros de desserialização que podem ocorrer se os dados armazenados no Firebase
                _logger.LogError(ex, "Erro de desserialização: Estrutura do nó de Sugestões de Resíduos é incompatível.");
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
        /// Obtém uma sugestão de resíduo específica por ID, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID da sugestão de resíduo a ser obtida</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao obter a sugestão de resíduo por ID</exception>
        public async Task<SugestaoResiduo> ObterPorId(string id)
        {
            try
            {
                /// Obtém a sugestão de resíduo do Firebase usando o ID fornecido
                var sugestaoResiduo = await _firebaseClient
                     .Child(NodeName)
                     .Child(id)
                     .OnceSingleAsync<SugestaoResiduo>();

                /// Garante que o ID da sugestão de resíduo seja definido corretamente,
                /// utilizando a chave do Firebase como ID quando necessário
                if (sugestaoResiduo != null)
                {
                    sugestaoResiduo.Id = id;
                }

                return sugestaoResiduo;
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro de infraestrutura no Firebase ao obter a sugestão de resíduo por ID: {id}");
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
                _logger.LogError(ex, $"Erro inesperado ao obter a sugestão de resíduo ID: {id}");
                throw;
            }

        }

        /// <summary>
        /// Adiciona uma sugestão de resíduo ao ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="sugestao">A sugestão de resíduo a ser adicionada</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao adicionar a sugestão de resíduo</exception>
        public async Task Criar(SugestaoResiduo sugestao)
        {
            try
            {
                if (string.IsNullOrEmpty(sugestao.Id))
                {
                    sugestao.Id = Guid.NewGuid().ToString(); // Garante que temos um ID único string
                }

                /// Adiciona a sugestão de resíduo ao Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(sugestao.Id)
                    .PutAsync(sugestao);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, "Erro no Firebase ao tentar criar nova sugestão de resíduo.");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, "Erro inesperado ao adicionar a sugestão de resíduo.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza uma sugestão de resíduo existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="sugestao">A sugestão de resíduo a ser atualizada</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao atualizar a sugestão de resíduo</exception>
        public async Task Atualizar(string id, SugestaoResiduo sugestao)
        {
            try
            {
                /// Garante que o ID da sugestão de resíduo seja definido corretamente para a atualização
                sugestao.Id = id;

                /// Atualiza a sugestão de resíduo no Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .PutAsync(sugestao);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar atualizar a sugestão de resíduo ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao atualizar a sugestão de resíduo ID: {id}");
                throw;
            }
        }
        
        /// <summary>
        /// Exclui uma sugestão de resíduo existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID da sugestão de resíduo a ser excluída</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao excluir a sugestão de resíduo</exception>
        public async Task Excluir(string id)
        {
            try
            {
                /// Exclui a sugestão de resíduo do Firebase usando o ID fornecido
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .DeleteAsync();
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar excluir a sugestão de resíduo ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao excluir a sugestão de resíduo ID: {id}");
                throw;
            }
        }
    }
}