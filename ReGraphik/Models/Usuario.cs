using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ReGraphik.Models
{
    public class Usuario
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string Id { get; set; }

        private string _nome;

        /// Se no Firebase estiver salvo como "nome" (minúsculo):
        [JsonPropertyName("nome")]
        [JsonProperty("nome")]
        public string Nome
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_nome))
                {
                    return !string.IsNullOrWhiteSpace(Login) ? Login : "Sem Nome";
                }
                return _nome;
            }
            set => _nome = value;
        }

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
                /// Usa a propriedade Nome (que agora já tem o fallback do Login)
                string nomeParaUsar = Nome;

                if (string.IsNullOrWhiteSpace(nomeParaUsar)) return "?";

                var partes = nomeParaUsar.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length == 1) return partes[0][0].ToString().ToUpper();
                return (partes[0][0].ToString() + partes[^1][0].ToString()).ToUpper();
            }
        }
    }
}