using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReGraphik.Models
{
    /// <summary>
    ///  Classe que representa um dado de convite para que o usuário possa cadastrar no sitema.
    /// </summary>
    public class ConviteFirebase
    {
        [JsonPropertyName("email")]
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("expira")]
        [JsonProperty("expira")]
        public string Expira { get; set; } = string.Empty;

        [JsonPropertyName("usado")]
        [JsonProperty("usado")]
        public bool Usado { get; set; }

        [JsonPropertyName("perfil")]
        [JsonProperty("perfil")]
        public string Perfil { get; set; } = string.Empty;
    }
}
