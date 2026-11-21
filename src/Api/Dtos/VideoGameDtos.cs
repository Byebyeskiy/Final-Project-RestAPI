using Domain.VideoGames;

namespace Api.Dtos;

public record VideoGameDto(Guid Id, string Title, decimal Price, DateTime ReleaseDate,
    Platform Platform, DateTime CreatedAt)
{
    public static VideoGameDto FromDomainModel(VideoGame videoGame)
        => new(videoGame.Id.Value, videoGame.Title, videoGame.Price, videoGame.ReleaseDate,
            videoGame.Platform, videoGame.CreatedAt);
}

public record CreateVideoGameDto(
    string Title,
    decimal Price,
    DateTime ReleaseDate,
    Platform Platform);

public record UpdateVideoGameDto(
    string Title,
    decimal Price,
    DateTime ReleaseDate,
    Platform Platform);
