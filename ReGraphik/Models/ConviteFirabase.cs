using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReGraphik.Models
{
    /// <summary>
    ///  Classe que representa um dado de convite para que o usuário possa cadastrar no sitema.
    /// </summary>
    public class ConviteFirebase
    {
        [JsonProperty("email")] 
        public string Email { get; set; } = string.Empty;

        [JsonProperty("expira")]
        public string Expira { get; set; } = string.Empty;

        [JsonProperty("usado")] 
        public bool Usado { get; set; }

        [JsonProperty("perfil")]
        public string Perfil { get; set; } = string.Empty;
    }
}
