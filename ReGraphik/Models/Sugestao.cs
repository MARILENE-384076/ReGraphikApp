using RestReGraphik.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ReGraphik.Models
{
    /// <summary>
    /// Classe que representa a estrutura de dados para uma sugestão, contendo propriedades como tipo de resíduo aceito e descrição da sugestão.
    /// </summary>
    public class Sugestao
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("tipo_residuo_aceito")]
        public string TipoResiduoAceito { get; set; }

        [JsonPropertyName("descricao_sugestao")]
        public string DescricaoSugestao { get; set; }

        /// <summary>
        /// Chave estrangeira para a sugestão de resíduo associada.
        /// </summary>
        [JsonPropertyName("fk_sugestao_residuo_id")]
        [ForeignKey("SugestaoResiduo")]
        public string? FkSugestaoResiduoId { get; set; }

        [JsonIgnore]
        public virtual SugestaoResiduo? SugestaoResiduo { get; set; }
    }
}
