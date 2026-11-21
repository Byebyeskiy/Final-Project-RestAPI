using Domain.VideoGames;

namespace Application.Common.Interfaces.Queries;

public interface IVideoGameQueries
{
    Task<IReadOnlyList<VideoGame>> GetAll(CancellationToken cancellationToken);
    Task<VideoGame?> GetById(Guid id, CancellationToken cancellationToken);
}
