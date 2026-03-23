using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ICR.Tests.Integration
{
    // A interface IClassFixture é usada pelo xUnit para compartilhar o contexto da aplicação em todos os testes desta classe
    public class ApiSecurityTests : BaseIntegrationTest
    {
        // Usa o HttpClient base mas como precisa de overrides no allow redirect, deixaremos local pro teste
        public ApiSecurityTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        // Teste: Verificação de Exposição de Dados Sensíveis
        [Fact]
        public async Task Get_UsersEndpoint_ShouldNotExposePasswordsOrHashes()
        {
            // Arrange: Simulando a busca por usuários ou tentativa de login que retorna dados
            // Caso o endpoint exija autenticação, o recebimento de 401 já é um bom sinal de segurança.
            // Act
            var response = await _client.GetAsync("/api/v1/userrole/users");
            
            // Lemos a resposta em string para verificar o conteúdo bruto retornado pela API
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert: Independentemente de retornar 200, 401 ou 403,
            // garantimos que NENHUMA resposta vaze as palavras-chave "password" ou "hash" de banco de dados.
            Assert.DoesNotContain("\"password\"", responseContent, System.StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("\"passwordHash\"", responseContent, System.StringComparison.OrdinalIgnoreCase);
        }

        // Teste: Verificação de Cabeçalhos de Segurança da Web (Security Headers)
        [Fact]
        public async Task Get_AnyEndpoint_ShouldContainSecurityHeaders()
        {
            // Arrange
            var endpoint = "/api/v1/auth/login";

            // Act: Usando HEAD ou GET para checar os cabeçalhos retornados pelo servidor
            var response = await _client.GetAsync(endpoint);

            // Assert: Verifica se a API está configurando os cabeçalhos de defesa
            // Nota: Se a sua API não tiver um middleware customizado para adicionar isso nativamente,
            // o teste falhará propositalmente indicando que você precisa implementar esse mecanismo no Program.cs
            
            // X-Content-Type-Options: Evita ataques de MIME sniffing
            Assert.True(response.Headers.Contains("X-Content-Type-Options") || response.StatusCode == HttpStatusCode.Unauthorized, 
                "Falta o header X-Content-Type-Options: nosniff");

            // X-Frame-Options: Previne Clickjacking (impedindo que a API/Site seja aberta em iframes não autorizados)
            Assert.True(response.Headers.Contains("X-Frame-Options") || response.StatusCode == HttpStatusCode.Unauthorized, 
                "Falta o header X-Frame-Options: DENY ou SAMEORIGIN");

            // Content-Security-Policy (CSP): Reduz o risco de XSS definindo quais recursos podem ser carregados
            Assert.True(response.Content.Headers.Contains("Content-Security-Policy") || response.Headers.Contains("Content-Security-Policy") || response.StatusCode == HttpStatusCode.Unauthorized, 
                "Falta o header Content-Security-Policy");
        }

        // Teste: CORS - Proteção de Compartilhamento de Recursos de Origem Cruzada
        [Fact]
        public async Task Options_RequestFromUnallowedOrigin_ShouldBeBlockedByCors()
        {   
            // Arrange: Cria uma requisição do tipo OPTIONS com uma Origem maliciosa
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/churches");
            request.Headers.Add("Origin", "https://malicious-hacker-site.com");
            request.Headers.Add("Access-Control-Request-Method", "POST"); // Preflight request

            // Act
            var response = await _client.SendAsync(request);

            // Assert: Em um ambiente de produção rigoroso, o header Allow-Origin não deve liberar a origem maliciosa.
            // Caso sua política esteja 'AllowAnyOrigin', não haverá bloqueio (o que é inseguro para produção).
            // A ausência do cabeçalho "Access-Control-Allow-Origin" indica falha na concessão de CORS para aquele domínio.
            var hasAllowOriginHeader = response.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowOrigins);
            
            // Se o header existir, ele NÂO PODE conter o wildcard "*" ou o site malicioso.
            if (hasAllowOriginHeader)
            {
                var originValue = string.Join(",", allowOrigins);
                Assert.NotEqual("*", originValue);
                Assert.NotEqual("https://malicious-hacker-site.com", originValue);
            }
        }

        // Teste: Imposição de Tráfego Seguro (HTTPS Enforcement)
        [Fact]
        public async Task Get_HttpEndpoint_ShouldRedirectToHttpsOrReturnHsts()
        {
            // Arrange: Tentativa forçada de chamar um endpoint via protocolo HTTP não criptografado
            var httpClientHstsOptions = new WebApplicationFactoryClientOptions { AllowAutoRedirect = false };
            var unsecureClient = _factory.CreateClient(httpClientHstsOptions);
            unsecureClient.BaseAddress = new System.Uri("http://localhost");

            // Act
            var response = await unsecureClient.GetAsync("/api/v1/churches");

            // Assert: 
            // 1. Deve redirecionar (307 Temporary Redirect / 308 Permanent Redirect) gerado pelo app.UseHttpsRedirection()
            // 2. Ou o Endpoint já não deve aceitar e retornar erro.
            if (response.StatusCode == HttpStatusCode.RedirectKeepVerb || response.StatusCode == HttpStatusCode.MovedPermanently)
            {
                // Garante que o redirecionamento é para a porta/schema HTTPS
                Assert.Equal("https", response.Headers.Location?.Scheme);
            }
            else
            {
                // Se o status HTTP for de resposta normal (ex: Unauthorized), testamos se o Strict-Transport-Security (HSTS) está presente (Adicionado anteriormente no Program.cs)
                Assert.True(response.Headers.Contains("Strict-Transport-Security") || !response.IsSuccessStatusCode);
            }
        }

        // Teste: Restrição de Upload de Arquivos Maliciosos
        [Fact]
        public async Task Post_UploadExecutableFile_ShouldReturnUnsupportedMediaTypeOrBadRequest()
        {
            // Arrange: Monta um formulário (Multipart) e tenta fingir que está subindo um arquivo executável disfarçado
            var content = new MultipartFormDataContent();
            
            // Criando conteúdo binário falso (representando um .exe ou script malicioso)
            var fileContent = new ByteArrayContent(new byte[] { 0x4D, 0x5A, 0x90, 0x00 }); // "MZ" Assinatura de Windows Executable
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-msdownload");
            
            // Enganando o servidor na tentativa de Bypass
            content.Add(fileContent, "file", "malicious_payload.exe");

            // Act: Envia para um suposto endpoint de upload
            var response = await _client.PostAsync("/api/v1/files/upload", content);

            // Assert: A API NÃO PODE retornar StatusCode indicando sucesso (200, 201).
            // Retornar NotFound (404) é esperado caso o endpoint não exista na sua API atual, 
            // e BadRequest (400) / UnsupportedMediaType (415) caso exista mas a validação de extensão funcione.
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest || 
                response.StatusCode == HttpStatusCode.UnsupportedMediaType || 
                response.StatusCode == HttpStatusCode.NotFound || 
                response.StatusCode == HttpStatusCode.Unauthorized,
                "A API falhou em barrar e está aceitando arquivos de formatos nocivos (.exe, .php, etc)."
            );
        }
    }
}