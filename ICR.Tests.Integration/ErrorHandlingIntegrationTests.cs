using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ICR.Tests.Integration
{
    // A interface IClassFixture injeta a API em memória e permite o compartilhamento
    // dessa instância em todos os métodos de teste.
    public class ErrorHandlingIntegrationTests : BaseIntegrationTest
    {
        public ErrorHandlingIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        // Teste: Certificando que rotas inexistentes geram um código de erro tratável (Global 404)
        [Fact]
        public async Task Get_InvalidRoute_ShouldReturnNotFoundWithoutStackTrace()
        {
            // Arrange: Um endpoint que claramente não existe no mapeamento de rotas da aplicação
            var invalidRoute = "/api/v1/invalid-hacker-route-999";

            // Act: Dispara a requisição GET
            var response = await _client.GetAsync(invalidRoute);

            // Assert: O roteador padrão deve retornar código 404 (Not Found)
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Lemos o corpo da resposta
            var content = await response.Content.ReadAsStringAsync();

            // Garantimos que estruturas de stack trace (comuns em vazamento de informações) NÃO estão na tela
            Assert.DoesNotContain("at Microsoft.AspNetCore", content);
            Assert.DoesNotContain("Exception:", content);
            Assert.DoesNotContain("StackTrace", content);
        }

        // Teste: Validando que Erros de Métodos Não Suportados são bem lidados (Ex: DELETE num endpoint que só tem GET)
        [Fact]
        public async Task Delete_ResourceWithoutDeleteMethod_ShouldReturnMethodNotAllowed()
        {
            // Arrange: Endpoints genéricos (como o de login) que geralmente não possuemHttpDelete 
            var endpoint = "/api/v1/auth/login";

            // Act: Dispara requisição usando um verbo indevido (DELETE)
            var response = await _client.DeleteAsync(endpoint);

            // Assert: A API (via framework) deve barrar com HTTP 405 (Method Not Allowed) ou 404
            // e nunca tentar executar alguma ação ou quebrar com erro interno.
            Assert.True(
                response.StatusCode == HttpStatusCode.MethodNotAllowed || 
                response.StatusCode == HttpStatusCode.NotFound
            );

            // Validação vital de segurança (evitar vazamento em erros de roteamento)
            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("Exception:", content);
        }

        // Teste: Falha inesperada. Testando proteção contra Vazamento de Stack Trace
        // Mesmo quando o servidor falha com HTTP 500, a resposta não pode expor mensagens do código-fonte.
        [Fact]
        public async Task Trigger_UnexpectedFailure_ShouldReturnGenericErrorWithoutStackTrace()
        {
            // NOTA: Em testes E2E/Integração limpos, forçar um erro 500 real na API pode ser difícil caso seu código
            // seja inteiramente tratado. Um jeito comum de emular é enviar payloads que quebrem serializadores profundos,
            // ou acessar endpoints específicos criados para testes de "throw new Exception()".
            // Para efeitos práticos e segurança geral, enviamos um header malformado ou payload gigantesco para provocar falhas no middleware
            
            // Arrange: Simulando um Header incompatível ou um problema que o model binder nativo tem dificuldade.
            _client.DefaultRequestHeaders.Add("Accept", "invalid/format-malformed");
            
            var payload = new System.Net.Http.StringContent("not-a-json", System.Text.Encoding.UTF8, "application/json");

            // Act: A tentativa de fazer POST com dados cruamente defeituosos pode cair em exceções profundas
            var response = await _client.PostAsync("/api/v1/auth/login", payload);

            var content = await response.Content.ReadAsStringAsync();

            // Assert: Indiferente do erro ser 400 (Bad Request), 415 (Unsupported Media Type) ou 500 (Internal Server Error)
            // A rigorosa verificação é: O texto do erro NÃO contém rastros do C#
            
            Assert.DoesNotContain("System.", content);
            Assert.DoesNotContain("Exception", content);
            Assert.DoesNotContain("at ", content); // Sintaxe comum do Stack Trace "at Namespace.Class.Method()"
            Assert.DoesNotContain("DevExceptionPage", content); // A página amarela de erros não deve vazar para a API configurada direito
        }

        // Teste: Simulação de Verificação de Payload com Entidades Incompletas/Falhas Lógicas (Regras de Domínio)
        [Fact]
        public async Task Post_MissingRequiredFieldConstraint_ShouldReturnHandledProblemDetails()
        {
            // Arrange: Um DTO de Igreja que deveria ter FederationId porém passamos nulo intencionalmente.
            // Para garantir que o Entity Framework trate o problema na inserção (Foreign Key violada ou NULL inválido)
            // gerando uma mensagem controlada, e não o erro bruto de banco ("NpgsqlException / SqlException").
            var invalidChurchPayload = new 
            {
                Name = "Igreja Quebrada",
                FederationId = (int?)null
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/churches", invalidChurchPayload);
            var content = await response.Content.ReadAsStringAsync();

            // Assert: A operação deve ser negada logo na entrada (400) ou no uso do serviço.
            // Sob nenhuma circunstância vazamentos do banco de dados (ex: 'Violation of PRIMARY KEY constraint' ou 'Npgsql NpgsqlException') 
            // podem chegar ao usuário final.
            Assert.DoesNotContain("SqlException", content, System.StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("NpgsqlException", content, System.StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Entity Framework", content, System.StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Database", content, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}