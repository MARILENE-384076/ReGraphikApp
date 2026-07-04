using ReGraphik.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RestReGraphik.Models
{
    /// <summary>
    /// Classe que representa a estrutura de dados para uma sugestão de resíduo, contendo propriedades como o ID do cadastro de resíduo associado,
    /// o ID da sugestão associada e a data de aplicação da sugestão.
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
