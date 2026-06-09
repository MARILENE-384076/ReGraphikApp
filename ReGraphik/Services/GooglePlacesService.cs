using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ReGraphik.Models;

namespace ReGraphik.Services
{
    /// <summary>
    /// Serviço responsável pela comunicação com a API do Google Places.
    /// Realiza buscas textuais de locais e extrai dados detalhados como localização geográfica e contatos.
    /// </summary>
    public class GooglePlacesService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Chave de autenticação da API do Google Maps. 
        /// Prefira buscar de variáveis de ambiente: Environment.GetEnvironmentVariable("GOOGLE_PLACES_KEY")
        /// </summary>
        private readonly string _apiKey = "AIzaSyCPeDl0hmzFeROHcUxPbnQUvAhOA_N-ros";

        /// <summary>
        /// Busca postos de coleta baseados no tipo de resíduo e na cidade informada.
        /// Realiza chamadas secundárias seguras para obter telefone e site de cada local,
        /// tratando as exceções para evitar quebras no aplicativo ou vazamento de credenciais via logs.
        /// </summary>
        /// <param name="cidade">Nome da cidade para a busca (Ex: "São Paulo").</param>
        /// <param name="material">Tipo de material ou resíduo (Ex: "Plástico").</param>
        /// <returns>Retorna uma <see cref="List{PontosColeta}"/> contendo os locais encontrados e seus detalhes.</returns>
        public async Task<List<PontosColeta>> BuscarPostosNoBrasilAsync(string cidade, string material)
        {
            var listaDePostos = new List<PontosColeta>();

            try
            {
                /// Monta a string de busca de forma segura, escapando caracteres especiais
                string termoBusca = $"{material} em {cidade}";
                string searchUrl = $"https://maps.googleapis.com/maps/api/place/textsearch/json?query={Uri.EscapeDataString(termoBusca)}&key={_apiKey}";

                string jsonResposta;

                ///Protege a requisição HTTP principal contra vazamento da API Key nos logs
                try
                {
                    jsonResposta = await _httpClient.GetStringAsync(searchUrl);
                }
                catch (HttpRequestException)
                {
                    ///Omitimos o parâmetro de exceção para não expor a URL completa e a API KEY
                    System.Diagnostics.Debug.WriteLine("[SEGURANÇA] Falha na comunicação com a API de busca. Detalhes ocultados para proteger as credenciais.");
                    return listaDePostos;
                }

                /// Processamento nativo do JSON recebido
                using (JsonDocument doc = JsonDocument.Parse(jsonResposta))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("results", out JsonElement resultados))
                    {
                        int idContador = 1;

                        /// Itera sobre cada local retornado pela pesquisa principal
                        foreach (JsonElement item in resultados.EnumerateArray())
                        {
                            string placeId = item.TryGetProperty("place_id", out JsonElement placeIdElement)
                                ? placeIdElement.GetString()
                                : idContador.ToString();

                            /// Cria o objeto instanciando com valores padrão seguros
                            var novoPonto = new PontosColeta
                            {
                                Id = placeId,
                                NomePonto = item.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() : "Sem Nome",
                                Cidade = cidade,
                                Estado = "UF",
                                CEP = "00000-000",
                                ResiduosAceitos = material,
                                Lat = 0.0,
                                Lng = 0.0,
                                telefone = "Não informado",
                                site = "Não informado"
                            };

                            /// Isola a leitura de coordenadas geográficas
                            /// Evita que um nó de JSON corrompido quebre o loop de todos os outros pontos
                            try
                            {
                                if (item.TryGetProperty("geometry", out JsonElement geometry) &&
                                    geometry.TryGetProperty("location", out JsonElement location))
                                {
                                    if (location.TryGetProperty("lat", out JsonElement latElement))
                                        novoPonto.Lat = latElement.GetDouble();

                                    if (location.TryGetProperty("lng", out JsonElement lngElement))
                                        novoPonto.Lng = lngElement.GetDouble();
                                }
                            }
                            catch (Exception jsonEx)
                            {
                                System.Diagnostics.Debug.WriteLine($"Erro ao processar coordenadas do ponto {placeId}: {jsonEx.Message}");
                            }

                            /// Tenta resgatar o endereço formatado caso esteja disponível
                            if (item.TryGetProperty("formatted_address", out JsonElement enderecoElement))
                            {
                                novoPonto.Cidade = enderecoElement.GetString() ?? cidade;
                            }

                            ///Consulta e processamento de dados adicionais de contato Telefone e Site
                            /// Apenas executa se um Place ID válido foi recuperado
                            if (!string.IsNullOrEmpty(placeId) && placeId != idContador.ToString())
                            {
                                string detailsUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=formatted_phone_number,website&key={_apiKey}";

                                try
                                {
                                    string detailsJson = await _httpClient.GetStringAsync(detailsUrl);
                                    using (JsonDocument detailsDoc = JsonDocument.Parse(detailsJson))
                                    {
                                        JsonElement detailsRoot = detailsDoc.RootElement;
                                        if (detailsRoot.TryGetProperty("result", out JsonElement detailsResult))
                                        {
                                            /// Mapeia o telefone formatado
                                            if (detailsResult.TryGetProperty("formatted_phone_number", out JsonElement phoneElement))
                                            {
                                                novoPonto.telefone = phoneElement.GetString();
                                            }

                                            /// Mapeia o website
                                            if (detailsResult.TryGetProperty("website", out JsonElement siteElement))
                                            {
                                                novoPonto.site = siteElement.GetString();
                                            }
                                        }
                                    }
                                }
                                catch (HttpRequestException)
                                {
                                    /// Evita vazamento de URL na chamada de detalhes
                                    System.Diagnostics.Debug.WriteLine($"[SEGURANÇA] Falha de rede nos detalhes do local {placeId}. Chave protegida.");
                                }
                                catch (Exception exDet)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Erro genérico ao obter detalhes do local {placeId}: {exDet.Message}");
                                }
                            }

                            listaDePostos.Add(novoPonto);
                            idContador++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                /// Impede que qualquer erro não previsto cause o fechamento abrupto da interface WPF
                System.Diagnostics.Debug.WriteLine("Erro crítico geral no processamento do Google Places: " + ex.Message);
            }

            return listaDePostos;
        }
    }
}