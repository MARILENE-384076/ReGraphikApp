namespace ApiRestReGraphik.Models
{
    /// <summary>
    /// Classe que representa a estrutura de dados para uma solicitação de login,
    /// contendo as propriedades necessárias para autenticação do usuário, como o nome de usuário (Login) e a senha (Senha).
    /// </summary>
    public class LoginRequest
    {
        public string Login { get; set; } = "";
        public string Senha { get; set; } = "";
    }
}
