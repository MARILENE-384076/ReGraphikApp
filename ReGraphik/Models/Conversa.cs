using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ReGraphik.Models
{
    /// <summary>
    /// Representa uma conversa entre o usuário logado e outro usuário,
    /// exibida na lista lateral do painel de chat.
    /// </summary>
    public class Conversa
    {
        [JsonPropertyName("usuario_id")]
        [JsonProperty("usuario_id")]
        public string UsuarioId { get; set; } = string.Empty;

        [JsonPropertyName("usuario_nome")]
        [JsonProperty("usuario_nome")]
        public string UsuarioNome { get; set; } = string.Empty;

        [JsonPropertyName("ultima_mensagem")]
        [JsonProperty("ultima_mensagem")]
        public string UltimaMensagem { get; set; } = string.Empty;

        [JsonPropertyName("ultima_data_hora")]
        [JsonProperty("ultima_data_hora")]
        public DateTime UltimaDataHora { get; set; }

        [JsonPropertyName("mensagens_nao_lidas")]
        [JsonProperty("mensagens_nao_lidas")]
        public int MensagensNaoLidas { get; set; }

        /// <summary>
        /// Formata o horário da última mensagem de forma amigável.
        /// Ignorado na serialização JSON.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string HorarioFormatado
        {
            get
            {
                var diff = DateTime.Now - UltimaDataHora;

                return diff switch
                {
                    { TotalMinutes: < 1 } => "agora",
                    { TotalHours: < 1 } => $"{(int)diff.TotalMinutes}min",
                    { TotalDays: < 1 } => UltimaDataHora.ToString("HH:mm"),
                    _ => UltimaDataHora.ToString("dd/MM")
                };
            }
        }

        /// <summary>
        /// Iniciais do nome do usuário para exibição no avatar.
        /// Ignorado na serialização JSON.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string Iniciais
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UsuarioNome))
                    return "?";

                var partes = UsuarioNome.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (partes.Length >= 2)
                    return $"{partes[0][0]}{partes[^1][0]}".ToUpper();

                return UsuarioNome.Length >= 2
                    ? UsuarioNome[..2].ToUpper()
                    : UsuarioNome.ToUpper();
            }
        }
    }
}
