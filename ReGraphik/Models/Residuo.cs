using ReGraphik.Models;
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
        /// <summary>Identificador único do resíduo (GUID gerado pelo sistema).</summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Retorna o ID formatado e encurtado para exibição em cards (Ex: #10dcd90e).
        /// </summary>
        public string IdCard
        {
            get
            {
                if (string.IsNullOrEmpty(Id)) return "#00000000";
                return Id.Length > 8 ? $"#{Id.Substring(0, 8)}" : $"#{Id}";
            }
        }

        /// <summary>Chave estrangeira para o usuário que cadastrou o resíduo.</summary>
        [JsonPropertyName("id_usuario")]
        [ForeignKey("Usuario")]
        public string? IdUsuario { get; set; }

        /// <summary>Propriedade de navegação para o usuário associado ao resíduo.</summary>
        [JsonIgnore]
        public virtual Usuario? Usuario { get; set; }

        /// <summary>Tipo do material (ex: Papel Offset, Lona, PVC).</summary>
        [JsonPropertyName("tipo_residuo")]
        public string TipoResiduo { get; set; } = string.Empty;

        /// <summary>Especificação técnica do material (ex: Fosco, Brilhoso).</summary>
        [JsonPropertyName("especificacao")]
        public string Especificacao { get; set; } = string.Empty;

        /// <summary>Origem do resíduo (ex: Produção, Acabamento).</summary>
        [JsonPropertyName("origem")]
        public string Origem { get; set; } = string.Empty;

        /// <summary>Nome do projeto de onde o resíduo originou.</summary>
        [JsonPropertyName("projeto")]
        public string Projeto { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade numérica do resíduo. O significado depende da <see cref="UnidadeMedida"/>.
        /// </summary>
        [JsonPropertyName("quantidade")]
        public double Quantidade { get; set; }

        /// <summary>
        /// Unidade de medida da quantidade (ex: kg, g, ton, unid).
        /// Evita ambiguidade na leitura dos dados de estoque.
        /// </summary>
        [JsonPropertyName("unidade_medida")]
        public string UnidadeMedida { get; set; } = "kg";

        /// <summary>Data em que o resíduo foi cadastrado no sistema.</summary>
        [JsonPropertyName("data_cadastro")]
        public DateTime DataCadastro { get; set; }

        /// <summary>Condição física do material (ex: Bom, Regular, Danificado).</summary>
        [JsonPropertyName("condicao")]
        public string Condicao { get; set; } = string.Empty;

        /// <summary>Comprimento/altura do resíduo em centímetros.</summary>
        [JsonPropertyName("dimensoes_cm")]
        public double? DimensoesCm { get; set; }

        /// <summary>Largura do resíduo em centímetros.</summary>
        [JsonPropertyName("dimensoes_lm")]
        public double? DimensoesLm { get; set; }

        /// <summary>Observações adicionais sobre o resíduo.</summary>
        [JsonPropertyName("observacao")]
        public string Observacao { get; set; } = string.Empty;

        /// <summary>Imagem do resíduo codificada em Base64 (opcional).</summary>
        [JsonPropertyName("anexo")]
        public string? Anexo { get; set; }

        /// <summary>Status atual do resíduo (ex: Disponível, Reservado, Descartado).</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
