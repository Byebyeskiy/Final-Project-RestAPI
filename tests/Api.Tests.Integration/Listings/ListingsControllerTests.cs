using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.Listings;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.Listings;
using Xunit;

namespace Api.Tests.Integration.Listings;

public sealed class ListingsControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IClassFixture<IntegrationTestWebFactory>, IAsyncLifetime
{
    private const string Route = "listings";
    private readonly Listing _existing = ListingTestData.Existing();

    private string ExistingRoute => $"{Route}/{_existing.Id}";

    public async Task InitializeAsync()
    {
        Client.DefaultRequestHeaders.Add("X-Admin-Key", IntegrationTestWebFactory.AdminKey);
        Context.Listings.Add(_existing);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        await Context.ListingReservations.ExecuteDeleteAsync();
        await Context.Listings.ExecuteDeleteAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task ShouldCreateListing()
    {
        var game = ListingTestData.NewGame();
        var request = new CreateListingDto(game.Title, game.Price, game.AvailableSince, game.Category);

        using var response = await Client.PostAsJsonAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.Content.ReadFromJsonAsync<ListingDto>();
        dto.Should().NotBeNull();
        var saved = await Find(new ListingId(dto!.Id));
        AssertDetails(saved, game);
        saved.Status.Should().Be(ListingStatus.Pending);
        dto.Status.Should().Be(nameof(ListingStatus.Pending));
        using var readResponse = await Client.GetAsync(response.Headers.Location);
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readDto = await readResponse.Content.ReadFromJsonAsync<ListingDto>();
        readDto.Should().BeEquivalentTo(dto, options => options.Excluding(game => game.CreatedAt));
        readDto!.CreatedAt.Should().BeCloseTo(dto.CreatedAt, TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    public async Task ShouldNotCreateListingBecauseTitleAlreadyTaken()
    {
        var request = new CreateListingDto(_existing.Title, _existing.Price,
            _existing.AvailableSince, _existing.Category);

        using var response = await Client.PostAsJsonAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var saved = await Context.Listings.AsNoTracking().SingleAsync();
        saved.Id.Should().Be(_existing.Id);
        AssertDetails(saved, _existing);
    }

    [Fact]
    public async Task ShouldNotCreateListingBecauseRequestIsInvalid()
    {
        var request = new CreateListingDto("", 0, default, (ListingCategory)999);

        using var response = await Client.PostAsJsonAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var saved = await Context.Listings.AsNoTracking().SingleAsync();
        saved.Id.Should().Be(_existing.Id);
        AssertDetails(saved, _existing);
    }

    [Fact]
    public async Task ShouldUpdateListing()
    {
        var game = ListingTestData.Updated();
        var request = new UpdateListingDto(game.Title, game.Price, game.AvailableSince, game.Category);

        using var response = await Client.PutAsJsonAsync(ExistingRoute, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ListingDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(_existing.Id.Value);
        AssertDetails(await Find(_existing.Id), game);
    }

    [Fact]
    public async Task ShouldNotUpdateListingBecauseTitleAlreadyTaken()
    {
        var other = ListingTestData.NewGame();
        Context.Listings.Add(other);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        var request = new UpdateListingDto(other.Title, other.Price, other.AvailableSince, other.Category);

        using var response = await Client.PutAsJsonAsync(ExistingRoute, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        AssertDetails(await Find(_existing.Id), _existing);
        AssertDetails(await Find(other.Id), other);
    }

    [Fact]
    public async Task ShouldNotUpdateListingBecauseListingNotFound()
    {
        var game = ListingTestData.Updated();
        var request = new UpdateListingDto(game.Title, game.Price, game.AvailableSince, game.Category);

        using var response = await Client.PutAsJsonAsync($"{Route}/{ListingId.New()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        AssertDetails(await Find(_existing.Id), _existing);
    }

    [Fact]
    public async Task ShouldDeleteListing()
    {
        using var response = await Client.DeleteAsync(ExistingRoute);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Context.Listings.AnyAsync(game => game.Id == _existing.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task ShouldNotDeleteListingBecauseListingNotFound()
    {
        using var response = await Client.DeleteAsync($"{Route}/{ListingId.New()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        AssertDetails(await Find(_existing.Id), _existing);
    }

    [Fact]
    public async Task ShouldApprovePendingListing()
    {
        using var response = await Client.PostAsync($"{ExistingRoute}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ListingDto>();
        dto!.Status.Should().Be(nameof(ListingStatus.Approved));
        (await Find(_existing.Id)).Status.Should().Be(ListingStatus.Approved);
    }

    [Fact]
    public async Task ShouldRejectPendingListingWithReason()
    {
        const string reason = "Add a clearer description";

        using var response = await Client.PostAsJsonAsync($"{ExistingRoute}/reject", new RejectListingDto(reason));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<ListingDto>();
        dto!.Status.Should().Be(nameof(ListingStatus.Rejected));
        dto.RejectionReason.Should().Be(reason);
        var saved = await Find(_existing.Id);
        saved.Status.Should().Be(ListingStatus.Rejected);
        saved.RejectionReason.Should().Be(reason);
    }

    [Fact]
    public async Task ShouldRequireRejectionReason()
    {
        using var response = await Client.PostAsJsonAsync($"{ExistingRoute}/reject", new RejectListingDto(" "));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Find(_existing.Id)).Status.Should().Be(ListingStatus.Pending);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenModeratingMissingListing()
    {
        using var response = await Client.PostAsync($"{Route}/{ListingId.New()}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Find(_existing.Id)).Status.Should().Be(ListingStatus.Pending);
    }

    [Fact]
    public async Task ShouldAcceptOnlyOneConcurrentModerationDecision()
    {
        var approve = Client.PostAsync($"{ExistingRoute}/approve", null);
        var reject = Client.PostAsJsonAsync($"{ExistingRoute}/reject", new RejectListingDto("Missing details"));

        await Task.WhenAll(approve, reject);
        using var approved = await approve;
        using var rejected = await reject;

        new[] { approved.StatusCode, rejected.StatusCode }.Should()
            .BeEquivalentTo(new[] { HttpStatusCode.OK, HttpStatusCode.Conflict });
        var saved = await Find(_existing.Id);
        var winningStatus = approved.IsSuccessStatusCode ? ListingStatus.Approved : ListingStatus.Rejected;
        saved.Status.Should().Be(winningStatus);
    }

    [Theory]
    [InlineData("approve", false)]
    [InlineData("approve", true)]
    [InlineData("reject", false)]
    [InlineData("reject", true)]
    public async Task ShouldRequireValidAdminKeyForModeration(string action, bool sendInvalidKey)
    {
        Client.DefaultRequestHeaders.Remove("X-Admin-Key");
        if (sendInvalidKey)
        {
            Client.DefaultRequestHeaders.Add("X-Admin-Key", "invalid");
        }

        using var response = await Client.PostAsJsonAsync(
            $"{ExistingRoute}/{action}", new RejectListingDto("Missing information"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await Find(_existing.Id)).Status.Should().Be(ListingStatus.Pending);

        using var catalogResponse = await Client.GetAsync(Route);
        catalogResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldReserveApprovedListingAndDecreaseStock()
    {
        var game = ListingTestData.NewGame();
        using var created = await Client.PostAsJsonAsync(Route, new CreateListingDto(
            game.Title, game.Price, game.AvailableSince, game.Category, Quantity: 3));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var listing = await created.Content.ReadFromJsonAsync<ListingDto>();
        listing!.AvailableQuantity.Should().Be(3);
        var path = $"{Route}/{listing.Id}";
        using var approved = await Client.PostAsync($"{path}/approve", null);
        approved.StatusCode.Should().Be(HttpStatusCode.OK);

        using var response = await Client.PostAsJsonAsync($"{path}/reservations", new ReserveListingDto(2));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var receipt = await response.Content.ReadFromJsonAsync<ListingReservationDto>();
        var stored = await Context.ListingReservations.AsNoTracking().SingleAsync();
        receipt!.Id.Should().Be(stored.Id);
        stored.ListingId.Value.Should().Be(listing.Id);
        stored.Quantity.Should().Be(2);
        (await Find(stored.ListingId)).AvailableQuantity.Should().Be(1);
        using var read = await Client.GetAsync(path);
        var updated = await read.Content.ReadFromJsonAsync<ListingDto>();
        updated!.AvailableQuantity.Should().Be(1);
    }

    [Fact]
    public async Task ShouldRejectReservationForPendingListing()
    {
        using var response = await Client.PostAsJsonAsync(
            $"{ExistingRoute}/reservations", new ReserveListingDto(1));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await Find(_existing.Id)).AvailableQuantity.Should().Be(1);
        (await Context.ListingReservations.CountAsync()).Should().Be(0);
    }

    [Theory]
    [InlineData(0, HttpStatusCode.BadRequest)]
    [InlineData(2, HttpStatusCode.Conflict)]
    public async Task ShouldRejectInvalidOrUnavailableQuantity(int quantity, HttpStatusCode expectedStatus)
    {
        using var approved = await Client.PostAsync($"{ExistingRoute}/approve", null);
        approved.StatusCode.Should().Be(HttpStatusCode.OK);

        using var response = await Client.PostAsJsonAsync(
            $"{ExistingRoute}/reservations", new ReserveListingDto(quantity));

        response.StatusCode.Should().Be(expectedStatus);
        (await Find(_existing.Id)).AvailableQuantity.Should().Be(1);
        (await Context.ListingReservations.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWhenReservingUnknownListing()
    {
        using var response = await Client.PostAsJsonAsync(
            $"{Route}/{ListingId.New()}/reservations", new ReserveListingDto(1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Context.ListingReservations.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ShouldAllowOnlyOneConcurrentReservationForLastUnit()
    {
        using var approved = await Client.PostAsync($"{ExistingRoute}/approve", null);
        approved.StatusCode.Should().Be(HttpStatusCode.OK);

        var responses = await Task.WhenAll(
            Client.PostAsJsonAsync($"{ExistingRoute}/reservations", new ReserveListingDto(1)),
            Client.PostAsJsonAsync($"{ExistingRoute}/reservations", new ReserveListingDto(1)));
        using var first = responses[0];
        using var second = responses[1];

        new[] { first.StatusCode, second.StatusCode }.Should()
            .BeEquivalentTo(new[] { HttpStatusCode.Created, HttpStatusCode.Conflict });
        (await Find(_existing.Id)).AvailableQuantity.Should().Be(0);
        var reservation = await Context.ListingReservations.AsNoTracking().SingleAsync();
        reservation.ListingId.Should().Be(_existing.Id);
        reservation.Quantity.Should().Be(1);

        using var delete = await Client.DeleteAsync(ExistingRoute);
        delete.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await Context.ListingReservations.CountAsync()).Should().Be(1);
    }

    private Task<Listing> Find(ListingId id) =>
        Context.Listings.AsNoTracking().SingleAsync(game => game.Id == id);

    private static void AssertDetails(Listing actual, Listing expected)
    {
        actual.Title.Should().Be(expected.Title);
        actual.Price.Should().Be(expected.Price);
        actual.AvailableSince.Should().Be(expected.AvailableSince);
        actual.Category.Should().Be(expected.Category);
    }
}
