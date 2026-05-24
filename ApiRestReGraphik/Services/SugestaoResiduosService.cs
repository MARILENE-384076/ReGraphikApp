using ApiRestReGraphik.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace ApiRestReGraphik.Services
{
    public class SugestaoResiduosService
    {
        // Logger para registrar informações e erros relacionados ao serviço ReGraphik
        private readonly ILogger<SugestaoResiduosService> _logger;
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "sugestoes_residuos";

        /// <summary>
        ///  Construtor da classe SugestaoResiduosService que recebe as dependências necessárias, para permitir o registro de informações e erros durante a execução dos métodos do serviço.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros</param>
        public SugestaoResiduosService(ILogger<SugestaoResiduosService> logger)
        {
            _logger = logger;
            _firebaseClient = new FirebaseClient("https://regraphikfirebase-default-rtdb.firebaseio.com/")  ;
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
                // Obtém os dados do Firebase para a coleção de sugestões de resíduos
                var sugestaoResiduos = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<SugestaoResiduo>();

                // Mapeia os dados do Firebase para a lista de sugestões de resíduos
                return sugestaoResiduos.Select(r => r.Object).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao listar os dados do ReGraphik: {ex.Message}");
                throw new Exception("Erro ao listar os dados do ReGraphik");
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
                // Obtém a sugestão de resíduo do Firebase usando o ID fornecido
                var sugestaoResiduo = await _firebaseClient
                     .Child(NodeName)
                     .Child(id)
                     .OnceSingleAsync<SugestaoResiduo>();

                return sugestaoResiduo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao obter a sugestão de resíduo por ID: {ex.Message}");
                throw new Exception("Erro ao obter a sugestão de resíduo por ID");
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

                // Adiciona a sugestão de resíduo ao Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(sugestao.Id)
                    .PutAsync(sugestao);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao adicionar a sugestão de resíduo: {ex.Message}");
                throw new Exception("Erro ao adicionar a sugestão de resíduo");
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
                // Garante que o ID da sugestão de resíduo seja definido corretamente para a atualização
                sugestao.Id = id;

                // Atualiza a sugestão de resíduo no Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .PutAsync(sugestao);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao atualizar a sugestão de resíduo: {ex.Message}");
                throw new Exception("Erro ao atualizar a sugestão de resíduo");
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
                // Exclui a sugestão de resíduo do Firebase usando o ID fornecido
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao excluir a sugestão de resíduo: {ex.Message}");
                throw new Exception("Erro ao excluir a sugestão de resíduo");
            }
        }
    }
}