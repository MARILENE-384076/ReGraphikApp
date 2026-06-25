using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using ReGraphik.Models;

namespace ReGraphik.Services
{
    public class ConviteService
    {
        private const string NodeConvites = "convites";
        private readonly FirebaseClient _db;

        public ConviteService()
        {
            _db = FirebaseConfig.Client;
        }

        /// <summary>
        /// Chamado pelo Administrador para gerar convite
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<string> GerarConviteAsync(string email, string perfil)
        {
            var token = GerarToken(8);
            var convite = new
            {
                email = email.Trim().ToLower(),
                expira = DateTime.UtcNow.AddHours(48).ToString("o"),
                usado = false
            };
            await _db.Child(NodeConvites).Child(token).PutAsync(convite);
            return token;
        }

        /// <summary>
        /// Etapa 1: verifica se o e-mail tem convite ativo
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> ExisteConvitePendenteAsync(string email)
        {
            try
            {
                var todos = await _db.Child(NodeConvites).OnceAsync<ConviteFirebase>();
                return todos.Any(item =>
                    string.Equals(item.Object.Email, email.Trim(),
                        StringComparison.OrdinalIgnoreCase) &&
                    !item.Object.Usado &&
                    DateTime.UtcNow <= DateTime.Parse(item.Object.Expira));
            }
            catch { return false; }
        }

        /// <summary>
        /// Etapa 2: valida o token digitado pelo usuario
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <param name="perfil"></param>
        /// <returns></returns>
        public async Task<string?> ValidarTokenAsync(string email, string token)
        {
            try
            {
                var resultado = await _db
                    .Child(NodeConvites)
                    .Child(token.Trim().ToUpper())
                    .OnceSingleAsync<ConviteFirebase>();

                if (resultado == null) return null;
                if (resultado.Usado) return null;
                if (DateTime.UtcNow > DateTime.Parse(resultado.Expira)) return null;
                if (!string.Equals(resultado.Email, email.Trim(),
                    StringComparison.OrdinalIgnoreCase)) return null;

                /// Retorna o perfil salvo no Firabse
                return !string.IsNullOrWhiteSpace(resultado.Perfil) ? resultado.Perfil : "User";
            }
            catch 
            { 
                return null; 
            }
        }

        /// <summary>
        /// Etapa 3: marca token como usado apos o cadastro
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task MarcarComoUsadoAsync(string token)
        {
            await _db
                .Child(NodeConvites)
                .Child(token.Trim().ToUpper())
                .Child("usado")
                .PutAsync(true);
        }

        /// <summary>
        /// Gera token de 8 caracteres sem caracteres ambiguos (0,O,I,1)
        /// </summary>
        /// <param name="tamanho"></param>
        /// <returns></returns>
        private static string GerarToken(int tamanho)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rng = new Random();
            var c = new char[tamanho];
            for (int i = 0; i < tamanho; i++)
                c[i] = chars[rng.Next(chars.Length)];
            return new string(c);
        }
    }
}
