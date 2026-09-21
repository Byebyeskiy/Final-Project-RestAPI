using System.ComponentModel.DataAnnotations;
using Domain.VideoGames;

namespace Api.Dtos;

public record VideoGameDto(Guid Id, string Title, decimal Price, DateTime ReleaseDate,
    Platform Platform, DateTime CreatedAt)
{
    public static VideoGameDto FromDomainModel(VideoGame videoGame)
        => new(videoGame.Id, videoGame.Title, videoGame.Price, videoGame.ReleaseDate,
            videoGame.Platform, videoGame.CreatedAt);
}

public record CreateVideoGameDto(
    [Required, MaxLength(255)] string Title,
    [Range(0.01, 1_000_000)] decimal Price,
    DateTime ReleaseDate,
    Platform Platform);

public record UpdateVideoGameDto(
    [Required, MaxLength(255)] string Title,
    [Range(0.01, 1_000_000)] decimal Price,
    DateTime ReleaseDate,
    Platform Platform);