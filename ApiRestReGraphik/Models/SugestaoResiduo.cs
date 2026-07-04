using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models
{
    /// <summary>
    /// Classe que representa a estrutura de dados para uma sugestão de resíduo.
    /// </summary>  
    public class SugestaoResiduo
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }

        // Fiel ao Diagrama: DataAplicacao está como VARCHAR
        [JsonPropertyName("data_aplicacao")]
        [JsonProperty("data_aplicacao")]
        public DateTime? DataAplicacao { get; set; }

        // Fiel ao Diagrama: Sugestao está como DATE
        [JsonPropertyName("sugestao")]
        [JsonProperty("sugestao")]
        public string? Sugestao { get; set; }

        // Mapeamento da FK para manter o vínculo com o Resíduo se necessário
        [JsonPropertyName("id_cadastro_residuo")]
        [JsonProperty("id_cadastro_residuo")]
        public string? IdCadastroResiduo { get; set; }
    }
}
