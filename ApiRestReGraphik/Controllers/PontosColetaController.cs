using ApiRestReGraphik.Models;
using ApiRestReGraphik.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiRestReGraphik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PontosColetaController : ControllerBase
    {
        private readonly PontosColetaService _pontosColetaService;
        private readonly ILogger<PontosColetaController> _logger;

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Construtor da classe PontosColetaController, que recebe um logger e um serviço de PontosColeta para ser utilizado nas ações do controlador.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros.</param>
        /// <param name="pontosColetaService">Serviço de PontosColeta para operações relacionadas.</param>
        /// <param name="configuration">Configuração para acessar as chaves de API e outras configurações necessárias.</param>
        /// <param name="httpClientFactory">Fábrica de HttpClient para criar instâncias de HttpClient para chamadas externas.</param>
        public PontosColetaController(
            ILogger<PontosColetaController> logger,
            PontosColetaService pontosColetaService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration) 
        {
            _logger = logger;
            _pontosColetaService = pontosColetaService;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration; 
        }


        /// <summary>
        ///  GET api/PontosColeta - Obtém dados diretamente da API externa do Google Maps.
        /// </summary>
        /// 
        /// <remarks>Responsável por listar os dados dos Pontos de Coleta. Retornando uma coleção de objetos detalhando informações técnicas e operacionais de cada ponto de coleta, 
        /// com atributos como ID, nome do ponto, cidade, estado, CEP e os tipos de resíduos aceitos.
        /// 
        /// Observação: Retorna um status 200 OK com os dados do ReGraphik ou um status 500 Internal Server Error em caso de falha.
        /// </remarks>
        /// <response code="200">Retorna os dados do ReGraphik.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _pontosColetaService.Listar();
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                // Loga o erro de comunicação com a API externa e retorna um status 502 Bad Gateway com uma mensagem de erro
                _logger.LogError(ex, "Falha na comunicação com a API externa do Google Maps.");
                return StatusCode(StatusCodes.Status502BadGateway, "Não foi possível obter os dados da API externa.");
            }
            catch (ArgumentException ex)
            {
                // Loga o erro de argumento inválido e retorna um status 400 Bad Request com a mensagem de erro
                _logger.LogWarning(ex, "Requisição inválida processada pelo serviço.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro
                _logger.LogError(ex, "Erro ao obter dados dos Pontos de Coleta.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro interno ao processar a solicitação.");
            }
        }

        /// <summary>
        /// POST api/PontosColeta/sincronizar?cidade=... - Busca os dados no Google e grava direto no Firebase.
        /// </summary>
        /// 
        /// <remarks>Responsável por buscar os dados no Google e gravar direto no Firebase. 
        /// Observação:Retorna um status 200 OK com uma mensagem de sucesso ou um status 400 Bad Request se o parâmetro 'cidade' for inválido ou um status 
        /// 500 Internal Server Error em caso de falha.</remarks>
        /// 
        /// <response code="200">Sincronização concluída com sucesso.</response>
        /// <response code="400">Parâmetro 'cidade' inválido ou ausente.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        [HttpPost("sincronizar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SincronizarCidade([FromQuery] string cidade)
        {
            if (string.IsNullOrWhiteSpace(cidade))
            {
                return BadRequest("O parâmetro 'cidade' é obrigatório para a sincronização.");
            }

            try
            {
                // Monta a URL da API do Google Maps usando o nome da cidade e a chave de API do appsettings.json
                var apiKey = _configuration["GoogleMaps:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return StatusCode(500, "Chave de API do Google Maps não configurada no servidor.");
                }

                if (_httpClient == null)
                {
                    return StatusCode(500, "Erro de infraestrutura: HttpClient não injetado.");
                }

                // Chama o serviço para sincronizar os dados da cidade com o Google Maps e obter o total de pontos salvos e ignorados por duplicidade
                var (salvo, ignorado) = await _pontosColetaService.SincronizarComGoogleMapsAsync(cidade, apiKey, _httpClient);

                return Ok(new
                {
                    Mensagem = $"Sincronização de '{cidade}' concluída com sucesso!",
                    PontosSalvos = salvo,
                    PontosIgnoradosPorDuplicidade = ignorado
                });
            }
            catch (ArgumentException ex)
            {
                // Loga o erro de argumento inválido e retorna um status 400 Bad Request com a mensagem de erro
                _logger.LogWarning(ex, "Requisição inválida processada pelo serviço.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro
                _logger.LogError(ex, "Erro ao sincronizar dados.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro interno ao processar a solicitação.");
            }   
        }

        /// <summary>
        /// GET api/PontosColeta/{id} - Obtém um ponto de coleta específico do ReGraphik com base no ID fornecido.
        /// </summary>
        /// 
        /// <remarks>Responsável por obter um ponto de coleta específico do ReGraphik com base no ID fornecido. 
        /// 
        /// Exemplo de resposta: 
        /// 
        /// {
        ///     "Id": "0d95265b-2757-424e-8ea9-445e8fd2a422",
        ///     "NomePonto": "Ponto de Coleta Central",
        ///     "Cidade": "São Paulo",
        ///     "Estado": "SP",
        ///     "Cep": "01000-000",
        ///     "ResiduosAceitos": "Papel, Plástico, Metal"
        /// }
        /// 
        /// Observação: Retorna um status 200 OK com os dados do resíduo, um status 404 Not Found se o resíduo não for encontrado ou 
        /// um status 500 Internal Server Error em caso de falha.
        /// 
        /// </remarks>
        /// 
        /// <param name="id">ID do ponto de coleta a ser obtido.</param>
        /// 
        /// <response code="200">Retorna os dados do ponto de coleta solicitado.</response>
        /// <response code="404">Ponto de coleta com o ID fornecido não encontrado.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var result = await _pontosColetaService.ObterPorId(id);
                if (result == null)
                {
                    return NotFound($"Ponto de coleta com ID {id} não encontrado.");
                }
                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                // Loga o erro de comunicação com a API externa e retorna um status 404 Not Found com uma mensagem de erro
                _logger.LogError(ex, $"Falha na comunicação com a API externa ao obter ponto de coleta com ID {id}.");
                return StatusCode(StatusCodes.Status404NotFound, $"Não foi possível obter os dados do ponto de coleta com ID {id} devido a um erro de comunicação com a API externa.");
            }
            catch (Exception ex)    
            {
                // Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro
                _logger.LogError(ex, $"Erro ao obter dados do Ponto de Coleta com ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao processar a solicitação do ponto de coleta");
            }
        }

        /// <summary>
        /// POST api/PontosColeta - Criar um novo ponto de coleta no ReGraphik.
        /// </summary>
        /// 
        /// <remarks>Responsável por criar um novo ponto de coleta no ReGraphik.
        /// 
        /// Requisitos de validação:
        /// - Id: Deve ser um guid gerado automaticamente pelo sistema. (ex: "0d95265b-2757-424e-8ea9-445e8fd2a422")
        /// - NomePonto: Deve ser uma string não vazia. (ex: "Ponto de Coleta Central", "Ponto de Coleta Norte", etc.)
        /// - Cidade: Deve ser uma string não vazia. (ex: "São Paulo", "Rio de Janeiro", etc.)
        /// - Estado: Deve ser uma string não vazia. (ex: "SP", "RJ", etc.)
        /// - Cep: Deve ser uma string no formato de CEP brasileiro. (ex: "01000-000", "20000-000", etc.)
        /// - ResiduosAceitos: Deve ser uma string não vazia listando os tipos de resíduos aceitos. (ex: "Papel, Plástico, Metal", "Vidro, Eletrônicos", etc.)
        /// - Lat: Deve ser um número decimal representando a latitude do ponto de coleta. (ex: -23.55052, -22.90685, etc.)
        /// - Lng: Deve ser um número decimal representando a longitude do ponto de coleta. (ex: -46.633308, -43.172896, etc.)
        /// 
        /// Observação: Retorna um status 201 Created com os dados do ponto de coleta criado, um status 400 Bad Request se a requisição for inválida ou
        /// um status 500 Internal Server Error em caso de falha.
        /// </remarks>
        /// 
        /// <param name="pontoColeta">Objeto do tipo PontoColeta a ser criado.</param>
        /// 
        /// <response code="201">Ponto de coleta criado com sucesso.</response>
        /// <response code="400">Requisição inválida, ponto de coleta não fornecido ou dados incorretos.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] PontosColeta pontoColeta)
        {
            try
            {
                if (pontoColeta == null)
                {
                    return BadRequest("Ponto de coleta inválido.");
                }

                await _pontosColetaService.Criar(pontoColeta);

                return CreatedAtAction(nameof(GetById), new { id = pontoColeta.Id }, pontoColeta);
            }
            catch (ArgumentException ex)
            {
                // Loga o erro de argumento inválido e retorna um status 400 Bad Request com a mensagem de erro
                _logger.LogWarning(ex, "Requisição inválida processada para criar ponto de coleta.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro
                _logger.LogError(ex, $"Erro ao criar dados do Ponto de Coleta.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro interno ao processar a solicitação.");
            }
        }

        /// <summary>
        /// PUT api/PontosColeta/{id} - Atualizar um ponto de coleta existente no ReGraphik com base no ID fornecido.
        /// </summary>
        /// 
        /// <remarks>Responsável por atualizar um ponto de coleta existente no ReGraphik com base no ID fornecido.
        /// Observação: Retorna um status 200 OK com os dados do ponto de coleta atualizado, um status 400 Bad Request se a requisição for inválida,
        /// um status 404 Not Found se o ponto de coleta não for encontrado ou um status 500 Internal Server Error em caso de falha.</remarks>
        /// 
        /// <param name="id"></param>
        /// <param name="pontoColeta"></param>
        /// 
        /// <response code="200">Ponto de coleta atualizado com sucesso.</response>
        /// <response code="400">Requisição inválida, ponto de coleta não fornecido ou dados incorretos.</response>
        /// <response code="404">Ponto de coleta com o ID fornecido não encontrado.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(string id, [FromBody] PontosColeta pontoColeta)
        {
            try
            {
                if (pontoColeta == null || id != pontoColeta.Id)
                {
                    return BadRequest($"ID do ponto de coleta inválido.");
                }

                var existing = await _pontosColetaService.ObterPorId(id);
                if (existing == null)
                {
                    return NotFound($"Ponto de coleta com ID {id} não encontrado.");
                }

                await _pontosColetaService.Atualizar(id, pontoColeta);
                return Ok($"Ponto de coleta com ID {id} atualizado com sucesso.");
            }
            catch (ArgumentException ex)
            {
                // Loga o erro de argumento inválido e retorna um status 400 Bad Request com a mensagem de erro
                _logger.LogWarning(ex, $"Requisição inválida processada para atualizar ponto de coleta com ID {id}.");
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                // Loga o erro de comunicação com a API externa e retorna um status 404 Not Found com uma mensagem de erro
                _logger.LogError(ex, $"Falha na comunicação com a API externa ao atualizar ponto de coleta com ID {id}.");
                return StatusCode(StatusCodes.Status404NotFound, $"Não foi possível atualizar os dados do ponto de coleta com ID {id} devido a um erro de comunicação com a API externa.");
            }
            catch (Exception ex)
            {
                // Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro
                _logger.LogError(ex, $"Erro ao atualizar dados do Ponto de Coleta com ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao processar a atualização do ponto de coleta");
            }
        }

        /// <summary>
        /// DELETE api/PontosColeta/{id} - Excluir um ponto de coleta do ReGraphik com base no ID fornecido. 
        /// </summary>
        /// 
        /// <remarks>Responsável por excluir um ponto de coleta do ReGraphik com base no ID fornecido. 
        /// Observação: Retorna um status 200 OK se a exclusão for bem-sucedida, um status 404 Not Found se o ponto de coleta não for 
        /// encontrado ou um status 500 Internal Server Error em caso de falha.</remarks>
        /// 
        /// <param name="id">ID do ponto de coleta a ser excluído.</param>
        /// 
        /// <response code="200">Ponto de coleta excluído com sucesso.</response>
        /// <response code="404">Ponto de coleta com o ID fornecido não encontrado.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var existing = await _pontosColetaService.ObterPorId(id);
                if (existing == null)
                {
                    return NotFound($"Ponto de coleta com ID {id} não encontrado.");
                }

                await _pontosColetaService.Excluir(id);
                return Ok($"Ponto de coleta com ID {id} excluído com sucesso.");
            }
            catch (HttpRequestException ex)
            {
                // Loga o erro de comunicação com a API externa e retorna um status 404 Not Found com uma mensagem de erro
                _logger.LogError(ex, $"Falha na comunicação com a API externa ao excluir ponto de coleta com ID {id}.");
                return StatusCode(StatusCodes.Status404NotFound, $"Não foi possível excluir os dados do ponto de coleta com ID {id} devido a um erro de comunicação com a API externa.");
            }
            catch (Exception ex)
            {
                // Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro
                _logger.LogError(ex, $"Erro ao excluir dados do Ponto de Coleta com ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao processar a exclusão do ponto de coleta");
            }
        }
    }
}