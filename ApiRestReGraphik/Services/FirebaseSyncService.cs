using Google.Apis.Auth.OAuth2;
using System.Text;
using System.Text.Json;

namespace ApiRestReGraphik.Services
{
    public class FirebaseSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly string _databaseUrl = "https://regraphikfirebase-default-rtdb.firebaseio.com/";

        public FirebaseSyncService()
        {
            _httpClient = new HttpClient();
        }

        // Método genérico para salvar qualquer dado em qualquer "nó" do Firebase
        public async Task SalvarNoFirebaseAsync<T>(string no, T dados)
        {
            // Obtém o token de acesso gerado automaticamente pelo arquivo JSON de credenciais
            var credential = GoogleCredential.FromFile("ReGraphikFirebaseKey.json")
                                             .CreateScoped("https://www.googleapis.com/auth/userinfo.email",
                                                           "https://www.googleapis.com/auth/firebase.database");
            var token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            // Transforma o objeto C# em JSON
            var json = JsonSerializer.Serialize(dados);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Envia um PUT para a URL do Firebase incluindo o Token de Autenticação
            var url = $"{_databaseUrl}{no}.json?access_token={token}";
            var response = await _httpClient.PutAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Erro ao salvar no Firebase: {erro}");
            }
        }
    }
}
