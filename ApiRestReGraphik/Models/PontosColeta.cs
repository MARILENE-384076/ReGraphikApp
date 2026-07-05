using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models
{
    /// <summary>
    /// Classe que representa a estrutura de dados para um ponto de coleta.
    /// </summary>
    public class PontosColeta
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("nome_ponto")]
        public string NomePonto { get; set; } = "Sem Nome";

        [JsonPropertyName("cidade")]
        public string Cidade { get; set; } = string.Empty;

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;

        [JsonPropertyName("cep")]
        public string CEP { get; set; } = string.Empty;

        [JsonPropertyName("residuos_aceitos")]
        public string ResiduosAceitos { get; set; } = string.Empty;

        [JsonPropertyName("latitude")]
        public double Lat { get; set; } = 0.0;

        [JsonPropertyName("longitude")]
        public double Lng { get; set; } = 0.0;
    }
}
