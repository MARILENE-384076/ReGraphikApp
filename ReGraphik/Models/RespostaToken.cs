using System.Text.Json.Serialization;

namespace ReGraphik.Models
{
    /// <summary>
    /// Representa um alerta de mensagem de token, onde irá mostrar o token para o usuário.
    /// </summary>
    public class RespostaToken
    {
        [JsonPropertyName("mensagem")] 
        public string Mensagem { get; set; }

    }
}
