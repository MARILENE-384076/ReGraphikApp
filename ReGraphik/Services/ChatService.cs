using ReGraphik.Models;
using ReGraphik.Services.Interface;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace ReGraphik.Services
{
    /// <summary>
    /// Serviço de chat que consome a API REST do ReGraphik para troca de mensagens entre usuários.
    /// </summary>
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7002/api";

        public ChatService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Obtém todas as mensagens trocadas entre dois usuários, ordenadas por data/hora.
        /// </summary>
        public async Task<List<Mensagem>> ObterMensagensAsync(string usuarioId1, string usuarioId2)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/Chat/conversa/{usuarioId1}/{usuarioId2}");

                if (!response.IsSuccessStatusCode)
                    return [];

                var mensagens = await response.Content.ReadFromJsonAsync<List<Mensagem>>();
                return mensagens ?? [];
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Envia uma nova mensagem via API REST.
        /// </summary>
        public async Task EnviarMensagemAsync(Mensagem mensagem)
        {
            await _httpClient.PostAsJsonAsync($"{BaseUrl}/Chat", mensagem);
        }

        /// <summary>
        /// Marca como lidas todas as mensagens recebidas de um remetente.
        /// </summary>
        public async Task MarcarComoLidaAsync(string remetenteId, string destinatarioId)
        {
            await _httpClient.PutAsync(
                $"{BaseUrl}/Chat/marcar-lida/{remetenteId}/{destinatarioId}", null);
        }

        /// <summary>
        /// Lista todos os usuários cadastrados (exceto o próprio) para iniciar conversas.
        /// </summary>
        public async Task<List<Usuario>> ListarUsuariosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/Usuario");
                if (!response.IsSuccessStatusCode)
                    return [];

                var usuarios = await response.Content.ReadFromJsonAsync<List<Usuario>>();
                return usuarios ?? [];
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Conta quantas mensagens não lidas o destinatário recebeu de um remetente.
        /// </summary>
        public async Task<int> ContarNaoLidasAsync(string destinatarioId, string remetenteId)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"{BaseUrl}/Chat/nao-lidas/{destinatarioId}/{remetenteId}");

                if (!response.IsSuccessStatusCode) return 0;

                var resultado = await response.Content.ReadFromJsonAsync<int>();
                return resultado;
            }
            catch
            {
                return 0;
            }
        }
    }
}
