using ReGraphik.Models;

namespace ReGraphik.Services.Interface
{
    public interface IAutorizarService
    {
        Task<bool> SolicitarAcessoAsync(string email);

        Task<bool> FinalizarCadastroAsync(
            string nome, string cpf, string email,
            string login, string senha, string token);

        Task<Usuario?> LoginAsync(string login, string senha);

        Task<bool> ValidarTokenAsync(string email, string token);

        Task<bool> AtualizarAsync(string id, object usuario);
    }
}