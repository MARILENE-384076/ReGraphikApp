using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
        public string? Id { get; set; }

        [JsonPropertyName("sugestao")]
        public string SugestaoTexto { get; set; }

        [JsonPropertyName("data_aplicacao")]
        public DateTime? DataAplicacao { get; set; }
    }
}
