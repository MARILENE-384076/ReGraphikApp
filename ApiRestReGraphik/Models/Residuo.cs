using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiRestReGraphik.Models
{
    /// <summary>
    /// Representa um resíduo do sistema de estoque reverso do ReGraphik.
    /// Armazenado no Firebase Realtime Database sob o nó "residuos".
    /// </summary>
    public class Residuo
    {
        /// <summary>
        /// Identificador único do resíduo (GUID). Ignorado na desserialização do body HTTP.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Chave estrangeira para o usuário responsável pelo cadastro.
        /// </summary>
        [JsonIgnore]
        [JsonPropertyName("id_usuario")]
        [ForeignKey("Usuario")]
        public string? IdUsuario { get; set; }

        /// <summary>
        /// Propriedade de navegação para o usuário associado.</summary>
        [JsonIgnore]
        [BindNever]
        [ValidateNever]
        public virtual Usuario? Usuario { get; set; }

        /// <summary>
        /// Tipo do material (ex: Papel Offset, Lona, PVC).
        /// </summary>
        [JsonPropertyName("tipo_residuo")]
        public string TipoResiduo { get; set; } = string.Empty;

        /// <summary>
        /// Especificação técnica do material (ex: Fosco, Brilhoso).
        /// </summary>
        [JsonPropertyName("especificacao")]
        public string Especificacao { get; set; } = string.Empty;

        /// <summary>
        /// Origem do resíduo dentro do processo produtivo.
        /// </summary>
        [JsonPropertyName("origem")]
        public string Origem { get; set; } = string.Empty;

        /// <summary>
        /// Projeto de onde o resíduo foi gerado.
        /// </summary>
        [JsonPropertyName("projeto")]
        public string Projeto { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade numérica do resíduo.
        /// Deve ser interpretada em conjunto com <see cref="UnidadeMedida"/>.
        /// </summary>
        [JsonPropertyName("quantidade")]
        public double Quantidade { get; set; }

        /// <summary>
        /// Unidade de medida da quantidade (ex: kg, g, ton, unid).
        /// Valor padrão: "kg".
        /// </summary>
        [JsonPropertyName("unidade_medida")]
        public string UnidadeMedida { get; set; } = "kg";

        /// <summary>
        /// Unidade de dimensão do resíduo (ex: cm, m, mm).
        /// Valor padrão: "cm".
        /// </summary>
        [JsonPropertyName("unidade_dimensao")]
        public string UnidadeDimensao { get; set; } = "cm";


        /// <summary>
        /// Data de cadastro do resíduo no sistema.
        /// </summary>
        [JsonPropertyName("data_cadastro")]
        public DateTime DataCadastro { get; set; }

        /// <summary>
        /// Condição física do material (ex: Bom, Regular, Danificado).
        /// </summary>
        [JsonPropertyName("condicao")]
        public string Condicao { get; set; } = string.Empty;

        /// <summary>
        /// Comprimento do resíduo em centímetros.
        /// </summary>
        [JsonPropertyName("dimensoes_cm")]
        public double? DimensoesCm { get; set; }


        /// <summary>
        /// Largura do resíduo em centímetros.
        /// </summary>
        [JsonPropertyName("dimensoes_lm")]
        public double? DimensoesLm { get; set; }

        /// <summary>
        /// Observações adicionais sobre o resíduo.
        /// </summary>
        [JsonPropertyName("observacao")]
        public string Observacao { get; set; } = string.Empty;

        /// <summary>
        /// Caminho da imagem salva.
        /// </summary>
        public string? Anexo { get; set; }

        /// <summary>
        /// Arquivo enviado pelo formulário.
        /// </summary>
        [JsonIgnore]
        [NotMapped]
        public IFormFile? Imagem { get; set; }

        /// <summary>
        /// Status atual do resíduo (ex: Disponível, Reservado, Descartado).
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
