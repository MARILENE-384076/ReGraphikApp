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

        [JsonPropertyName("data_cadastro")]
        [JsonProperty("data_cadastro")]
        public DateTime DataCadastro { get; set; }

        [JsonPropertyName("foto_perfil")]
        [JsonProperty("foto_perfil")]
        public string? FotoPerfil { get; set; }

        [JsonPropertyName("ativo")]
        [JsonProperty("ativo")]
        public bool Ativo { get; set; }

        /// <summary>
        /// Iniciais do nome para exibição no avatar quando não há foto.
        /// Retorna as iniciais das duas primeiras palavras do nome (ex: "Bruno Maia" → "BM").
        /// </summary>
        public string Iniciais
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nome)) return "?";
                var partes = Nome.Trim().Split(' ',
                    StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length == 1)
                    return partes[0][0].ToString().ToUpper();
                return $"{partes[0][0]}{partes[^1][0]}".ToUpper();
            }
        }
    }
}