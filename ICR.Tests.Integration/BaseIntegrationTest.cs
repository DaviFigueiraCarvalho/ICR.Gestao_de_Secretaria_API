using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace ICR.Tests.Integration
{
    // Classe base que unifica a injeção da CustomWebApplicationFactory e instanciacão do HttpClient.
    // Todos os testes que interagem com a API herdarão dela.
    public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly CustomWebApplicationFactory _factory;
        protected readonly HttpClient _client;

        protected BaseIntegrationTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            // Cria um HttpClient apontando para a API hospedada em memória
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Utilitário para adicionar um Bearer Token manual nas requisições do HttpClient
        /// </summary>
        protected void AuthenticateHttpClient(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Remove qualquer credencial do HttpClient (para testar exclusão/401)
        /// </summary>
        protected void ClearAuthentication()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        // --- Helper Fake / Mocked para gerar um Token de acesso ---
        // (Opção B: Testes autenticados usando um serviço ou gerando o JWT em teste)
        // Se a API tiver uma forma rápida de contornar ou uma rota de dev para gerar tokens:
        
        /// <summary>
        /// Executa o login na API e já injeta o Token no HttpClient.
        /// </summary>
        protected async Task LoginAndAuthenticateAsync(string username, string password)
        {
            var payload = new { Username = username, Password = password };
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", payload);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<SimulatedAuthResponse>();
                if (responseContent != null && !string.IsNullOrWhiteSpace(responseContent.Token))
                {
                    var pureToken = responseContent.Token.Replace("Bearer ", "");
                    AuthenticateHttpClient(pureToken);
                }
            }
        }

        // Molde privado apenas para deserializar
        private class SimulatedAuthResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
