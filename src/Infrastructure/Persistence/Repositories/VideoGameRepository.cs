using Application.Common.Interfaces;
using Domain.VideoGames;

namespace Infrastructure.Persistence.Repositories;

public class VideoGameRepository : IVideoGameRepository
{
    private readonly List<VideoGame> _videoGames = [];

    public Task<IReadOnlyList<VideoGame>> GetAll(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<VideoGame>>(_videoGames.ToList());
    }

    public Task<VideoGame?> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_videoGames.FirstOrDefault(x => x.Id == id));
    }

    public Task<VideoGame?> GetByTitle(string title, CancellationToken cancellationToken)
    {
        return Task.FromResult(_videoGames.FirstOrDefault(x => x.Title == title));
    }

    public Task<VideoGame> Add(VideoGame videoGame, CancellationToken cancellationToken)
    {
        _videoGames.Add(videoGame);
        return Task.FromResult(videoGame);
    }

    public Task<VideoGame> Update(VideoGame videoGame, CancellationToken cancellationToken)
    {
        var existing = _videoGames.FirstOrDefault(x => x.Id == videoGame.Id);
        if (existing is not null)
        {
            _videoGames.Remove(existing);
        }

        _videoGames.Add(videoGame);
        return Task.FromResult(videoGame);
    }

    public Task Delete(VideoGame videoGame, CancellationToken cancellationToken)
    {
        _videoGames.RemoveAll(x => x.Id == videoGame.Id);
        return Task.CompletedTask;
    }
}