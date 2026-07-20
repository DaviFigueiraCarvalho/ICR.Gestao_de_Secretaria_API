using ICR.Domain.Model;
using ICR.Domain.Model.CellAggregate;
using ICR.Domain.Model.ChurchAggregate;
using ICR.Domain.Model.FamilyAggregate;
using ICR.Domain.Model.FederationAggregate;
using ICR.Domain.Model.MemberAggregate;
using ICR.Domain.Model.UserRoleAgreggate;
using ICR.Infra;
using ICR.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ICR.Tests.Integration;

public class FilteringRepositoriesTests
{
    [Fact]
    public async Task CellsFilter_AppliesScopeSearchAndPagination()
    {
        var data = await CreateSeededContextAsync();
        await using var context = data.Context;
        var repository = new CellRepository(context);

        var result = (await repository.GetFilteredAsync(
            data.FirstFederationId,
            data.FirstChurchId,
            page: 2,
            pageSize: 1,
            search: "cell")).ToList();

        var cell = Assert.Single(result);
        Assert.Equal("Beta Cell", cell.Name);
        Assert.Equal(Cell.CellType.ComunidadeMissionaria, cell.Type);
    }

    [Fact]
    public async Task ChurchesByFederation_AppliesSearchAndPagination()
    {
        var data = await CreateSeededContextAsync();
        await using var context = data.Context;
        var repository = new ChurchRepository(context);

        var result = (await repository.GetChurchesbyFederationId(
            data.FirstFederationId,
            page: 2,
            pageSize: 1,
            search: "church")).ToList();

        var church = Assert.Single(result);
        Assert.Equal("Beta Church", church.Name);
        Assert.Equal(data.FirstFederationId, church.FederationId);
    }

    [Fact]
    public async Task FamiliesFilter_CombinesChurchAndCellWithSearch()
    {
        var data = await CreateSeededContextAsync();
        await using var context = data.Context;
        var repository = new FamilyRepository(context);

        var result = await repository.GetFilteredAsync(
            data.FirstChurchId,
            data.SecondCellId,
            pageNumber: 1,
            pageQuantity: 10,
            search: "beta");

        var family = Assert.Single(result);
        Assert.Equal("Beta Family", family.Name);
        Assert.Equal(data.FirstChurchId, family.churchId);
        Assert.Equal(data.SecondCellId, family.CellId);
    }

    [Fact]
    public async Task UsersFilter_ReturnsChurchAndCellAndAppliesPagination()
    {
        var data = await CreateSeededContextAsync();
        await using var context = data.Context;
        var repository = new UserRoleRepository(context);

        var result = (await repository.GetFilteredUsersAsync(
            data.FirstChurchId,
            cellId: null,
            page: 2,
            pageSize: 1,
            search: "user")).ToList();

        var user = Assert.Single(result);
        Assert.Equal("bob.user", user.Username);
        Assert.Equal(data.FirstChurchId, user.ChurchId);
        Assert.Equal("Alpha Church", user.ChurchName);
        Assert.Equal(data.SecondCellId, user.CellId);
        Assert.Equal("Beta Cell", user.CellName);
    }

    private static async Task<SeededData> CreateSeededContextAsync()
    {
        var options = new DbContextOptionsBuilder<ConnectionContext>()
            .UseInMemoryDatabase($"FilteringTests_{Guid.NewGuid()}")
            .Options;
        var context = new ConnectionContext(options);

        var firstFederation = new Federation("First Federation", null);
        var secondFederation = new Federation("Second Federation", null);
        context.Federations.AddRange(firstFederation, secondFederation);
        await context.SaveChangesAsync();

        var emptyAddress = new Address(null, null, null, null, null, null);
        var firstChurch = new Church("Alpha Church", emptyAddress, firstFederation.Id);
        var secondChurch = new Church(
            "Beta Church",
            new Address(null, null, null, null, null, null),
            firstFederation.Id);
        var otherChurch = new Church(
            "Other Church",
            new Address(null, null, null, null, null, null),
            secondFederation.Id);
        context.Churches.AddRange(firstChurch, secondChurch, otherChurch);
        await context.SaveChangesAsync();

        var firstCell = new Cell("Alpha Cell", Cell.CellType.Celula, firstChurch.Id, null);
        var secondCell = new Cell("Beta Cell", Cell.CellType.ComunidadeMissionaria, firstChurch.Id, null);
        var otherCell = new Cell("Other Cell", Cell.CellType.Celula, otherChurch.Id, null);
        context.Cells.AddRange(firstCell, secondCell, otherCell);
        await context.SaveChangesAsync();

        var firstFamily = new Family("Alpha Family", firstChurch.Id, firstCell.Id, null, null, null);
        var secondFamily = new Family("Beta Family", firstChurch.Id, secondCell.Id, null, null, null);
        var otherFamily = new Family("Other Family", otherChurch.Id, otherCell.Id, null, null, null);
        context.Families.AddRange(firstFamily, secondFamily, otherFamily);
        await context.SaveChangesAsync();

        var birthDate = new DateOnly(1990, 1, 1);
        var alice = new Member(
            firstFamily.Id,
            "Alice Member",
            GenderType.MULHER,
            birthDate,
            false,
            MemberRole.Outros,
            null,
            ClassType.JOVENS);
        var bob = new Member(
            secondFamily.Id,
            "Bob Member",
            GenderType.HOMEM,
            birthDate,
            false,
            MemberRole.Outros,
            null,
            ClassType.JOVENS);
        var carla = new Member(
            otherFamily.Id,
            "Carla Member",
            GenderType.MULHER,
            birthDate,
            false,
            MemberRole.Outros,
            null,
            ClassType.JOVENS);
        context.Members.AddRange(alice, bob, carla);
        await context.SaveChangesAsync();

        context.Users.AddRange(
            new User(alice.Id, "alice.user", "hash", User.UserScope.CHURCH),
            new User(bob.Id, "bob.user", "hash", User.UserScope.CHURCH),
            new User(carla.Id, "carla.user", "hash", User.UserScope.CHURCH));
        await context.SaveChangesAsync();

        return new SeededData(
            context,
            firstFederation.Id,
            firstChurch.Id,
            secondCell.Id);
    }

    private sealed record SeededData(
        ConnectionContext Context,
        long FirstFederationId,
        long FirstChurchId,
        long SecondCellId);
}
