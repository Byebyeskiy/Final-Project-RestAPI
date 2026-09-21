using Domain.VideoGames;

namespace Application.VideoGames.Services.Abstract;

public interface IVideoGameService
{
    Task<IReadOnlyList<VideoGame>> GetVideoGames(CancellationToken cancellationToken);
    Task<VideoGame?> GetVideoGame(Guid id, CancellationToken cancellationToken);
    Task<VideoGame> Add(string title, decimal price, DateTime releaseDate,
        Platform platform, CancellationToken cancellationToken);
    Task<VideoGame?> Update(Guid id, string title, decimal price, DateTime releaseDate,
        Platform platform, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}