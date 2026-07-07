using ReGraphik.Models;
using ReGraphik.Services.Interface;
using ReGraphik.Views;
using System;
using System.IO;
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
        public async Task<bool> FinalizarCadastroAsync(string nome, string cpf, string email, string login, string senha, string token, string perfil)
        {
            /// Usamos MultipartFormDataContent para simular o [FromForm] da API
            using var content = new MultipartFormDataContent();

            /// Adiciona cada campo como StringContent (respeitando as propriedades do seu UsuarioDto da API)
            content.Add(new StringContent(nome ?? ""), "Nome");
            content.Add(new StringContent(cpf ?? ""), "CPF");
            content.Add(new StringContent(email ?? ""), "Email");
            content.Add(new StringContent(login ?? ""), "Login");
            content.Add(new StringContent(senha ?? ""), "Senha");
            content.Add(new StringContent(perfil ?? "User"), "Perfil");

            var response = await _httpClient.PostAsync("Usuario", content);

            /// adicione estas linhas temporariamente para debugar
            if (!response.IsSuccessStatusCode)
            {
                /// Lê a string exata enviada pelo 'return BadRequest("...")' da sua API
                string motivoDoServidor = await response.Content.ReadAsStringAsync();

                /// Escreve no console de saída do Visual Studio (Janela Output/Saída)
                System.Diagnostics.Debug.WriteLine($"Falha na API: {motivoDoServidor}");

                /// Dispara a sua janela personalizada na Thread principal com uma mensagem amigável
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    MensagemWindow.Exibir(
                        "Não foi possível cadastrar",
                        "Verifique se as informações digitadas estão corretas ou se o login/e-mail já estão em uso no sistema.",
                        MensagemWindow.TipoMensagem.Aviso);
                });
            }

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

        /// <summary>
        /// Atualiza a foto de perfil do usuário e retorna a URL final que a API gerou para a foto.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="usuario"></param>
        /// <param name="caminhoFotoLocal"></param>
        /// <returns></returns>
        public async Task<string?> AtualizarComFotoAsync(string id, Usuario usuario, string caminhoFoto)
        {
            using var form = new MultipartFormDataContent();

            /// Vincula as strings obrigatórias (Use os nomes exatos das propriedades da classe da API)
            form.Add(new StringContent(usuario.Nome ?? ""), "Nome");
            form.Add(new StringContent(usuario.CPF ?? ""), "CPF");
            form.Add(new StringContent(usuario.Email ?? ""), "Email");
            form.Add(new StringContent(usuario.Login ?? ""), "Login");
            form.Add(new StringContent(usuario.Senha ?? ""), "Senha");
            form.Add(new StringContent(usuario.Perfil ?? "User"), "Perfil");
            form.Add(new StringContent(usuario.Ativo.ToString()), "Ativo");

            /// Trata e anexa o arquivo real utilizando o nome EXATO que a API espera: "ImagemPerfil"
            if (File.Exists(caminhoFoto))
            {
                var bytes = await File.ReadAllBytesAsync(caminhoFoto);
                var fileContent = new ByteArrayContent(bytes);

                string extensao = Path.GetExtension(caminhoFoto).ToLower();
                string tipoMime = extensao switch
                {
                    ".png" => "image/png",
                    _ => "image/jpeg"
                };
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(tipoMime);

                form.Add(fileContent, "ImagemPerfil", Path.GetFileName(caminhoFoto));
            }

            /// Envia a requisição PUT para a API
            var response = await _httpClient.PutAsync($"https://webregraphik.runasp.net/api/Usuario/{id}", form);

            if (!response.IsSuccessStatusCode)
            {
                string erroDaApi = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Erro retornado pela API: {erroDaApi}");
                return null; // Agora permitido pois alteramos o retorno do método para string?
            }

            /// Captura o JSON do usuário atualizado
            string conteudoSucesso = await response.Content.ReadAsStringAsync();

            /// Desserializa o objeto utilizando System.Text.Json
            var usuarioAtualizado = System.Text.Json.JsonSerializer.Deserialize<Usuario>(
                conteudoSucesso,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            /// Retorna a URL estável/final que a API gerou para a foto
            return usuarioAtualizado?.FotoPerfil;
        }

    }
}