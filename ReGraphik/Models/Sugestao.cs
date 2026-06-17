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
    }
}
