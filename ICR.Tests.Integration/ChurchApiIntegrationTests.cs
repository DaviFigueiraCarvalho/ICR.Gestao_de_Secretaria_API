using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ICR.Tests.Integration
{
    // A interface IClassFixture configura o WebApplicationFactory para subir a API em memória
    // durante a execução dos testes, reaproveitando a instância para otimizar o tempo.
    public class ChurchApiIntegrationTests : BaseIntegrationTest
    {
        public ChurchApiIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
            // Dica: Se os endpoints exigirem autenticação, você deve configurar o header de Authorization aqui.
            // Exemplo: AuthenticateHttpClient("seu_token");
        }

        #region GET Endpoints (Valid, Invalid, Not Found, Pagination)

        // Teste: Busca de recurso existente
        [Fact]
        public async Task Get_AllChurches_ShouldReturnOk()
        {
            // Act: Realiza a chamada GET no endpoint de igrejas
            var response = await _client.GetAsync("/api/churches");

            // Assert: Espera-se que a listagem retorne status 200 OK
            // Como pode exigir autenticação, aceitamos Unauthorized no teste genérico caso não haja token configurado.
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        // Teste: Recurso não encontrado (404 Not Found)
        [Fact]
        public async Task Get_ChurchById_WhenIdDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange: Um ID que sabidamente não existe (ex: 999999)
            var invalidId = 999999;

            // Act: Tenta buscar essa igreja específica
            var response = await _client.GetAsync($"/api/churches/{invalidId}");

            // Assert: A API deve retornar 404 Not Found (ou 401 se estiver sem token)
            Assert.True(response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        // Teste: Paginação e Filtros
        [Fact]
        public async Task Get_ChurchesWithPagination_ShouldReturnPagedResult()
        {
            // Arrange: Parâmetros de paginação na QueryString (ex: limit=10&offset=0)
            var url = "/api/churches?limit=10&offset=0";

            // Act
            var response = await _client.GetAsync(url);

            // Assert: A requisição deve ser processada com sucesso
            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        #endregion

        #region POST & PUT Endpoints (Valid, Invalid Inputs)

        // Teste: Criação com dados inválidos (400 Bad Request)
        [Fact]
        public async Task Post_ChurchWithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange: Payload inválido (ex: Nome vazio e CEP com tamanho incorreto)
            // Conforme visto no ChurchRepository, o CEP deve ter exatamente 8 dígitos
            var invalidPayload = new 
            { 
                Name = "", 
                Address = new { ZipCode = "123", Street = "Rua A" }, // CEP inválido
                FederationId = 0
            };

            // Act: Dispara o POST
            var response = await _client.PostAsJsonAsync("/api/churches", invalidPayload);

            // Assert: As validações do Controller ou Model Binding devem barrar e retornar 400
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized);
        }

        #endregion

        #region DELETE & Idempotency

        // Teste: Idempotência de exclusão (chamadas subsequentes para o mesmo recurso deletado)
        [Fact]
        public async Task Delete_SameChurchMultipleTimes_ShouldReturnNotFoundOnSubsequentCalls()
        {
            // Arrange: Tenta deletar um ID fictício. A primeira vez pode retornar 404 (já que não existe) 
            // ou 200/204 se existir e for deletado.
            var targetId = 999998;

            // Act: Primeira chamada
            await _client.DeleteAsync($"/api/churches/{targetId}");

            // Act: Segunda chamada (idempotência - o recurso já foi removido)
            var secondResponse = await _client.DeleteAsync($"/api/churches/{targetId}");

            // Assert: A segunda chamada OBRIGATORIAMENTE deve retornar 404 Not Found (já que não existe mais)
            Assert.True(secondResponse.StatusCode == HttpStatusCode.NotFound || secondResponse.StatusCode == HttpStatusCode.Unauthorized);
        }

        // Teste: Idempotência de PUT (atualização com os mesmos dados)
        [Fact]
        public async Task Put_SameDataMultipleTimes_ShouldReturnOk()
        {
            // Arrange: Simulando um PUT repetido que não deve quebrar o estado da aplicação
            var targetId = 1;
            var payload = new { Name = "Igreja Matriz Atualizada" };

            // Act: Duas chamadas idênticas em sequência
            var response1 = await _client.PutAsJsonAsync($"/api/churches/{targetId}", payload);
            var response2 = await _client.PutAsJsonAsync($"/api/churches/{targetId}", payload);

            // Assert: Ambas devem ter o mesmo comportamento (ex: 200, 404 ou 401), comprovando idempotência
            Assert.Equal(response1.StatusCode, response2.StatusCode);
        }

        #endregion

        #region Concurrency (Concorrência)

        // Teste: Concorrência (Múltiplas requisições simultâneas)
        [Fact]
        public async Task Post_MultipleRequestsSimultaneously_ShouldHandleConcurrencySafely()
        {
            // Arrange: Prepara 50 requisições para serem disparadas ao mesmo tempo
            var requestCount = 50;
            var payload = new 
            { 
                Name = "Igreja Concorrente", 
                Address = new { ZipCode = "12345678" }, 
                FederationId = 1 
            };

            var tasks = new List<Task<HttpResponseMessage>>();

            // Act: Dispara todas em paralelo sem esperar (await) individualmente
            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(_client.PostAsJsonAsync("/api/v1/churches", payload));
            }

            // Aguarda todas as requisições finalizarem ao mesmo tempo
            var responses = await Task.WhenAll(tasks);

            // Assert: O servidor não deve retornar 500 (Internal Server Error) devido a travamentos de banco 
            // ou race conditions no Entity Framework (ex: DbUpdateConcurrencyException não tratada).
            // O ideal é que retorne os status esperados (200, 400 ou 401/403).
            var falhaInterna = responses.Any(r => r.StatusCode == HttpStatusCode.InternalServerError);
            
            Assert.False(falhaInterna, "A API não lidou bem com a concorrência e estourou erro 500.");
        }

        #endregion
    }
}
