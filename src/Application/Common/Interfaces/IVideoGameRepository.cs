using Domain.VideoGames;

namespace Application.Common.Interfaces;

public interface IVideoGameRepository
{
    Task<IReadOnlyList<VideoGame>> GetAll(CancellationToken cancellationToken);
    Task<VideoGame?> GetById(Guid id, CancellationToken cancellationToken);
    Task<VideoGame?> GetByTitle(string title, CancellationToken cancellationToken);
    Task<VideoGame> Add(VideoGame videoGame, CancellationToken cancellationToken);
    Task<VideoGame> Update(VideoGame videoGame, CancellationToken cancellationToken);
    Task Delete(VideoGame videoGame, CancellationToken cancellationToken);
}