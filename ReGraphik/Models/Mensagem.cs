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
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("remetenteId")]
        public string RemetenteId { get; set; } = string.Empty;

        [JsonPropertyName("remetenteNome")]
        public string RemetenteNome { get; set; } = string.Empty;

        [JsonPropertyName("destinatarioId")]
        public string DestinatarioId { get; set; } = string.Empty;

        [JsonPropertyName("texto")]
        public string Texto { get; set; } = string.Empty;

        [JsonPropertyName("dataHora")]
        public DateTime DataHora { get; set; }

        [JsonPropertyName("lida")]
        public bool Lida { get; set; }

        /// <summary>
        /// Indica se a mensagem foi enviada pelo usuário logado (para diferenciação visual).
        /// Não é persistido — calculado em runtime.
        /// </summary>
        [JsonIgnore]
        public bool EhMinhaMensagem { get; set; }
    }
}
