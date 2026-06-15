using ReGraphik.Models;
using System.Threading.Tasks;

namespace ReGraphik.Services.Interface
{
    /// <summary>
    /// Interface para o serviço de autorização, que define os métodos necessários para cadastro, login e validação de usuários.
    /// </summary>
    public interface IAutorizarService
    {
        /// <summary>
        /// Envia os dados do usuário para iniciar o fluxo de cadastro e gerar um token.
        /// </summary>
        Task<CadastroResponse?> CadastrarAsync(string nome, string cpf, string email, string login, string senha);

        /// <summary>
        /// Realiza a autenticação direta do usuário por login e senha.
        /// </summary>
        Task<Usuario?> LoginAsync(string login, string senha);

        /// <summary>
        /// Envia o token para validação final de segurança do cadastro.
        /// </summary>
        Task<bool> ValidarTokenAsync(string email, string token);

        /// <summary>
        /// Atualiza os dados de um usuário existente na API baseado no ID.
        /// </summary>
        Task<bool> AtualizarAsync(string id, object usuario);
    }
}