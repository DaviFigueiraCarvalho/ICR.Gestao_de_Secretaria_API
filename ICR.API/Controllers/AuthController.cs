using BCrypt.Net;
using ICR.Application.Services;
using ICR.Domain.DTOs;
using ICR.Domain.Model.UserRoleAgreggate;
using ICR.Infra.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ICR.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRoleRepository _repository;
        private readonly TokenService _tokenService;

        public AuthController(
            IUserRoleRepository repository,
            TokenService tokenService)
        {
            _repository = repository;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("Login")]
        public async Task<IActionResult> Login([FromBody] AuthRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username e senha são obrigatórios, gênio");

            var user = await _repository.GetUserByUsernameAsync(dto.Username);

            if (user == null)
                return Unauthorized("Usuário ou senha inválidos");

            // Confere hash direito, não inventa comparação idiota
            var passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!passwordOk)
                return Unauthorized("Usuário ou senha inválidos");

            var token = _tokenService.GenerateToken(new Domain.Model.UserRoleAgreggate.User
            {
                Id = user.Id,
                Username = user.Username,
                Scope = user.Scope
            });

            return Ok(new AuthResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Scope = user.Scope,
                Token = $"Bearer {token}"
            });
        }
    }
}
