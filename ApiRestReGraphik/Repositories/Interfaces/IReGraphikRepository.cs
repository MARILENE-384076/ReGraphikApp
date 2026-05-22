namespace ApiRestReGraphik.Repositories.Interface
{
    /// <summary>
    /// Define os métodos para acessar e manipular os dados relacionados ao ReGraphik, 
    /// como listar, obter por ID, adicionar, atualizar e excluir resíduos.
    /// </summary>
    public interface IReGraphikRepository
    {
        Task<List<string>> GetAll();
        Task<string> GetById(int id);
        Task Add(string residuo);
        Task Update(string residuo);
        Task Delete(int id);
    }
}
