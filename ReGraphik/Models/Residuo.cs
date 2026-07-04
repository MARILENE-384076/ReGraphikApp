using ReGraphik.Models;
using RestReGraphik.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ReGraphik.Models
{
    /// <summary>
    /// Representa um resíduo cadastrado no sistema de estoque reverso do ReGraphik.
    /// Contém informações sobre o material, origem, quantidade, unidade de medida,
    /// dimensões, condição, status e o usuário responsável pelo cadastro.
    /// </summary>
    public class Residuo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Retorna o ID do resíduo formatado como uma string de 8 caracteres, precedida por um "#".
        /// </summary>
        public string IdCard
        {
            get
            {
                if (string.IsNullOrEmpty(Id)) return "#00000000";
                return Id.Length > 8 ? $"#{Id.Substring(0, 8)}" : $"#{Id}";
            }
        }

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

        [JsonPropertyName("fk_sugestao_residuo_id")]
        public string? FkSugestaoResiduoId { get; set; }

    }
}

