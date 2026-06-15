using Newtonsoft.Json;

namespace ReGraphik.Models
{
    public class Usuario
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonProperty("cpf")]
        public string CPF { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("senha")]
        public string Senha { get; set; }

        [JsonProperty("perfil")]
        public string Perfil { get; set; }

        [JsonProperty("foto_perfil")]
        public string? FotoPerfil { get; set; }

        [JsonProperty("ativo")]
        public bool Ativo { get; set; }
    }
}