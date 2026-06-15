using ReGraphik.Models;
using ReGraphik.Services.Interface;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReGraphik.Services
{
    /// <summary>
    /// Esta classe é responsável por lidar com a lógica de autorização do usuário, como login, cadastro e validação de tokens de segurança.
    /// </summary>
    public class AutorizarService : IAutorizarService
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://webregraphik.runasp.net/api/") };

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Envia os dados iniciais do usuário para o servidor realizar o pré-cadastro e gerar o token.
        /// </summary>
        /// <summary>
        public async Task<CadastroResponse?> CadastrarAsync(string nome, string cpf, string email, string login, string senha)
        {
            // CORREÇÃO: Propriedades agora batem exatamente com as expectativas do back-end (CPF em caixa alta e envio do Perfil)
            var payload = new Dictionary<string, object>
            {
                { "name", nome },        
                { "cpf", cpf },           
                { "email", email },       
                { "login", login },       
                { "senha", senha },       
                { "perfil", "Administrador" }      
            };

            var jsonEnviar = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonEnviar, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("usuario", content);

            if (!response.IsSuccessStatusCode)
            {
                var erroDaApi = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro no servidor ({response.StatusCode}): {erroDaApi}");
            }

            var jsonResposta = await response.Content.ReadAsStringAsync();

            // Retorna o resultado mapeado (contendo o Token) para a CadastroViewModel manipular
            return JsonSerializer.Deserialize<CadastroResponse>(jsonResposta, _jsonOptions);
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
            var payload = new { Email = email, Token = token };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

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

            // Nota: Se a sua API aceitar PUT para atualizações, mude para _httpClient.PutAsync
            var response = await _httpClient.PostAsync($"usuario/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var erroDaApi = await response.Content.ReadAsStringAsync();
                throw new Exception($"Status {response.StatusCode}. Detalhes: {erroDaApi}");
            }

            return true;
        }
    }
}