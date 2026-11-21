using Domain.VideoGames;
using LanguageExt;

namespace Application.Common.Interfaces.Repositories;

public interface IVideoGameRepository
{
    Task<Option<VideoGame>> GetById(VideoGameId id, CancellationToken cancellationToken);
    Task<Option<VideoGame>> GetByTitle(string title, CancellationToken cancellationToken);
    Task<VideoGame> Add(VideoGame videoGame, CancellationToken cancellationToken);
    Task<VideoGame> Update(VideoGame videoGame, CancellationToken cancellationToken);
    Task Delete(VideoGame videoGame, CancellationToken cancellationToken);
}
