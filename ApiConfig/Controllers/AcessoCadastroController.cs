using ApiConfig.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiConfig.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcessoCadastroController : ControllerBase
    {
        private readonly ILogger<AcessoCadastroController> _logger;

        /// <summary>
        /// Construtor da classe AcessoController, que recebe um logger e um serviço de Acesso para ser utilizado nas ações do controlador.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros.</param>
        public AcessoCadastroController(ILogger<AcessoCadastroController> logger)
        {
            _logger = logger;
        }

        [HttpPost("validar-token")]
        public async Task<IActionResult> ValidarToken(AcessoCadastro dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x =>
                    x.Email == dto.Email);

            if (usuario == null)
                return NotFound();

            if (usuario.TokenValidacao != dto.Token)
                return BadRequest();

            usuario.Ativo = true;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
