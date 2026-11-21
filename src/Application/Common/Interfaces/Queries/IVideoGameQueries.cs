using Domain.VideoGames;
using LanguageExt;

namespace Application.Common.Interfaces.Queries;

public interface IVideoGameQueries
{
    Task<IReadOnlyList<VideoGame>> GetAll(CancellationToken cancellationToken);
    Task<Option<VideoGame>> GetById(VideoGameId id, CancellationToken cancellationToken);
}
