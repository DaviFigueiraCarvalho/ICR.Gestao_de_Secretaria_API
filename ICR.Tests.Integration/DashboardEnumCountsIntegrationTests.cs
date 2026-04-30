using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ICR.Domain.DTOs;
using ICR.Domain.Model;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.FederationAggregate;
using ICR.Domain.Model.MemberAggregate;
using ICR.Infra;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ICR.Tests.Integration
{
    public class DashboardEnumCountsIntegrationTests : BaseIntegrationTest
    {
        public DashboardEnumCountsIntegrationTests(CustomWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Get_ClassCounts_National_ShouldReturnCounts()
        {
            await LoginAndAuthenticateAsync("admin", "Password123!");

            var scopeData = await SeedMembersAsync();

            var response = await _client.GetAsync("/api/v1/dashboard/classes/national");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<List<EnumCountResponseDTO>>();

            Assert.NotNull(payload);
            Assert.Contains(payload!, x => x.Id == (int)ClassType.BEBE && x.Count == 1);
            Assert.Contains(payload!, x => x.Id == (int)ClassType.JUVENIS && x.Count == 0);
        }

        [Fact]
        public async Task Get_ClassCounts_Federation_ShouldReturnCounts()
        {
            await LoginAndAuthenticateAsync("admin", "Password123!");

            var scopeData = await SeedMembersAsync();

            var response = await _client.GetAsync($"/api/v1/dashboard/classes/federation/{scopeData.FederationId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<List<EnumCountResponseDTO>>();

            Assert.NotNull(payload);
            Assert.Contains(payload!, x => x.Id == (int)ClassType.BEBE && x.Count == 1);
        }

        [Fact]
        public async Task Get_ClassCounts_Church_ShouldReturnCounts()
        {
            await LoginAndAuthenticateAsync("admin", "Password123!");

            var scopeData = await SeedMembersAsync();

            var response = await _client.GetAsync($"/api/v1/dashboard/classes/church/{scopeData.ChurchId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<List<EnumCountResponseDTO>>();

            Assert.NotNull(payload);
            Assert.Contains(payload!, x => x.Id == (int)ClassType.BEBE && x.Count == 1);
        }

        [Fact]
        public async Task Get_MemberRoleCounts_National_ShouldReturnCounts()
        {
            await LoginAndAuthenticateAsync("admin", "Password123!");

            var scopeData = await SeedMembersAsync();

            var response = await _client.GetAsync("/api/v1/dashboard/member-roles/national");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<List<EnumCountResponseDTO>>();

            Assert.NotNull(payload);
            Assert.Contains(payload!, x => x.Id == (int)MemberRole.Pastor && x.Count == 1);
            Assert.Contains(payload!, x => x.Id == (int)MemberRole.Obreiro && x.Count == 0);
        }

        [Fact]
        public async Task Get_MemberRoleCounts_Federation_ShouldReturnCounts()
        {
            await LoginAndAuthenticateAsync("admin", "Password123!");

            var scopeData = await SeedMembersAsync();

            var response = await _client.GetAsync($"/api/v1/dashboard/member-roles/federation/{scopeData.FederationId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<List<EnumCountResponseDTO>>();

            Assert.NotNull(payload);
            Assert.Contains(payload!, x => x.Id == (int)MemberRole.Pastor && x.Count == 1);
        }

        [Fact]
        public async Task Get_MemberRoleCounts_Church_ShouldReturnCounts()
        {
            await LoginAndAuthenticateAsync("admin", "Password123!");

            var scopeData = await SeedMembersAsync();

            var response = await _client.GetAsync($"/api/v1/dashboard/member-roles/church/{scopeData.ChurchId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<List<EnumCountResponseDTO>>();

            Assert.NotNull(payload);
            Assert.Contains(payload!, x => x.Id == (int)MemberRole.Pastor && x.Count == 1);
        }

        [Fact]
        public async Task Get_ClassCounts_Federation_WithNoMembers_ShouldReturnZeroCounts()
        {
            await LoginAndAuthenticateAsync("admin", "Password123!");

            var emptyScope = await SeedEmptyFederationAsync();

            var response = await _client.GetAsync($"/api/v1/dashboard/classes/federation/{emptyScope.FederationId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var payload = await response.Content.ReadFromJsonAsync<List<EnumCountResponseDTO>>();

            Assert.NotNull(payload);
            Assert.All(payload!, item => Assert.Equal(0, item.Count));
        }

        private async Task<(long FederationId, long ChurchId)> SeedMembersAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ConnectionContext>();

            if (context.Members.Any(m => m.Name == "Dashboard Member"))
            {
                var existing = context.Members.First(m => m.Name == "Dashboard Member");
                var churchId = context.Families.Where(f => f.Id == existing.FamilyId).Select(f => f.ChurchId).FirstOrDefault();
                var federationId = context.Churches.Where(c => c.Id == churchId).Select(c => c.FederationId).FirstOrDefault();
                return (federationId, churchId);
            }

            var federation = new Federation("Dashboard Federation", null);
            context.Federations.Add(federation);
            await context.SaveChangesAsync();

            var church = new Church("Dashboard Church", new Address("12345678", "Rua A", "100", "Cidade", "ST"), federation.Id);
            context.Churches.Add(church);
            await context.SaveChangesAsync();

            var cell = new Cell("Dashboard Cell", Cell.CellType.Celula, church.Id, null);
            context.Cells.Add(cell);
            await context.SaveChangesAsync();

            var family = new Family("Dashboard Family", church.Id, cell.Id, null, null, null);
            context.Families.Add(family);
            await context.SaveChangesAsync();

            var member = new Member(family.Id, "Dashboard Member", GenderType.HOMEM, DateTime.UtcNow.AddYears(-30), false, MemberRole.Pastor, "11999999999", ClassType.BEBE);
            context.Members.Add(member);
            await context.SaveChangesAsync();

            return (federation.Id, church.Id);
        }

        private async Task<(long FederationId, long ChurchId)> SeedEmptyFederationAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ConnectionContext>();

            var federation = new Federation("Empty Federation", null);
            context.Federations.Add(federation);
            await context.SaveChangesAsync();

            var church = new Church("Empty Church", new Address("87654321", "Rua B", "200", "Cidade", "ST"), federation.Id);
            context.Churches.Add(church);
            await context.SaveChangesAsync();

            return (federation.Id, church.Id);
        }
    }
}
