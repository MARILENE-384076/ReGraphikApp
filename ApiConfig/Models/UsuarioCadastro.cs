using System.Text.Json.Serialization;

namespace ApiConfig.Models
{
    /// <summary>
    /// Classe que representa a estrutura de dados para um usuário, contendo propriedades como nome, CPF,
    /// email, login e senha.
    /// </summary>
    public class UsuarioCadastro
    {
        [JsonIgnore]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Nome { get; set; }

        [JsonPropertyName("cpf")]
        public string CPF { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("login")]
        public string Login { get; set; }

        [JsonPropertyName("senha")]
        public string Senha { get; set; }

    }
}
