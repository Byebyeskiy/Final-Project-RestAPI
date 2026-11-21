using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.VideoGames;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Tests.Data.VideoGames;
using Xunit;

namespace Api.Tests.Integration.VideoGames;

public sealed class VideoGamesControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IClassFixture<IntegrationTestWebFactory>, IAsyncLifetime
{
    private const string Route = "videogames";
    private readonly VideoGame _existing = VideoGameTestData.Existing();

    private string ExistingRoute => $"{Route}/{_existing.Id}";

    public async Task InitializeAsync()
    {
        Context.VideoGames.Add(_existing);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    public async Task DisposeAsync()
    {
        await Context.VideoGames.ExecuteDeleteAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task ShouldCreateVideoGame()
    {
        var game = VideoGameTestData.NewGame();
        var request = new CreateVideoGameDto(game.Title, game.Price, game.ReleaseDate, game.Platform);

        using var response = await Client.PostAsJsonAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.Content.ReadFromJsonAsync<VideoGameDto>();
        dto.Should().NotBeNull();
        var saved = await Find(new VideoGameId(dto!.Id));
        AssertDetails(saved, game);
        using var readResponse = await Client.GetAsync(response.Headers.Location);
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readDto = await readResponse.Content.ReadFromJsonAsync<VideoGameDto>();
        readDto.Should().BeEquivalentTo(dto, options => options.Excluding(game => game.CreatedAt));
        readDto!.CreatedAt.Should().BeCloseTo(dto.CreatedAt, TimeSpan.FromMicroseconds(1));
    }

    [Fact]
    public async Task ShouldNotCreateVideoGameBecauseTitleAlreadyTaken()
    {
        var request = new CreateVideoGameDto(_existing.Title, _existing.Price,
            _existing.ReleaseDate, _existing.Platform);

        using var response = await Client.PostAsJsonAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var saved = await Context.VideoGames.AsNoTracking().SingleAsync();
        saved.Id.Should().Be(_existing.Id);
        AssertDetails(saved, _existing);
    }

    [Fact]
    public async Task ShouldNotCreateVideoGameBecauseRequestIsInvalid()
    {
        var request = new CreateVideoGameDto("", 0, default, (Platform)999);

        using var response = await Client.PostAsJsonAsync(Route, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var saved = await Context.VideoGames.AsNoTracking().SingleAsync();
        saved.Id.Should().Be(_existing.Id);
        AssertDetails(saved, _existing);
    }

    [Fact]
    public async Task ShouldUpdateVideoGame()
    {
        var game = VideoGameTestData.Updated();
        var request = new UpdateVideoGameDto(game.Title, game.Price, game.ReleaseDate, game.Platform);

        using var response = await Client.PutAsJsonAsync(ExistingRoute, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<VideoGameDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(_existing.Id.Value);
        AssertDetails(await Find(_existing.Id), game);
    }

    [Fact]
    public async Task ShouldNotUpdateVideoGameBecauseTitleAlreadyTaken()
    {
        var other = VideoGameTestData.NewGame();
        Context.VideoGames.Add(other);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        var request = new UpdateVideoGameDto(other.Title, other.Price, other.ReleaseDate, other.Platform);

        using var response = await Client.PutAsJsonAsync(ExistingRoute, request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        AssertDetails(await Find(_existing.Id), _existing);
        AssertDetails(await Find(other.Id), other);
    }

    [Fact]
    public async Task ShouldNotUpdateVideoGameBecauseVideoGameNotFound()
    {
        var game = VideoGameTestData.Updated();
        var request = new UpdateVideoGameDto(game.Title, game.Price, game.ReleaseDate, game.Platform);

        using var response = await Client.PutAsJsonAsync($"{Route}/{VideoGameId.New()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        AssertDetails(await Find(_existing.Id), _existing);
    }

    [Fact]
    public async Task ShouldDeleteVideoGame()
    {
        using var response = await Client.DeleteAsync(ExistingRoute);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Context.VideoGames.AnyAsync(game => game.Id == _existing.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task ShouldNotDeleteVideoGameBecauseVideoGameNotFound()
    {
        using var response = await Client.DeleteAsync($"{Route}/{VideoGameId.New()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        AssertDetails(await Find(_existing.Id), _existing);
    }

    private Task<VideoGame> Find(VideoGameId id) =>
        Context.VideoGames.AsNoTracking().SingleAsync(game => game.Id == id);

    private static void AssertDetails(VideoGame actual, VideoGame expected)
    {
        actual.Title.Should().Be(expected.Title);
        actual.Price.Should().Be(expected.Price);
        actual.ReleaseDate.Should().Be(expected.ReleaseDate);
        actual.Platform.Should().Be(expected.Platform);
    }
}
