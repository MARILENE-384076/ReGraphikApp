using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models
{
    /// <summary>
    /// Classe que representa a estrutura de dados para um usuário.
    /// </summary>
    public class Usuario
    {
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

        [JsonPropertyName("perfil")]
        public string Perfil { get; set; } = "User";

        [JsonPropertyName("data_cadastro")]
        public DateTime DataCadastro { get; set; }

        [JsonPropertyName("foto_perfil")]
        public string? FotoPerfil { get; set; }

        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; }

    }
}
