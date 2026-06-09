using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReGraphik.Services
{
    public class ImgurApiService
    {
        private const string ClientId = "4901967be259bb8";

        /// <summary>
        /// Faz o upload de uma imagem para o Imgur de forma 100% gratuita e anônima.
        /// </summary>
        public async Task<string> EnviarFotoPerfilAsync(string caminhoArquivoLocal)
        {
            if (!File.Exists(caminhoArquivoLocal))
                throw new FileNotFoundException("O arquivo de imagem selecionado não foi encontrado.");

            using (var client = new HttpClient())
            {
                // CORREÇÃO: O Imgur exige que a requisição tenha um User-Agent para não retornar erro 403/400
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ReGraphikApp/1.0");

                // Configura a autenticação básica que o Imgur pede
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", ClientId);

                // Lê os bytes da imagem local
                byte[] imagemBytes = await File.ReadAllBytesAsync(caminhoArquivoLocal);

                using (var content = new MultipartFormDataContent())
                {
                    var imageContent = new ByteArrayContent(imagemBytes);

                    // Tratamento dinâmico do tipo de imagem (.png, .jpg, etc)
                    string extensao = Path.GetExtension(caminhoArquivoLocal).ToLower();
                    string contentType = extensao == ".png" ? "image/png" : "image/jpeg";
                    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

                    content.Add(imageContent, "image");

                    // Envia para os servidores do Imgur
                    var response = await client.PostAsync("https://api.imgur.com/3/image", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var erroDoServidor = await response.Content.ReadAsStringAsync();

                        // Melhoria no diagnóstico para sabermos o motivo real se o Imgur rejeitar
                        throw new Exception($"Status: {response.StatusCode}. Detalhes: {erroDoServidor}");
                    }

                    // Lê o link da internet gerado pelo Imgur
                    var jsonResposta = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(jsonResposta))
                    {
                        // Extrai a URL direta da imagem (ex: https://i.imgur.com/abcde.png)
                        string urlPublica = doc.RootElement.GetProperty("data").GetProperty("link").GetString();
                        return urlPublica;
                    }
                }
            }
        }
    }
}

