using ICR.Domain.Model.FederationAggregate;
using ICR.Domain.Model.UserRoleAgreggate;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ICR.Application.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        // IConfiguration será injetado pelo projeto API
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public object GenerateToken(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var secret = Environment.GetEnvironmentVariable("JWT_KEY")
                         ?? _configuration["JWT_KEY"];

            if (string.IsNullOrEmpty(secret))
                throw new InvalidOperationException("JWT secret not configured.");

            var key = Encoding.ASCII.GetBytes(secret);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim("scope", user.Scope.ToString())
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);

            return handler.WriteToken(token);
        }
    }
}
