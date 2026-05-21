using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models
{
    public class Usuario
    {
        [JsonPropertyName("id")]
        public int ID { get; set; }

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

        [JsonPropertyName("data_cadastro")]
        public DateTime DataCadastro { get; set; }
    }
}
