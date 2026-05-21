using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models
{
    public class Sugestao
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("id_tipo_residuo")]
        public int IdTipoResiduo{ get; set; }

        [JsonPropertyName("descricao_sugestao")]
        public required string DescricaoSugestao { get; set; }
    }
}
