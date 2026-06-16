using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ReGraphik.Models
{
    public class Usuario
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonPropertyName("cpf")]
        [JsonProperty("cpf")]
        public string CPF { get; set; }

        [JsonPropertyName("email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonPropertyName("login")]
        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonPropertyName("senha")]
        [JsonProperty("senha")]
        public string Senha { get; set; }

        [JsonPropertyName("perfil")]
        [JsonProperty("perfil")]
        public string Perfil { get; set; }

        [JsonPropertyName("foto_perfil")]
        [JsonProperty("foto_perfil")]
        public string? FotoPerfil { get; set; }

        [JsonPropertyName("ativo")]
        [JsonProperty("ativo")]
        public bool Ativo { get; set; }
    }
}