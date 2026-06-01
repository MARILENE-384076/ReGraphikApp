using ReGraphik.Models;
using ReGraphik.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReGraphik.Services
{
    // Esta classe é responsável por lidar com a lógica de autorização do usuário, como login, logout e verificação de permissões.
    public class AutorizarService : IAutorizarService
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://webregraphik.runasp.net/api/") };

        public async Task<bool> CadastrarAsync(object usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("usuario", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<Usuario?> LoginAsync(string login, string senha)
        {
            var content = new StringContent(JsonSerializer.Serialize(new { login, senha }), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("usuario/login", content);

            if (!response.IsSuccessStatusCode) return null;

            var jsonResposta = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Usuario>(jsonResposta, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
