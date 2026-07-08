using ReGraphik.Models;

namespace ReGraphik.Services.Interface
{
    /// <summary>
    /// Interface que define os métodos para o serviço de autorização, incluindo solicitações de acesso, 
    /// finalização de cadastro, login, validação de token e atualização de informações do usuário.
    /// </summary>
    public interface IAutorizarService
    {
        Task<bool> SolicitarAcessoAsync(string email);

        Task<bool> FinalizarCadastroAsync(
            string nome, string cpf, string email,
            string login, string senha, string token, string perfil);

        Task<Usuario?> LoginAsync(string login, string senha);

        Task<bool> ValidarTokenAsync(string email, string token);

        Task<bool> AtualizarAsync(string id, object usuario);

        Task<string?> AtualizarComFotoAsync(string id, Usuario usuario, string caminhoFoto);

        Task<string> RecuperarSenhaAsync(string email);
    }
}