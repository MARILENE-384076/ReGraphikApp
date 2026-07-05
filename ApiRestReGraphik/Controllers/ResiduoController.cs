using ApiRestReGraphik.Models;
using ApiRestReGraphik.Models.DTOs;
using ApiRestReGraphik.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ApiRestReGraphik.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResiduoController : ControllerBase
    {
        private readonly ResiduoService _residuoService;
        private readonly ILogger<ResiduoController> _logger;
        private readonly IWebHostEnvironment _env;

        private const string ImgBbApiKey = "68af9837d10d537215a04d7b1b60aa3c";

        /// <summary>
        /// Construtor da classe ResiduoController, que recebe um logger e um serviço de Residuo para ser utilizado nas ações do controlador.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros.</param>
        /// <param name="residuoService">Serviço de Residuo para operações relacionadas.</param>
        /// <param name="env"></param>
        public ResiduoController(ILogger<ResiduoController> logger, ResiduoService residuoService, IWebHostEnvironment env)
        {
            _logger = logger;
            _residuoService = residuoService;
            _env = env;
        }


        /// <summary>
        ///  GET api/Residuo - Obtém dados do Residuo e retorna uma lista de resíduos cadastrados no ReGraphik.
        /// </summary>
        /// 
        /// <remarks>Responsável por listar os dados do Residuo. Retornando uma coleção de objetos detalhando informações técnicas e operacionais de cada resíduo, 
        /// com atributos como ID, tipo de resíduo, origem, especificação, projeto associado, quantidade, data de cadastro, condição, dimensões, observações, anexos e status.
        /// 
        /// Observação: Retorna um status 200 OK com os dados do ReGraphik ou um status 500 Internal Server Error em caso de falha.
        /// </remarks>
        /// 
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
                /// Obtém a lista de resíduos
                var residuos = await _residuoService.Listar();
                var result = residuos.Select(r => MapearParaDto(r));
                return Ok(result);
            }
            /// Loga o erro de argumento inválido e retorna um status 400 Bad Request com a mensagem de erro
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Requisição inválida ao listar os resíduos");
                return BadRequest("Requisição inválida ao listar os resíduos");
            }
            /// Loga o erro de requisição HTTP e retorna um status 404 Not Found com a mensagem de erro
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Falha ao listar os resíduos");
                return StatusCode(StatusCodes.Status404NotFound, "Não foi possível listar os resíduos.");
            }
            /// Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro genérica
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao listar os resíduos");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro interno ao processar a solicitação.");
            }
        }

        /// <summary>
        /// GET api/Residuo/{id} - Obtém um resíduo específico do ReGraphik com base no ID fornecido.
        /// </summary>
        /// 
        /// <remarks>Responsável por obter um resíduo específico do ReGraphik com base no ID fornecido. 
        /// 
        /// Exemplo de resposta: 
        /// 
        /// {
        ///     "Id": -NxYZ123456789,
        ///     "IdUsuario": 2,
        ///     "TipoResiduo": "Papel",
        ///     "Origem": "Escritório",
        ///     "Especificacao": "Papel A4 usado",
        ///     "Projeto": "Projeto X",
        ///     "Quantidade": 10.5,
        ///     "DataCadastro": "2024-06-01T12:00:00",
        ///     "Condicao": "Usado",
        ///     "DimensoesCm": 30.0,
        ///     "DimensoesLm": 10.0,
        ///     "Observacao": "Papel reciclável",
        ///     "Anexo": "http://example.com/anexo1.jpg",
        ///     "Status": "Disponível"
        /// }
        /// 
        /// Observação: Retorna um status 200 OK com os dados do resíduo, um status 404 Not Found se o resíduo não for encontrado ou 
        /// um status 500 Internal Server Error em caso de falha.
        /// 
        /// </remarks>
        /// 
        /// <param name="id">ID do resíduo a ser obtido.</param>
        /// 
        /// <response code="200">Retorna os dados do resíduo solicitado.</response>
        /// <response code="404">Resíduo com o ID fornecido não encontrado.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id)
        {
            if (!InputSanitizationService.IdEhSeguro(id))
                return BadRequest("ID inválido ou com caracteres não permitidos.");

            try
            {
                var result = await _residuoService.ObterPorId(id);
                if (result == null)
                {
                    return NotFound($"Resíduo com ID {id} não encontrado.");
                }

                var dto = MapearParaDto(result);
                return Ok(dto);
            }
            catch (HttpRequestException ex)
            {
                /// Loga o erro de comunicação com a API externa e retorna um status 404 Not Found com uma mensagem de erro
                _logger.LogError(ex, $"Falha ao obter resíduo com ID {id}.");
                return StatusCode(StatusCodes.Status404NotFound, $"Não foi possível obter os dados do resíduo com ID {id}.");
            }
            catch (Exception ex)
            {
                /// Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro genérica
                _logger.LogError(ex, $"Erro ao obter dados do Residuo com ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Não foi possível obter os dados do resíduo com ID {id}.");
            }
        }

        /// <summary>
        /// POST api/Residuo - Criar um novo resíduo no ReGraphik.
        /// </summary>
        /// 
        /// <remarks>Responsável por criar um novo resíduo no ReGraphik.
        /// 
        /// Requisitos de validação:
        /// - Id: Deve ser um guid gerado automaticamente pelo sistema. (ex: "0d95265b-2757-424e-8ea9-445e8fd2a422")
        /// - IdUsuario: Deve ser um guid representando o ID do usuário que está criando o resíduo. (ex: "0d95265b-2757-424e-8ea9-445e8fd2a422")
        /// - TipoResiduo: Deve ser uma string não vazia. (ex: "Papel", "Plástico", "Metal", etc.)
        /// - Origem: Deve ser uma string não vazia. (ex: "Escritório", "Indústria", "Residencial", etc.)
        /// - Especificacao: Deve ser uma string não vazia. (ex: "Papel A4 usado", "Garrafas PET limpas", "Latas de alumínio amassadas", etc.)
        /// - Projeto: Deve ser uma string não vazia. (ex: "Projeto X", "Projeto Y", "Projeto Z", etc.)
        /// - Quantidade: Deve ser um número decimal positivo. (ex: 10.5, 20.0, 50.75, etc.)
        /// - DataCadastro: Deve ser uma data válida. (ex: "2024-06-01T12:00:00", "2024-07-15T09:30:00", etc.)
        /// - Condicao: Deve ser uma string não vazia. (ex: "Novo", "Usado", "Reciclado", etc.)
        /// - DimensoesCm: Deve ser um número decimal positivo. (ex: 30.0, 50.5, 100.75, etc.)
        /// - DimensoesLm: Deve ser um número decimal positivo. (ex: 10.0, 20.5, 50.75, etc.)
        /// - Observacao: Deve ser uma string não vazia. (ex: "Papel reciclável", "Plástico limpo", "Metal amassado", etc.)
        /// - Anexo: Deve ser uma URL válida. (ex: "http://example.com/anexo1.jpg", "http://example.com/anexo2.jpg", etc.)
        /// - Status: Deve ser uma string não vazia. (ex: "Ativo", "Inativo", "Pendente", etc.)
        /// 
        /// Observação: Retorna um status 201 Created com os dados do resíduo criado, um status 400 Bad Request se a requisição for inválida ou
        /// um status 500 Internal Server Error em caso de falha.
        /// </remarks>
        /// 
        /// <param name="dto">Objeto do tipo Residuo a ser criado.</param>
        /// 
        /// <response code="201">Resíduo criado com sucesso.</response>
        /// <response code="400">Requisição inválida, resíduo não fornecido ou dados incorretos.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromForm] ResiduoDto dto)
        {
            try
            {
                string linkDaImagemWeb = "";

                /// Se o usuário enviou uma imagem, transformamos ela em Link de Internet
                if (dto.Imagem != null)
                {
                    using var httpClient = new HttpClient();
                    using var content = new MultipartFormDataContent();

                    /// Converte a imagem recebida em bytes
                    using var ms = new MemoryStream();
                    await dto.Imagem.CopyToAsync(ms);
                    var byteData = ms.ToArray();

                    content.Add(new ByteArrayContent(byteData, 0, byteData.Length), "image", dto.Imagem.FileName);

                    /// SUA CHAVE DA API DO IMGBB AQUI
                    string apiKey = "68af9837d10d537215a04d7b1b60aa3c";

                    var response = await httpClient.PostAsync($"https://api.imgbb.com/1/upload?key={apiKey}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        using var jsonDoc = JsonDocument.Parse(jsonString);

                        /// Pega a URL direta da imagem criada pelo ImgBB
                        linkDaImagemWeb = jsonDoc.RootElement
                            .GetProperty("data")
                            .GetProperty("url")
                            .GetString();
                    }
                    else
                    {
                        throw new Exception("Falha ao transformar a imagem em link no servidor externo.");
                    }
                }

                /// Criamos o objeto já com o LINK FINAL da internet
                var novoResiduo = new Residuo
                {
                    TipoResiduo = InputSanitizationService.SanitizarTexto(dto.TipoResiduo),
                    Origem = InputSanitizationService.SanitizarTexto(dto.Origem),
                    Especificacao = InputSanitizationService.SanitizarTexto(dto.Especificacao),
                    Projeto = dto.Projeto,
                    Quantidade = dto.Quantidade,
                    DataCadastro = dto.DataCadastro == default ? DateTime.UtcNow : dto.DataCadastro,
                    Condicao = dto.Condicao,
                    DimensoesCm = dto.DimensoesCm,
                    DimensoesLm = dto.DimensoesLm,
                    Observacao = InputSanitizationService.SanitizarTexto(dto.Observacao, 2000),
                    Anexo = linkDaImagemWeb, // <-- Aqui vai o link puro da internet (ex: https://i.ibb.co/XYZ/foto.png)
                    Status = dto.Status
                };

                var erros = InputSanitizationService.ValidarResiduo(novoResiduo);
                if (erros.Any()) return BadRequest(new { mensagem = "Dados inválidos.", erros });

                await _residuoService.Criar(novoResiduo);

                var dtoRetorno = MapearParaDto(novoResiduo);
                return CreatedAtAction(nameof(GetById), new { id = novoResiduo.Id }, dtoRetorno);
            }
            catch (ArgumentException ex)
            {
                /// Loga o erro de argumento inválido e retorna um status 400 Bad Request com a mensagem de erro
                _logger.LogWarning(ex, "Requisição inválida processada para criar resíduo.");
                return BadRequest("Requisição inválida processada pelo serviço.");
            }
            catch (Exception ex)
            {
                /// Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro
                _logger.LogError(ex, $"Erro ao criar dados do resíduo.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro interno ao processar a solicitação.");
            }
        }

        /// <summary>
        /// PUT api/Residuo/{id} - Atualizar um resíduo existente no ReGraphik com base no ID fornecido.
        /// </summary>
        /// 
        /// <remarks>Responsável por atualizar um resíduo existente no ReGraphik com base no ID fornecido.
        /// Observação: Retorna um status 200 OK com os dados do resíduo atualizado, um status 400 Bad Request se a requisição for inválida,
        /// um status 404 Not Found se o resíduo não for encontrado ou um status 500 Internal Server Error em caso de falha.</remarks>
        /// 
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// 
        /// <response code="200">Resíduo atualizado com sucesso.</response>
        /// <response code="400">Requisição inválida, resíduo não fornecido ou dados incorretos.</response>
        /// <response code="404">Resíduo com o ID fornecido não encontrado.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(string id, [FromBody] ResiduoDto dto)
        {
            if (!InputSanitizationService.IdEhSeguro(id))
                return BadRequest("ID inválido ou com caracteres não permitidos.");

            if (dto == null)
                return BadRequest("Dados de atualização inválidos.");

            try
            {
                var existing = await _residuoService.ObterPorId(id);
                if (existing == null)
                {
                    return NotFound($"Resíduo com ID {id} não encontrado.");
                }

                // CORREÇÃO: Mantém o link da imagem atual, a menos que uma nova imagem seja enviada
                string caminhoImagem = existing.Anexo;
                if (dto.Imagem != null)
                {
                    caminhoImagem = await EnviarImagemParaImgBb(dto.Imagem);
                }

                existing.TipoResiduo = InputSanitizationService.SanitizarTexto(dto.TipoResiduo);
                existing.Origem = InputSanitizationService.SanitizarTexto(dto.Origem);
                existing.Especificacao = InputSanitizationService.SanitizarTexto(dto.Especificacao);
                existing.Projeto = dto.Projeto;
                existing.Quantidade = dto.Quantidade;
                existing.Condicao = dto.Condicao;
                existing.DimensoesCm = dto.DimensoesCm;
                existing.DimensoesLm = dto.DimensoesLm;
                existing.Observacao = InputSanitizationService.SanitizarTexto(dto.Observacao, 2000);
                existing.Anexo = caminhoImagem; // Grava a URL da web
                existing.Status = dto.Status;

                await _residuoService.Atualizar(id, existing);
                return Ok(new { mensagem = $"Resíduo com ID {id} atualizado com sucesso." });
            }
            catch (ArgumentException ex)
            {
                /// Loga o erro de argumento inválido e retorna um status 400 Bad Request com a mensagem de erro
                _logger.LogWarning(ex, $"Requisição inválida processada para atualizar resíduo com ID {id}.");
                return BadRequest("Requisição inválida processada pelo serviço.");
            }
            catch (HttpRequestException ex)
            {
                /// Loga o erro de requisição HTTP e retorna um status 404 Not Found com a mensagem de erro
                _logger.LogError(ex, $"Falha ao atualizar residuo com ID {id}.");
                return StatusCode(StatusCodes.Status404NotFound, $"Não foi possível atualizar os dados do residuo com ID {id}.");
            }
            catch (Exception ex)
            {
                /// Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro
                _logger.LogError(ex, $"Erro ao atualizar dados do Residuo com ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar a solicitação.");
            }
        }

        /// <summary>
        /// DELETE api/Residuo/{id} - Excluir um resíduo do ReGraphik com base no ID fornecido. 
        /// </summary>
        /// 
        /// <remarks>Responsável por excluir um resíduo do ReGraphik com base no ID fornecido. 
        /// Observação: Retorna um status 200 OK se a exclusão for bem-sucedida, um status 404 Not Found se o resíduo não for 
        /// encontrado ou um status 500 Internal Server Error em caso de falha.</remarks>
        /// 
        /// <param name="id">ID do resíduo a ser excluído.</param>
        /// 
        /// <response code="200">Resíduo excluído com sucesso.</response>
        /// <response code="404">Resíduo com o ID fornecido não encontrado.</response>
        /// <response code="500">Ocorreu um erro ao processar a solicitação.</response>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            if (!InputSanitizationService.IdEhSeguro(id))
                return BadRequest("ID inválido ou com caracteres não permitidos.");

            try
            {
                var existing = await _residuoService.ObterPorId(id);
                if (existing == null)
                {
                    return NotFound($"Resíduo com ID {id} não encontrado.");
                }

                await _residuoService.Excluir(id);
                return Ok($"Resíduo com ID {id} excluído com sucesso.");
            }
            catch (HttpRequestException ex)
            {
                /// Loga o erro de comunicação com a API externa e retorna um status 404 Not Found com uma mensagem de erro
                _logger.LogError(ex, $"Falha ao excluir resíduo com ID {id}.");
                return StatusCode(StatusCodes.Status404NotFound, $"Não foi possível excluir os dados do resíduo com ID {id}.");
            }
            catch (Exception ex)
            {
                /// Loga o erro genérico e retorna um status 500 Internal Server Error com uma mensagem de erro genérica
                _logger.LogError(ex, $"Erro ao excluir dados do Residuo com ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar a solicitação.");
            }
        }

        /// <summary>
        /// Mapeador atualizado para a nova estrutura de ResiduoDto
        /// </summary>
        private static ResiduoDto MapearParaDto(Residuo residuo) => new ResiduoDto
        {
            Id = residuo.Id,
            TipoResiduo = residuo.TipoResiduo,
            Origem = residuo.Origem,
            Especificacao = residuo.Especificacao,
            Projeto = residuo.Projeto,
            Quantidade = residuo.Quantidade,
            DataCadastro = residuo.DataCadastro,
            Condicao = residuo.Condicao,
            DimensoesCm = residuo.DimensoesCm,
            DimensoesLm = residuo.DimensoesLm,
            Observacao = residuo.Observacao,
            Anexo = residuo.Anexo,
            Status = residuo.Status
        };

        /// <summary>
        /// Método auxiliar para isolar o envio de imagens ao ImgBB
        /// </summary>
        private async Task<string> EnviarImagemParaImgBb(IFormFile imagem)
        {
            using var httpClient = new HttpClient();
            using var content = new MultipartFormDataContent();

            using var ms = new MemoryStream();
            await imagem.CopyToAsync(ms);
            var byteData = ms.ToArray();

            content.Add(new ByteArrayContent(byteData, 0, byteData.Length), "image", imagem.FileName);

            var response = await httpClient.PostAsync($"https://api.imgbb.com/1/upload?key={ImgBbApiKey}", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var jsonDoc = JsonDocument.Parse(jsonString);

                return jsonDoc.RootElement
                    .GetProperty("data")
                    .GetProperty("url")
                    .GetString() ?? "";
            }

            throw new Exception("Falha ao transformar a imagem em link no servidor externo ImgBB.");
        }
    }
}