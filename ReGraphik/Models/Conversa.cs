using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReGraphik.Models
{
    /// <summary>
    /// Representa uma conversa entre o usuário logado e outro usuário,
    /// exibida na lista lateral do painel de chat.
    /// </summary>
    public class Conversa
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNome { get; set; } = string.Empty;
        public string UltimaMensagem { get; set; } = string.Empty;
        public DateTime UltimaDataHora { get; set; }
        public int MensagensNaoLidas { get; set; }

        /// <summary>
        /// Formata o horário da última mensagem de forma amigável.
        /// </summary>
        public string HorarioFormatado
        {
            get
            {
                var diff = DateTime.Now - UltimaDataHora;
                if (diff.TotalMinutes < 1) return "agora";
                if (diff.TotalHours < 1) return $"{(int)diff.TotalMinutes}min";
                if (diff.TotalDays < 1) return UltimaDataHora.ToString("HH:mm");
                return UltimaDataHora.ToString("dd/MM");
            }
        }

        /// <summary>
        /// Iniciais do nome do usuário para exibição no avatar.
        /// </summary>
        public string Iniciais
        {
            get
            {
                var partes = UsuarioNome.Trim().Split(' ');
                if (partes.Length >= 2)
                    return $"{partes[0][0]}{partes[^1][0]}".ToUpper();
                return UsuarioNome.Length >= 2
                    ? UsuarioNome[..2].ToUpper()
                    : UsuarioNome.ToUpper();
            }
        }
    }
}
