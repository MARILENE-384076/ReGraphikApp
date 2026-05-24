using ApiRestReGraphik.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace ApiRestReGraphik.Services
{
    public class SugestaoService
    {
        // Logger para registrar informações e erros relacionados ao serviço ReGraphik
        private readonly ILogger<SugestaoService> _logger;
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "sugestoes";

        /// <summary>
        ///  Construtor da classe SugestaoService que recebe as dependências necessárias, para permitir o registro de informações e erros durante a execução dos métodos do serviço.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros</param>
        public SugestaoService(ILogger<SugestaoService> logger)
        {
            _logger = logger;
            _firebaseClient = new FirebaseClient("https://regraphikfirebase-default-rtdb.firebaseio.com/");
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
                // Obtém os dados do Firebase para a coleção de sugestões
                var sugestao = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<Sugestao>();

                // Mapeia os dados do Firebase para a lista de sugestões
                return sugestao.Select(r => r.Object).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao listar os dados do ReGraphik: {ex.Message}");
                throw new Exception("Erro ao listar os dados do ReGraphik");
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
                // Obtém a sugestão do Firebase usando o ID fornecido
                var sugestao = await _firebaseClient
                     .Child(NodeName)
                     .Child(id)
                     .OnceSingleAsync<Sugestao>();

                return sugestao;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao obter a sugestão por ID: {ex.Message}");
                throw new Exception("Erro ao obter a sugestão por ID");
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
                    sugestao.Id = Guid.NewGuid().ToString(); // Garante que temos um ID único string
                }

                // Adiciona a sugestão ao Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(sugestao.Id)
                    .PutAsync(sugestao);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao adicionar a sugestão: {ex.Message}");
                throw new Exception("Erro ao adicionar a sugestão");
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
                // Garante que o ID da sugestão seja definido corretamente para a atualização
                sugestao.Id = id;

                // Atualiza a sugestão no Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .PutAsync(sugestao);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao atualizar a sugestão: {ex.Message}");
                throw new Exception("Erro ao atualizar a sugestão");
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
                // Exclui a sugestão do Firebase usando o ID fornecido
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao excluir a sugestão: {ex.Message}");
                throw new Exception("Erro ao excluir a sugestão");
            }
        }
    }
}