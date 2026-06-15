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
        private readonly FirebaseClient _db;
        private const string NodeMensagens = "mensagens";
        private const string NodeUsuarios = "usuarios";

        public ChatService()
        {
            _db = FirebaseConfig.Client;
        }

        
        /// Gera ID de conversa deterministico (menor ID primeiro)
        
        private static string ConversaId(string id1, string id2)
        {
            var ids = new[] { id1, id2 };
            Array.Sort(ids, StringComparer.Ordinal);
            return $"{ids[0]}_{ids[1]}";
        }

        
        /// ObterMensagensAsync
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

        
        /// EnviarMensagemAsync
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

        
        /// MarcarComoLidaAsync
        
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
        /// ListarUsuariosAsync
        /// </summary>
        /// <returns></returns>
        public async Task<List<Usuario>> ListarUsuariosAsync()
        {
            try
            {
                var items = await _db
                    .Child(NodeUsuarios)
                    .OnceAsync<Usuario>();

                return items
                    .Select(i =>
                    {
                        var u = i.Object;
                        // Garante que o Id vem da chave do Firebase
                        // mesmo que o campo "id" dentro do JSON esteja vazio
                        if (string.IsNullOrEmpty(u.Id))
                            u.Id = i.Key;
                        return u;
                    })
                    .Where(u => u != null)
                    .ToList();
            }
            catch { return []; }
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
