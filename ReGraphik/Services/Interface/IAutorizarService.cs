using ReGraphik.Models;

namespace ReGraphik.Services.Interface
{
    // Interface para o serviço de autorização, que define os métodos necessários para cadastro e login de usuários.
    public interface IAutorizarService
    {
        Task<bool> CadastrarAsync(object usuario);
        Task<Usuario?> LoginAsync(string login, string senha);
    }
}