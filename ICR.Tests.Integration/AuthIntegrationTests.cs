using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ICR.Tests.Integration
{
    // A interface IClassFixture configura o WebApplicationFactory para subir a API em memória
    // durante a execução dos testes, reaproveitando a instância para otimizar o tempo.
    public class AuthIntegrationTests : BaseIntegrationTest
    {
        public AuthIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        #region Cenários de Login (Autenticação)

        [Fact]
        public async Task Login_ComCredenciaisValidas_DeveRetornarTokenEStatusOk()
        {
            // Arrange: Prepara os dados de requisição com credenciais corretas
            var requestBody = new { Username = "admin", Password = "Password123!" };

            // Act: Dispara o POST para o endpoint de login
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", requestBody);

            // Assert: Verifica se a resposta foi 200 (OK) e se o token foi gerado
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadFromJsonAsync<AuthResponseSimulada>();
            Assert.NotNull(responseContent);
            Assert.False(string.IsNullOrEmpty(responseContent.Token));
            Assert.StartsWith("Bearer ", responseContent.Token);
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange: Senha incorreta
            var requestBody = new { Username = "admin", Password = "SenhaErrada" };

            // Act: Dispara o POST para o endpoint
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", requestBody);

            // Assert: Verifica se o acesso foi negado (401)
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_MuitasTentativasFalhas_DeveRetornarTooManyRequests()
        {
            // Arrange: Usuário tentando realizar força bruta (Brute Force)
            var requestBody = new { Username = "admin", Password = "SenhaErrada" };
            HttpStatusCode ultimoStatus = HttpStatusCode.OK;

            // Act: Executa o login 10 vezes em sequência muito rápida e captura o status
            for (int i = 0; i < 10; i++)
            {
                var response = await _client.PostAsJsonAsync("/api/v1/auth/login", requestBody);
                ultimoStatus = response.StatusCode;
            }

            // Assert: O sistema de Rate Limit (Proteção contra Brute Force) deve bloquear com 429
            // Obs: É necessário garantir que o `UseRateLimiter` está configurado no Program.cs
            Assert.Equal(HttpStatusCode.TooManyRequests, ultimoStatus);
        }

        #endregion

        #region Cenários de Proteção e Autorização

        [Fact]
        public async Task AcessoEndpointProtegido_SemAutenticacao_DeveRetornarUnauthorized()
        {
            // Arrange: Cliente sem nenhum token de autorização
            _client.DefaultRequestHeaders.Authorization = null;

            // Act: Tenta acessar uma rota que exige [Authorize]
            var response = await _client.GetAsync("/api/v1/dashboard"); // Exemplo de endpoint protegido

            // Assert: O acesso deve ser bloqueado com erro 401
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AcessoEndpointProtegido_ComTokenInvalido_DeveRetornarUnauthorized()
        {
            // Arrange: Adiciona um token com assinatura falsa, corrompido ou malformado
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid_payload.invalid_signature");

            // Act: Tenta acessar uma rota protegida
            var response = await _client.GetAsync("/api/v1/dashboard");

            // Assert: O middleware JWT vai rejeitar e retornar 401
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AcessoEndpointProtegido_ComTokenExpirado_DeveRetornarUnauthorized()
        {
            // Arrange: Gera ou obtém um JWT artificial com data de Expiração (exp) no passado
            var tokenExpirado = GerarTokenExpiradoParaTeste();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenExpirado);

            // Act: Tenta realizar a validação
            var response = await _client.GetAsync("/api/v1/dashboard");

            // Assert: O middleware validação do Token detecta a expiração pela claim 'exp' e nega o acesso
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        #endregion

        #region Cenários Baseados em Rotas (Role-Based Authorization)

        [Fact]
        public async Task AcessoEndpointAdmin_ComUsuarioComum_DeveRetornarForbidden()
        {
            // Arrange: Pega um token válido de um usuário normal (User)
            var tokenUsuarioComum = await ObterTokenParaPerfilAsync("user", "UserPass123!");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenUsuarioComum);

            // Act: Tenta acessar endpoint restrito a administradores
            var response = await _client.GetAsync("/api/v1/federation/admin-only-action");

            // Assert: O status deve ser 403 (Forbidden), indicando que tem conta, mas não tem permissão
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task AcessoEndpointAdmin_ComUsuarioAdmin_DeveRetornarSucesso()
        {
            // Arrange: Pega um token de um super usuário / admin real
            var tokenAdmin = await ObterTokenParaPerfilAsync("admin", "AdminPass123!");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

            // Act: Tenta acessar a mesma URL exclusiva de admin
            var response = await _client.GetAsync("/api/v1/federation"); 

            // Assert: Acesso concedido
            Assert.True(response.IsSuccessStatusCode);
        }

        #endregion

        #region Métodos Auxiliares
        
        // Simulação do retorno do Controller de Auth
        private class AuthResponseSimulada
        {
            public string Token { get; set; }
        }

        // Método auxiliar para obter um token rapidamente dado um usuário e senha previstos no banco de dados de testes
        private async Task<string> ObterTokenParaPerfilAsync(string username, string password)
        {
            var requestBody = new { Username = username, Password = password };
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", requestBody);
            
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadFromJsonAsync<AuthResponseSimulada>();
            // Remove a palavra "Bearer " se ela vir colada no token do backend
            return content.Token.Replace("Bearer ", "");
        }

        // Cria manualmente um Token JWT estruturalmente correto mas vencido
        private string GerarTokenExpiradoParaTeste()
        {
            // Em um ambiente real de testes, você utilizaria a classe de serviço da aplicação
            // ou instanciaria um JwtSecurityTokenHandler forçando a expiração no passado
            // para manter a consistência com a mesma chave secreta usada pela API local na memória.
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var keyStr = "s3T8PqA9wX1yB4kZ7mR2tV6nH0cL5fD8uJ3gQ9pE1rT6yW2sK4bM7hN0"; // A chave de teste do seu appsettings
            var keyBytes = System.Text.Encoding.ASCII.GetBytes(keyStr);

            var descriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("Name", "expired_user")
                }),
                Expires = DateTime.UtcNow.AddDays(-1), // DATA NO PASSADO
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        #endregion
    }
}