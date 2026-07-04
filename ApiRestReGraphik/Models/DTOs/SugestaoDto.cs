using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models.DTOs
{
    /// <summary>
    /// Classe DTO que representa a estrutura de dados para uma sugestão.
    /// </summary>
    public class SugestaoDto
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonPropertyName("tipo_residuo_aceito")]
        [JsonProperty("tipo_residuo_aceito")]
        public string TipoResiduoAceito { get; set; } = string.Empty;

        [JsonPropertyName("descricao_sugestao")]
        [JsonProperty("descricao_sugestao")]
        public string DescricaoSugestao { get; set; } = string.Empty;

        /// <summary>
        /// Chave estrangeira para a sugestão de resíduo associada.
        /// </summary>
        [JsonPropertyName("fk_sugestao_residuo_id")]
        [JsonProperty("fk_sugestao_residuo_id")]
        public string? FkSugestaoResiduoId { get; set; }
    }
}
