using ReGraphik.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace ReGraphik.Models 
{
    /// <summary>
    /// Classe que representa a estrutura de dados para um resíduo, contendo propriedades como tipo de resíduo, origem, 
    /// especificação, projeto associado, quantidade, data de cadastro, condição, dimensões, observações, anexo e status.
    /// </summary>
    public class Residuo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Retorna o ID formatado e encurtado para o Card (Ex: #10dcd90e)
        /// </summary>
        public string IdCard
        {
            get
            {
                if (string.IsNullOrEmpty(Id)) return "#00000000";

                // Se o ID for um GUID completo, pega apenas os primeiros 8 caracteres
                return Id.Length > 8
                    ? $"#{Id.Substring(0, 8)}"
                    : $"#{Id}";
            }
        }

        [JsonPropertyName("id_usuario")]
        [ForeignKey("Usuario")]
        public string? IdUsuario { get; set; }

        /// <summary>
        /// Propriedade de navegação para o usuário associado
        /// </summary>
        [JsonIgnore]
        public virtual Usuario? Usuario { get; set; }

        [JsonPropertyName("tipo_residuo")]
        public string TipoResiduo { get; set; }

        [JsonPropertyName("origem")]
        public string Origem { get; set; }

        [JsonPropertyName("especificacao")]
        public string Especificacao { get; set; }

        [JsonPropertyName("projeto")]
        public string Projeto { get; set; }

        [JsonPropertyName("quantidade")]
        public double Quantidade { get; set; }

        [JsonPropertyName("data_cadastro")]
        public DateTime DataCadastro { get; set; }

        [JsonPropertyName("condicao")]
        public string Condicao { get; set; }

        [JsonPropertyName("dimensoes_cm")]
        public double? DimensoesCm { get; set; }

        [JsonPropertyName("dimensoes_lm")]
        public double? DimensoesLm { get; set; }

        [JsonPropertyName("observacao")]
        public string Observacao { get; set; }

        [JsonPropertyName("anexo")]
        public string? Anexo { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
