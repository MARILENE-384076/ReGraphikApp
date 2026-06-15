using ApiConfig.Models;
using ApiConfig.Services;
using FirebaseAdmin.Auth.Hash;
using Microsoft.AspNetCore.Mvc;

namespace ApiConfig.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioCadastroController : ControllerBase
    {
        private readonly ILogger<UsuarioCadastroController> _logger;
        private readonly TokenService _tokenService;

        /// <summary>
        /// Construtor da classe UsuarioCadastroController, que recebe um logger e um serviço de Usuario para ser utilizado nas ações do controlador.
        /// </summary>
        /// <param name="logger">Logger para registrar informações e erros.</param>
        /// <param name="tokenService"></param>
        public UsuarioCadastroController(
            ILogger<UsuarioCadastroController> logger,
            TokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(UsuarioCadastro dto)
        {
            var token = _tokenService.GerarToken();

            var usuario = new UsuarioCadastro
            {
                Nome = dto.Nome,
                CPF = dto.CPF,
                Email = dto.Email,
                Login = dto.Login,
                Senha = dto.Senha,

                TokenValidacao = token,
                Ativo = false
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await _emailService.EnviarToken(
                dto.Email,
                token);

            return Ok();
        }

    }
}
