using Application.Common.Interfaces;
using Application.VideoGames.Services.Abstract;
using Domain.VideoGames;

namespace Application.VideoGames.Services.Implementation;

public class VideoGameService(IVideoGameRepository videoGameRepository) : IVideoGameService
{
    public async Task<VideoGame> Add(string title, decimal price, DateTime releaseDate,
        Platform platform, CancellationToken cancellationToken)
    {
        var existingGame = await videoGameRepository.GetByTitle(title, cancellationToken);
        if (existingGame is not null)
        {
            throw new ArgumentException($"Video game with title '{title}' already exists");
        }

        var videoGame = VideoGame.New(Guid.NewGuid(), title, price, releaseDate, platform);
        return await videoGameRepository.Add(videoGame, cancellationToken);
    }

    public async Task<VideoGame?> Update(Guid id, string title, decimal price, DateTime releaseDate,
        Platform platform, CancellationToken cancellationToken)
    {
        var videoGame = await videoGameRepository.GetById(id, cancellationToken);
        if (videoGame is null)
        {
            return null;
        }

        var gameWithSameTitle = await videoGameRepository.GetByTitle(title, cancellationToken);
        if (gameWithSameTitle is not null && gameWithSameTitle.Id != id)
        {
            throw new ArgumentException($"Video game with title '{title}' already exists");
        }

        videoGame.UpdateDetails(title, price, releaseDate, platform);
        return await videoGameRepository.Update(videoGame, cancellationToken);
    }

    public async Task<IReadOnlyList<VideoGame>> GetVideoGames(CancellationToken cancellationToken)
    {
        return await videoGameRepository.GetAll(cancellationToken);
    }

    public async Task<VideoGame?> GetVideoGame(Guid id, CancellationToken cancellationToken)
    {
        return await videoGameRepository.GetById(id, cancellationToken);
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var videoGame = await videoGameRepository.GetById(id, cancellationToken);
        if (videoGame is null)
        {
            return false;
        }

        await videoGameRepository.Delete(videoGame, cancellationToken);
        return true;
    }
}