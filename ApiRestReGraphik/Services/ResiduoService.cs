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
        private const string UsersNodeName = "usuarios";

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
                // Obtém os dados do Firebase para a coleção de resíduos
                var residuosFirebase = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<Residuo>();

                // Converte os dados do Firebase para uma lista de objetos Residuo, garantindo que o ID seja atribuído corretamente a partir da chave do Firebase
                var listaResiduos = residuosFirebase.Select(r =>
                {
                    var residuo = r.Object;
                    if (residuo != null && string.IsNullOrEmpty(residuo.Id))
                    {
                        residuo.Id = r.Key; 
                    }
                    return residuo;
                }).Where(r => r != null).ToList();

                // Obtém os dados do Firebase para a coleção de usuários (caso seja necessário para mapear os resíduos aos usuários)
                var usuariosFirebase = await _firebaseClient
                    .Child(UsersNodeName)
                    .OnceAsync<Usuario>();

                // Cria um dicionário para mapear os usuários pelo ID, facilitando a associação dos resíduos aos seus respectivos usuários
                var dicionarioUsuarios = usuariosFirebase.ToDictionary(u => u.Key, u => u.Object);

                foreach (var residuo in listaResiduos)
                {
                    // Se o resíduo tiver um ID de usuário válido, associa o objeto de usuário correspondente ao resíduo usando o dicionário criado
                    if (!string.IsNullOrEmpty(residuo.IdUsuario) && dicionarioUsuarios.ContainsKey(residuo.IdUsuario))
                    {
                        residuo.Usuario = dicionarioUsuarios[residuo.IdUsuario];
                    }
                }

                return listaResiduos;
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

                // Se o resíduo for encontrado, tenta obter o usuário associado a ele (caso haja um ID de usuário válido)
                if (residuos != null && !string.IsNullOrEmpty(residuos.IdUsuario))
                {
                    residuos.Id = id; // Garante que o ID do resíduo seja definido corretamente a partir da chave do Firebase

                    if (!string.IsNullOrEmpty(residuos.IdUsuario))
                    {
                        var usuario = await _firebaseClient
                            .Child(UsersNodeName)
                            .Child(residuos.IdUsuario)
                            .OnceSingleAsync<Usuario>();

                        residuos.Usuario = usuario;
                    }
                }

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
                throw new Exception("Erro ao adicionar o resíduo", ex);
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