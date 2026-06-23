using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;

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

        // Chamado pelo Administrador para gerar um convite
        public async Task<string> GerarConviteAsync(string email)
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

        // Etapa 1: verifica se o e-mail tem convite ativo
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

        // Etapa 2: valida o token digitado pelo usuario
        public async Task<bool> ValidarTokenAsync(string email, string token)
        {
            try
            {
                var resultado = await _db
                    .Child(NodeConvites)
                    .Child(token.Trim().ToUpper())
                    .OnceSingleAsync<ConviteFirebase>();

                if (resultado == null) return false;
                if (resultado.Usado) return false;
                if (DateTime.UtcNow > DateTime.Parse(resultado.Expira)) return false;
                if (!string.Equals(resultado.Email, email.Trim(),
                    StringComparison.OrdinalIgnoreCase)) return false;

                return true;
            }
            catch { return false; }
        }

        // Etapa 3: marca token como usado apos o cadastro
        public async Task MarcarComoUsadoAsync(string token)
        {
            await _db
                .Child(NodeConvites)
                .Child(token.Trim().ToUpper())
                .Child("usado")
                .PutAsync(true);
        }

        // Gera token de 8 caracteres sem caracteres ambiguos (0,O,I,1)
        private static string GerarToken(int tamanho)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rng = new Random();
            var c = new char[tamanho];
            for (int i = 0; i < tamanho; i++)
                c[i] = chars[rng.Next(chars.Length)];
            return new string(c);
        }

        private class ConviteFirebase
        {
            [JsonProperty("email")] public string Email { get; set; } = "";
            [JsonProperty("expira")] public string Expira { get; set; } = "";
            [JsonProperty("usado")] public bool Usado { get; set; }
        }
    }
}
