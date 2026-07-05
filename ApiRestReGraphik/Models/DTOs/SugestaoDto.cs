using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models.DTOs
{
    /// <summary>
    /// Classe DTO que representa a estrutura de dados para uma sugestão.
    /// </summary>
    public class SugestaoDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("tipo_residuo_aceito")]
        public string TipoResiduoAceito { get; set; }

        [JsonPropertyName("descricao_sugestao")]
        public string DescricaoSugestao { get; set; }
    }
}
