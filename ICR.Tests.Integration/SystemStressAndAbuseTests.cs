using System;
using System.Collections.Concurrent;
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
    // A interface IClassFixture configura a aplicação em memória para que os testes de integração
    // rodem com a mesma infraestrutura da API real.
    public class SystemStressAndAbuseTests : BaseIntegrationTest
    {
        public SystemStressAndAbuseTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        // Teste: Simulando acesso sob carga leve (Light Load)
        [Fact]
        public async Task Get_EndpointUnderLightLoad_ShouldMaintainStability()
        {
            // Arrange: Definimos uma quantidade moderada de requisições sequenciais/rápidas
            var requestCount = 50;
            var endpoint = "/api/v1/churches";
            var responses = new List<HttpResponseMessage>();

            // Act: Disparamos requisições em um loop rápido (sem paralelismo extremo)
            for (int i = 0; i < requestCount; i++)
            {
                responses.Add(await _client.GetAsync(endpoint));
            }

            // Assert: A API não deve apresentar falhas de infraestrutura (erro 500).
            // Retornos 200 (OK), 401 (Unauthorized) ou 403 (Forbidden) são respostas aceitáveis.
            var hasInternalError = responses.Any(r => r.StatusCode == HttpStatusCode.InternalServerError);
            Assert.False(hasInternalError, "A API engasgou com carga leve e retornou Erro Interno (500).");
        }

        // Teste: Simulando múltiplos usuários concorrentes
        [Fact]
        public async Task Get_EndpointWithConcurrentUsers_ShouldNotCrash()
        {
            // Arrange: Vamos emular 100 acessos simultâneos (usuários diferentes consultando a API ao mesmo tempo)
            var concurrentUsers = 100;
            var endpoint = "/api/v1/churches";
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act: Criamos as Task e armazenamos na lista, fazendo com que todas rodem de forma assíncrona concorrente
            for (int i = 0; i < concurrentUsers; i++)
            {
                tasks.Add(_client.GetAsync(endpoint));
            }

            // Task.WhenAll aguarda todas as requisições disparadas terminarem
            var results = await Task.WhenAll(tasks);

            // Assert: Nenhuma das 100 requisições pode causar quebra do sistema (nenhum 500)
            var internalErrorsCount = results.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
            Assert.Equal(0, internalErrorsCount);
        }

        // Teste: Tentativa de abuso com muitas requisições rápidas (Rate Limiting ou Brute Force)
        [Fact]
        public async Task Post_RapidRepeatedRequests_ShouldTriggerRateLimitingOrRejectSafely()
        {
            // Arrange: Uma bomba de 200 requisições simultâneas para um endpoint crítico (ex: login)
            var requestCount = 200;
            var endpoint = "/api/v1/auth/login";
            var payload = new { Username = "attacker", Password = "bruteforce!" };
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act
            for (int i = 0; i < requestCount; i++)
            {
                tasks.Add(_client.PostAsJsonAsync(endpoint, payload));
            }

            var results = await Task.WhenAll(tasks);

            // Assert: 
            // 1. Não pode haver resposta 500 (banco de dados estourando pool de conexões, por exemplo).
            Assert.DoesNotContain(results, r => r.StatusCode == HttpStatusCode.InternalServerError);

            // 2. Idealmente, valida se o sistema de Limitador de Taxas (Rate Limiting - HTTP 429) atuou.
            // Nota: Se a sua API ainda não possui a configuração de AddRateLimiter(), 
            // este teste apenas garante que os retornos foram tratados adequadamente (ex: 401).
            var hasRateLimiting = results.Any(r => r.StatusCode == HttpStatusCode.TooManyRequests);
            var isHandledSafely = results.All(r => 
                r.StatusCode == HttpStatusCode.Unauthorized || 
                r.StatusCode == HttpStatusCode.BadRequest || 
                r.StatusCode == HttpStatusCode.TooManyRequests);

            Assert.True(isHandledSafely, "As requisições deveriam ter sido bloqueadas por Rate Limit (429) ou negadas por validação segura (401/400).");
        }

        // Teste: Comportamento do sistema sob teste de estresse severo (Burst Mode)
        [Fact]
        public async Task Get_SystemUnderStressBurst_ShouldNotExposeInternalErrors()
        {
            // Arrange: Disparando centenas de requisições ao mesmo tempo divididas em lotes virtuais
            var totalRequests = 500;
            var endpoint = "/api/v1/churches";
            var responses = new ConcurrentBag<HttpResponseMessage>();

            // Act: Usamos Parallel.For em processamento assíncrono para sobrecarregar as threads de resposta do Kestrel
            var tasks = Enumerable.Range(0, totalRequests).Select(async i =>
            {
                try
                {
                    var response = await _client.GetAsync(endpoint);
                    responses.Add(response);
                }
                catch (HttpRequestException)
                {
                    // Em um estresse muito forte, o servidor pode derrubar a conexão ativamente. 
                    // Isso é preferível a retornar pilhas de erros do sistema (Stack Trace).
                    // Para fins do teste, ignoramos este erro de rede esperado.
                }
            });

            await Task.WhenAll(tasks);

            // Assert: A propriedade mais importante em segurança em casos de DoS/DDoS é que 
            // as respostas providenciadas NUNCA contém vazamentos de memória ou stack traces refletidos em erros 500.
            var hasInternalError = responses.Any(r => r.StatusCode == HttpStatusCode.InternalServerError);
            
            Assert.False(hasInternalError, "O sistema não suportou o estresse e quebrou com Exceções Internas (Http 500), possivelmente vazando dados ou estourando limite do banco.");
        }
    }
}