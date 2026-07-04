using ApiRestReGraphik.Models;
using Firebase.Database;
using Firebase.Database.Query;
using System.Text.Json;

namespace ApiRestReGraphik.Services
{
    public class UsuarioService
    {
        /// <summary>
        /// Logger para registrar informações e erros relacionados ao serviço ReGraphik
        /// </summary>
        private readonly ILogger<UsuarioService> _logger;
        private readonly FirebaseClient _firebaseClient;
        private const string NodeName = "usuarios";

        /// <summary>
        ///  Construtor da classe UsuarioService que recebe as dependências necessárias, para permitir o registro de informações e erros durante a execução dos métodos do serviço.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros</param>
        /// <param name="configuration">Configuração para acessar as variáveis de ambiente</param>
        public UsuarioService(ILogger<UsuarioService> logger, IConfiguration configuration) 
        {
            _logger = logger;
            var firebaseUrl = configuration["Firebase:RealtimeDatabaseUrl"];
            _firebaseClient = new FirebaseClient(firebaseUrl);
        }

        /// <summary>
        /// Lista todos os usuários cadastrados no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao listar os dados</exception>
        public async Task<List<Usuario>> Listar()
        {
            try
            {
                /// Obtém os dados do Firebase para a coleção de usuários
                var usuarios = await _firebaseClient
                    .Child(NodeName)
                    .OnceAsync<Usuario>();

                /// Mapeia os dados do Firebase para a lista de usuários
                return usuarios.Select(r => r.Object).ToList();
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, "Falha de comunicação com o Firebase ao carregar dados do ReGraphik usuários.");
                throw;
            }
            catch (ArgumentException ex)
            {
                /// Captura erros de argumento que podem ocorrer se os dados do Firebase 
                _logger.LogError(ex, "Erro de consistência de dados ao tentar agrupar usuários por ID.");
                throw new InvalidOperationException("Não foi possível processar a relação entre usuários devido a dados inconsistentes.", ex);
            }
            catch (JsonException ex)
            {
                /// Captura erros de desserialização que podem ocorrer se os dados armazenados no Firebase
                _logger.LogError(ex, "Erro de desserialização: Estrutura do nó de Usuários é incompatível.");
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
        /// Obtém um usuário específico por ID, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID do usuário a ser obtido</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao obter o usuário por ID</exception>
        public async Task<Usuario> ObterPorId(string id)
        {
            try
            {
                /// Obtém um usuário do Firebase usando o ID fornecido
                var usuario = await _firebaseClient
                     .Child(NodeName)
                     .Child(id)
                     .OnceSingleAsync<Usuario>();   

                return usuario;
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro de infraestrutura no Firebase ao obter o usuário por ID: {id}");
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
                _logger.LogError(ex, $"Erro inesperado ao obter o usuário por ID: {id}");
                throw;
            }

        }

        /// <summary>
        /// Adiciona um novo usuário ao ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="usuario">O usuário a ser adicionado</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao adicionar o usuário</exception>
        public async Task Criar(Usuario usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario.Id))
                {
                    usuario.Id = Guid.NewGuid().ToString(); /// Garante que temos um ID único string
                }

                /// Adiciona o usuário ao Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(usuario.Id)
                    .PutAsync(usuario);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, "Erro no Firebase ao tentar criar novo usuário.");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, "Erro inesperado ao adicionar o usuário.");
                throw;
            }
        }

        /// <summary>
        /// Atualiza um usuário existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="usuario">O usuário a ser atualizado</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao atualizar o usuário</exception>
        public async Task Atualizar(string id, Usuario usuario)
        {
            try
            {
                /// Garante que o ID do usuário seja definido corretamente para a atualização
                usuario.Id = id;

                /// Atualiza o usuário no Firebase usando o ID como chave
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .PutAsync(usuario);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar atualizar o usuário ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao atualizar o usuário ID: {id}");
                throw;
            }
        }
        
        /// <summary>
        /// Exclui um usuário existente no ReGraphik, utilizando o repositório para acessar os dados e registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="id">ID do usuário a ser excluído</param>
        /// <returns></returns>
        /// <exception cref="Exception">Lançada quando ocorre um erro ao excluir o usuário</exception>
        public async Task Excluir(string id)
        {
            try
            {
                /// Exclui o usuário do Firebase usando o ID fornecido
                await _firebaseClient
                    .Child(NodeName)
                    .Child(id)
                    .DeleteAsync();
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar excluir o usuário ID: {id}");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao excluir o usuário ID: {id}");
                throw;
            }
        }

        /// <summary>
        /// Autentica um usuário com base no login e senha fornecidos, utilizando o repositório para acessar os dados e 
        /// registrando qualquer erro que possa ocorrer durante a operação.
        /// </summary>
        /// <param name="login">Login do usuário a ser autenticado.</param>
        /// <param name="senha">Senha do usuário a ser autenticado.</param>
        /// <returns>/returns>
        public async Task<Usuario?> Autenticar(string login, string senha)
        {
            try
            {
                /// Obtém os dados do Firebase para a coleção de usuários
                var usuarios = await _firebaseClient
                .Child(NodeName)
                .OnceAsync<Usuario>();

                return usuarios
                    .Select(r => r.Object)
                    .FirstOrDefault(u => u.Login == login && u.Senha == senha);
            }
            catch (FirebaseException ex)
            {
                /// Captura erros específicos relacionados à comunicação com o Firebase, como falhas de conexão ou erros de autenticação
                _logger.LogError(ex, $"Erro no Firebase ao tentar autenticar o usuário.");
                throw;
            }
            catch (Exception ex)
            {
                /// Captura qualquer outro tipo de exceção não mapeada e registra um erro crítico
                _logger.LogError(ex, $"Erro inesperado ao autenticar o usuário.");
                throw;
            }
        }
    }
}