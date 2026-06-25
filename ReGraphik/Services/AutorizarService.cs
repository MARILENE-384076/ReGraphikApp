using ReGraphik.Models;
using ReGraphik.Services.Interface;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace ReGraphik.Services
{
    /// <summary>
    /// Esta classe é responsável por lidar com a lógica de autorização do usuário, como login, cadastro e validação de tokens de segurança.
    /// </summary>
    public class AutorizarService : IAutorizarService
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://webregraphik.runasp.net/api/") };

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public async Task<bool> SolicitarAcessoAsync(string email)
        {
            var content = new StringContent(JsonSerializer.Serialize(new { email }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("usuario", content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Envia os dados iniciais do usuário para o servidor realizar o pré-cadastro e gerar o token.
        /// </summary>
        /// <summary>
        public async Task<bool> FinalizarCadastroAsync(string nome, string cpf, string email, string login, string senha, string token)
        {
            var payload = new Dictionary<string, object>
            {
                { "name", nome },
                { "cpf", cpf },
                { "email", email },
                { "login", login },
                { "senha", senha },
                { "perfil", "Administrador" }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(
                $"usuario/finalizar-cadastro?token={token}",
                content);

            var resposta = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Realiza a autenticação direta do usuário através de login e senha.
        /// </summary>
        public async Task<Usuario?> LoginAsync(string login, string senha)
        {
            var payload = new { login, senha };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("usuario/login", content);

            if (!response.IsSuccessStatusCode) return null;

            var jsonResposta = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Usuario>(jsonResposta, _jsonOptions);
        }

        /// <summary>
        /// Envia o token inserido pelo usuário para validação final no servidor.
        /// </summary>
        public async Task<bool> ValidarTokenAsync(string email, string token)
        {
            var content = new StringContent(JsonSerializer.Serialize(new { Email = email, Token = token }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("usuario/validar-token", content);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Atualiza os dados cadastrais do usuário baseado no ID informado.
        /// </summary>
        public async Task<bool> AtualizarAsync(string id, object usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"usuario/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var erroDaApi = await response.Content.ReadAsStringAsync();
                throw new Exception($"Status {response.StatusCode}. Detalhes: {erroDaApi}");
            }

            return true;
        }
    }
}