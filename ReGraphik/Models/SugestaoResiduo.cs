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

        [JsonPropertyName("id_cadastro_residuo")]
        [ForeignKey("CadastroResiduo")]
        public string? IdCadastroResiduo { get; set; }
        /// <summary>
        /// Chamando a propriedade de navegação para o cadastro de resíduo associado
        /// </summary>

        [JsonIgnore]
 
        public virtual Residuo CadastroResiduo { get; set; }

        [JsonPropertyName("id_sugestao")]
        [ForeignKey("Sugestao")]
        public string IdSugestao { get; set; }

        /// <summary>
        /// Chamando a propriedade de navegação para a sugestão associada
        /// </summary>

        [JsonIgnore]
      
        public virtual Sugestao Sugestao { get; set; } 

        [JsonPropertyName("data_aplicacao")]
        public DateTime? DataAplicacao { get; set; }
    }
}
