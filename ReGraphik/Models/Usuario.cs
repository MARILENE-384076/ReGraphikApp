using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ReGraphik.Models
{
    public class Usuario
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("nome")]
        [JsonProperty("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("cpf")]
        [JsonProperty("cpf")]
        public string CPF { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("login")]
        [JsonProperty("login")]
        public string Login { get; set; } = string.Empty;

        [JsonPropertyName("senha")]
        [JsonProperty("senha")]
        public string Senha { get; set; } = string.Empty;

        [JsonPropertyName("perfil")]
        [JsonProperty("perfil")]
        public string Perfil { get; set; } = string.Empty;

        [JsonPropertyName("data_cadastro")]
        [JsonProperty("data_cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        [JsonPropertyName("foto_perfil")]
        [JsonProperty("foto_perfil")]
        public string? FotoPerfil { get; set; }

        [JsonPropertyName("ativo")]
        [JsonProperty("ativo")]
        public bool Ativo { get; set; }

        /// <summary>
        /// Chave estrangeira para o resíduo associado ao usuário.
        /// </summary>
        [JsonPropertyName("fk_residuo_id")]
        [JsonProperty("fk_residuo_id")]
        public string? FkResiduoId { get; set; }

        /// <summary>
        /// Chave estrangeira para a conversa associada ao usuário. 
        /// </summary>

        [JsonPropertyName("fk_conversa_usuarioid")]
        [JsonProperty("fk_conversa_usuarioid")]
        public string? FkConversaUsuarioId { get; set; }

        /// <summary>
        /// Chave estrangeira para a mensagem associada ao usuário.
        /// </summary>

        [JsonPropertyName("fk_mensagem_id")]
        [JsonProperty("fk_mensagem_id")]
        public string? FkMensagemId { get; set; }

        /// <summary>
        /// Iniciais do nome para exibição no avatar quando não há foto.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore] /// Impede que essa propriedade calculada vá para o banco
        [Newtonsoft.Json.JsonIgnore]
        public string Iniciais
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nome)) return "?";

                var partes = Nome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                return partes.Length switch
                {
                    0 => "?",
                    1 => partes[0][0].ToString().ToUpper(),
                    _ => $"{partes[0][0]}{partes[1][0]}".ToUpper()
                };
            }
        }
    }
}