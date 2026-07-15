using Firebase.Database;
using Firebase.Database.Query;
using ReGraphik.Models;
using System.Text.Json;

namespace ReGraphik.Services
{
    /// <summary>
    /// Servico de chat que persiste mensagens diretamente
    /// no Firebase Realtime Database, sem passar pela API REST.
    /// </summary>
    public class ChatService
    {
        /// <summary>
        /// Instancia do FirebaseClient para interagir com o Realtime Database.
        /// </summary>
        private readonly FirebaseClient _db;
        private const string NodeMensagens = "mensagens";
        private const string NodeUsuarios = "usuarios";

        public ChatService()
        {
            _db = FirebaseConfigService.Client;
        }

        /// <summary>
        /// Gera um ID único para a conversa entre dois usuários, garantindo que a ordem dos IDs não importe.
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        private static string ConversaId(string id1, string id2)
        {
            var ids = new[] { id1, id2 };
            Array.Sort(ids, StringComparer.Ordinal);
            return $"{ids[0]}_{ids[1]}";
        }

        /// <summary>
        /// ObterMensagensAsync
        /// </summary>
        /// <param name="usuarioId1"></param>
        /// <param name="usuarioId2"></param>
        /// <returns></returns>
        public async Task<List<Mensagem>> ObterMensagensAsync(
            string usuarioId1, string usuarioId2)
        {
            try
            {
                var convId = ConversaId(usuarioId1, usuarioId2);
                var items = await _db
                    .Child(NodeMensagens)
                    .Child(convId)
                    .OnceAsync<Mensagem>();

                return items
                    .Select(i => i.Object)
                    .OrderBy(m => m.DataHora)
                    .ToList();
            }
            catch { return []; }
        }

        /// <summary>
        /// EnviarMensagemAsync
        /// </summary>
        /// <param name="mensagem"></param>
        /// <returns></returns>
        public async Task EnviarMensagemAsync(Mensagem mensagem)
        {
            var convId = ConversaId(
                mensagem.RemetenteId, mensagem.DestinatarioId);

            await _db
                .Child(NodeMensagens)
                .Child(convId)
                .Child(mensagem.Id)
                .PutAsync(mensagem);
        }

        /// <summary>
        /// MarcarComoLidaAsync
        /// </summary>
        /// <param name="remetenteId"></param>
        /// <param name="destinatarioId"></param>
        /// <returns></returns>

        public async Task MarcarComoLidaAsync(
            string remetenteId, string destinatarioId)
        {
            var convId = ConversaId(remetenteId, destinatarioId);

            var msgs = await _db
                .Child(NodeMensagens)
                .Child(convId)
                .OnceAsync<Mensagem>();

            var pendentes = msgs
                .Where(i => i.Object.RemetenteId == remetenteId
                         && !i.Object.Lida)
                .ToList();

            foreach (var item in pendentes)
            {
                await _db
                    .Child(NodeMensagens)
                    .Child(convId)
                    .Child(item.Key)
                    .Child("lida")
                    .PutAsync(true);
            }
        }

        /// <summary>
        /// ListarUsuariosAsync - Recupera os usuários do Firebase tratando incompatibilidades de nomenclatura do JSON de forma robusta.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Usuario>> ListarUsuariosAsync()
        {
            try
            {
                var items = await _db
                    .Child(NodeUsuarios)
                    .OnceAsync<object>();

                var listaUsuarios = new List<Usuario>();

                foreach (var item in items)
                {
                    if (item.Object == null) continue;

                    string jsonBruto = item.Object.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(jsonBruto)) continue;

                    var usuario = Newtonsoft.Json.JsonConvert.DeserializeObject<Usuario>(jsonBruto);

                    if (usuario != null)
                    {
                        if (string.IsNullOrEmpty(usuario.Id))
                        {
                            usuario.Id = item.Key;
                        }
                        using (var doc = JsonDocument.Parse(jsonBruto))
                        {
                            var root = doc.RootElement;

                            if (string.IsNullOrWhiteSpace(usuario.Nome))
                            {
                                if (root.TryGetProperty("Nome", out var propNome))
                                {
                                    usuario.Nome = propNome.GetString() ?? string.Empty;
                                }
                                else if (root.TryGetProperty("name", out var propName))
                                {
                                    usuario.Nome = propName.GetString() ?? string.Empty;
                                }
                            }

                            string? urlFotoDetectada = null;
                            if (root.TryGetProperty("FotoPerfil", out var propFoto))
                            {
                                urlFotoDetectada = propFoto.GetString();
                            }
                            else if (root.TryGetProperty("foto_perfil", out var propFotoUnder))
                            {
                                urlFotoDetectada = propFotoUnder.GetString();
                            }

                            if (!string.IsNullOrWhiteSpace(urlFotoDetectada))
                            {
                                usuario.FotoPerfil = urlFotoDetectada;
                            }
                        }

                        listaUsuarios.Add(usuario);
                    }
                }

                return listaUsuarios;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatService] Erro ao listar usuários: {ex.Message}");
                return [];
            }
        }

        /// <summary>
        /// ContarNaoLidasAsync
        /// </summary>
        /// <param name="destinatarioId"></param>
        /// <param name="remetenteId"></param>
        /// <returns></returns>

        public async Task<int> ContarNaoLidasAsync(
            string destinatarioId, string remetenteId)
        {
            try
            {
                var convId = ConversaId(destinatarioId, remetenteId);

                var msgs = await _db
                    .Child(NodeMensagens)
                    .Child(convId)
                    .OnceAsync<Mensagem>();

                return msgs.Count(i =>
                    i.Object.RemetenteId == remetenteId &&
                    i.Object.DestinatarioId == destinatarioId &&
                    !i.Object.Lida);
            }
            catch { return 0; }
        }
    }
}
