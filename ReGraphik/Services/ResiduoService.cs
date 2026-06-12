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
    /// <summary>
    /// Esta classe é responsável por lidar com a lógica relacionada aos resíduos, como obter a lista de resíduos do banco de dados.
    /// </summary>
    public class ResiduoService : IResiduoService
    {
        /// Usamos um HttpClient estático para reutilizar a mesma instância em toda a aplicação, evitando problemas de esgotamento de conexões.
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://webregraphik.runasp.net/api/") };

        /// <summary>
        /// Método para obter todos os resíduos do banco de dados, fazendo uma requisição GET para a API.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<List<Residuo>> ObterTodosResiduosAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("Residuo");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Erro na API: {response.StatusCode}");

            string jsonResult = await response.Content.ReadAsStringAsync();
            var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return JsonSerializer.Deserialize<List<Residuo>>(jsonResult, opcoes) ?? new List<Residuo>();
        }
    }
}
