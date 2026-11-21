using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Domain.VideoGames;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tests.Common;
using Xunit;

namespace Api.Tests.Integration.VideoGames;

public sealed class VideoGamesControllerTests(IntegrationTestWebFactory factory)
    : BaseIntegrationTest(factory), IClassFixture<IntegrationTestWebFactory>
{
    [Fact]
    public async Task ShouldCreateVideoGame()
    {
        var request = new CreateVideoGameDto("Test video game", 49.99m,
            new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc), Platform.Pc);

        using var response = await Client.PostAsJsonAsync("videogames", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var dto = await response.Content.ReadFromJsonAsync<VideoGameDto>();
        dto.Should().NotBeNull();
        var id = new VideoGameId(dto!.Id);
        var saved = await Context.VideoGames.SingleAsync(game => game.Id == id);
        saved.Title.Should().Be(request.Title);
        saved.Price.Should().Be(request.Price);
        saved.ReleaseDate.Should().Be(request.ReleaseDate);
        saved.Platform.Should().Be(request.Platform);
    }
}
