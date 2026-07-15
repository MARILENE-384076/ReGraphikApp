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
            _db = FirebaseConfigService.Client;
        }

        /// <summary>
        /// Chamado pelo Administrador para gerar convite
        /// </summary>
        /// <param name="email"></param>
        /// <param name="perfil"></param>
        /// <returns></returns>
        public async Task<string> GerarConviteAsync(string email, string perfil)
        {
            var token = GerarToken(8);
            var convite = new
            {
                email = email.Trim().ToLower(),
                /// Configurado para expirar em 30 minutos
                expira = DateTime.UtcNow.AddMinutes(30).ToString("o"),
                perfil = perfil, /// Salva o perfil (Admin/User) no Firebase
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
                {
                    if (item.Object == null || string.IsNullOrWhiteSpace(item.Object.Email))
                        return false;

                    /// Compara e-mails ignorando maiúsculas/minúsculas e espaços
                    bool emailIgual = string.Equals(item.Object.Email.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase);

                    /// Converte a string de expiração do Firebase forçando a leitura como UTC (por causa do 'Z')
                    DateTime dataExpiraUtc = DateTime.Parse(item.Object.Expira, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                    /// Compara de forma justa com o relógio UTC global
                    bool naoExpirou = DateTime.UtcNow <= dataExpiraUtc;

                    return emailIgual && !item.Object.Usado && naoExpirou;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erro ExisteConvitePendenteAsync]: {ex.Message}");
                return false;
            }
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
                /// Busca todos os convites do nó no Firebase
                var todos = await _db.Child(NodeConvites).OnceAsync<ConviteFirebase>();

                /// Procura pelo convite específico
                var conviteEncontrado = todos.FirstOrDefault(item =>
                    /// Compara o Token gerado pelo Firebase (a chave do nó, ex: "TBEM4EC7") com o token digitado
                    string.Equals(item.Key, token.Trim(), StringComparison.OrdinalIgnoreCase) &&

                    /// Compara o e-mail cadastrado de forma segura
                    string.Equals(item.Object.Email?.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase) &&

                    /// Verifica se ainda não foi utilizado
                    !item.Object.Usado);

                if (conviteEncontrado != null)
                {
                    /// Converte e valida a data de expiração em formato UTC
                    DateTime dataExpiraUtc = DateTime.Parse(conviteEncontrado.Object.Expira, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

                    if (DateTime.UtcNow <= dataExpiraUtc)
                    {
                        /// Se o token for válido e não expirou, retorna o perfil ("User", "Admin", etc.)
                        return conviteEncontrado.Object.Perfil ?? "User";
                    }
                }

                return null; 
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Erro ValidarTokenAsync]: {ex.Message}");
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

        /// <summary>
        /// Recupera o token ativo no Firebase atrelado a um e-mail.
        /// </summary>
        public async Task<string?> ObterTokenPorEmailAsync(string email)
        {
            try
            {
                /// Busca todos os convites do nó do Firebase
                var todos = await _db.Child(NodeConvites).OnceAsync<ConviteFirebase>();

                /// Procura o primeiro registro que pertença ao e-mail, que não esteja usado e não esteja expirado
                var conviteValido = todos.FirstOrDefault(item =>
                    string.Equals(item.Object.Email, email.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    !item.Object.Usado &&
                    DateTime.UtcNow <= DateTime.Parse(item.Object.Expira));

                /// Como a chave do nó (Key) é o token criado, retornamos item.Key
                return conviteValido?.Key;
            }
            catch
            {
                return null;
            }
        }
    }
}
