using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models
{
    /// <summary>
    /// Classe que representa a estrutura de dados para uma solicitação de login,
    /// contendo as propriedades necessárias para autenticação do usuário, como o nome de usuário (Login) e a senha (Senha).
    /// </summary>
    public class LoginRequest
    {
        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("senha")]
        public string Senha { get; set; }
    }
}
