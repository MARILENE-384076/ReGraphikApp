using ApiRestReGraphik.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace ApiRestReGraphik.Services
{
    public class ResiduoService
    {
        // Logger para registrar informações e erros relacionados ao serviço ReGraphik
        private readonly ILogger<ResiduoService> _logger;
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "residuos";

        /// <summary>
        ///  Construtor da classe ResiduoService que recebe as dependências necessárias, para permitir o registro de informações e erros durante a execução dos métodos do serviço.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros</param>
        public ResiduoService(ILogger<ResiduoService> logger)
        {
            _logger = logger;
            _firebaseClient = new FirebaseClient("https://regraphikfirebase-default-rtdb.firebaseio.com/");
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
                // Obtém os dados do Firebase para a coleção de resíduos
                var residuos = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<Residuo>();

                // Mapeia os dados do Firebase para a lista de Residuos
                return residuos.Select(r => r.Object).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao listar os dados do ReGraphik: {ex.Message}");
                throw new Exception("Erro ao listar os dados do ReGraphik");
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
                // Obtém o resíduo do Firebase usando o ID fornecido
                var residuos = await _firebaseClient
                     .Child(NodeName)
                     .Child(id)
                     .OnceSingleAsync<Residuo>();

                return residuos;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao obter o resíduo por ID: {ex.Message}");
                throw new Exception("Erro ao obter o resíduo por ID");
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
                    residuo.Id = Guid.NewGuid().ToString(); // Garante que temos um ID único string
                }

                // Adiciona o resíduo ao Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(residuo.Id)
                    .PutAsync(residuo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao adicionar o resíduo: {ex.Message}");
                throw new Exception("Erro ao adicionar o resíduo");
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
                // Garante que o ID do resíduo seja definido corretamente para a atualização
                residuo.Id = id;

                // Atualiza o resíduo no Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .PutAsync(residuo);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao atualizar o resíduo: {ex.Message}");
                throw new Exception("Erro ao atualizar o resíduo");
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
                // Exclui o resíduo do Firebase usando o ID fornecido
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao excluir o resíduo: {ex.Message}");
                throw new Exception("Erro ao excluir o resíduo");
            }
        }
    }
}