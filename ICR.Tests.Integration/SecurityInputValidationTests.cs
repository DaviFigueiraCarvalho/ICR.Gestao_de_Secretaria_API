using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ICR.Tests.Integration
{
    public class SecurityInputValidationTests : BaseIntegrationTest
    {
        public SecurityInputValidationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        // Teste: Tratamento de injeção de SQL
        [Fact]
        public async Task Post_WithSqlInjectionPayload_ShouldReturnBadRequestOrUnauthorized()
        {
            // Arrange: Payload com tentativa clássica de bypass de autenticação via SQL Injection
            var payload = new { Username = "admin' OR 1=1 --", Password = "password" };

            // Act: Dispara o POST para o endpoint
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

            // Assert: A API nunca deve retornar 500 (Internal Server Error) vazando dados de banco. 
            // O Entity Framework previne isso nativamente, devendo resultar em 400 (Bad Request) ou 401 (Unauthorized).
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        // Teste: Prevenção contra Cross-Site Scripting (XSS)
        [Fact]
        public async Task Post_WithXssPayload_ShouldReturnBadRequestOrUnauthorized()
        {
            // Arrange: Payload contendo tag de execução de script (XSS Refletido/Armazenado)
            var payload = new { Username = "<script>alert('xss')</script>", Password = "password" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

            // Assert: Entradas com potenciais vetores de XSS devem ser neutralizadas ou negadas.
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        // Teste: Injeção de Comandos (Command Injection)
        [Fact]
        public async Task Post_WithCommandInjection_ShouldBeHandledSafely()
        {
            // Arrange: Tentativa de encadear um comando arbitrário no terminal/SO
            var payload = new { Username = "admin; cat /etc/passwd", Password = "password" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

            // Assert: O comando não deve ser processado, e a requisição deve ser negada com segurança.
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        // Teste: Tratamento de JSON corrompido ou mal formatado
        [Fact]
        public async Task Post_WithMalformedJson_ShouldReturnBadRequest()
        {
            // Arrange: JSON intencionalmente quebrado (faltam aspas e chave de fechamento)
            var malformedJson = "{ \"Username\": \"admin\", \"Password\": \"123\" ";
            var content = new StringContent(malformedJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", content);

            // Assert: O parser (System.Text.Json) do ASP.NET Core deve estourar um erro de conversão tratável, retornando HTTP 400.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Teste: Dados faltantes ou vazios
        [Fact]
        public async Task Post_WithEmptyNullOrMissingFields_ShouldReturnBadRequest()
        {
            // Arrange: Credenciais em branco e nulas (testando falha de verificação de nulos)
            var payload = new { Username = "", Password = (string?)null };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

            // Assert: Regras de validação do Model Binding e validação do Controller devem retornar HTTP 400.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Teste: Limitação de tamanho de Payload / Prevenção a DoS
        [Fact]
        public async Task Post_WithExtremelyLongStrings_ShouldReturnBadRequestOrPayloadTooLarge()
        {
            // Arrange: String massiva testando Buffer Overflow ou tentativa de esgotamento de memória e CPU (Denial of Service)
            var massiveString = new string('A', 100000); 
            var payload = new { Username = massiveString, Password = massiveString };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);

            // Assert: O servidor web (Kestrel/IIS) ou a limitação do ModelState deve intervir. (400, 413 ou 401)
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.RequestEntityTooLarge || 
                response.StatusCode == HttpStatusCode.Unauthorized
            );
        }

        // Teste: Violação de tipagem de dados (Tipos Inválidos)
        [Fact]
        public async Task Post_WithInvalidDataTypes_ShouldReturnBadRequest()
        {
            // Arrange: Fornecendo número (int) e booleano (bool) quando a API espera strings
            var invalidTypeJson = "{ \"Username\": 12345, \"Password\": true }";
            var content = new StringContent(invalidTypeJson, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/auth/login", content);

            // Assert: O mapeamento de modelos deve falhar antes mesmo de atingir a lógica do Controller.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}