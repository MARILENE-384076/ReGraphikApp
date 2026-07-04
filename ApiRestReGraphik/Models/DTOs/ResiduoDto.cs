using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models.DTOs
{
    /// <summary>
    /// Classe DTO que representa a estrutura de dados para um resíduo, contendo propriedades como tipo de resíduo.
    /// </summary>
    public class ResiduoDto
    {
        [JsonIgnore]
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("tipo_residuo")]
        public string TipoResiduo { get; set; } = string.Empty;

        [JsonPropertyName("origem")]
        public string Origem { get; set; } = string.Empty;

        [JsonPropertyName("especificacao")]
        public string Especificacao { get; set; } = string.Empty;

        [JsonPropertyName("projeto")]
        public string Projeto { get; set; } = string.Empty;

        [JsonPropertyName("quantidade")]
        public int Quantidade { get; set; }

        [JsonPropertyName("data_cadastro")]
        public DateTime DataCadastro { get; set; }

        [JsonPropertyName("condicao")]
        public string Condicao { get; set; } = string.Empty;

        [JsonPropertyName("dimensoes_cm")]
        public double? DimensoesCm { get; set; }

        [JsonPropertyName("dimensoes_lm")]
        public double? DimensoesLm { get; set; }

        [JsonPropertyName("observacao")]
        public string Observacao { get; set; } = string.Empty;

        [JsonPropertyName("anexo")]
        public string? Anexo { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Chave estrangeira para a sugestão de resíduo associada.
        /// </summary>
        [JsonPropertyName("fk_sugestao_residuo_id")]
        [ForeignKey("SugestaoResiduo")]
        public string? FkSugestaoResiduoId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public virtual SugestaoResiduo? SugestaoResiduo { get; set; }
    }
}
