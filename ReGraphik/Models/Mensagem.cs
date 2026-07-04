using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReGraphik.Models
{
    public class Mensagem
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("remetenteId")]
        [JsonProperty("remetenteId")]
        public string RemetenteId { get; set; } = string.Empty;

        [JsonPropertyName("remetenteNome")]
        [JsonProperty("remetenteNome")]
        public string RemetenteNome { get; set; } = string.Empty;

        [JsonPropertyName("destinatarioId")]
        [JsonProperty("destinatarioId")]
        public string DestinatarioId { get; set; } = string.Empty;

        [JsonPropertyName("texto")]
        [JsonProperty("texto")]
        public string Texto { get; set; } = string.Empty;

        [JsonPropertyName("dataHora")]
        [JsonProperty("dataHora")]
        public DateTime DataHora { get; set; }

        [JsonPropertyName("lida")]
        [JsonProperty("lida")]
        public bool Lida { get; set; }

        /// <summary>
        /// Indica se a mensagem foi enviada pelo usuário logado (para diferenciação visual).
        /// Não é persistido — calculado em runtime.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool EhMinhaMensagem { get; set; }
    }
}
