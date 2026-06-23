using Newtonsoft.Json;
using System.Text.Json.Serialization;

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

        [JsonProperty("data_cadastro")]
        public DateTime DataCadastro { get; set; }

        [JsonProperty("foto_perfil")]
        public string? FotoPerfil { get; set; }

        [JsonProperty("ativo")]
        public bool Ativo { get; set; }

        [JsonProperty("token_validacao")]
        public string? TokenValidacao { get; set; }

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