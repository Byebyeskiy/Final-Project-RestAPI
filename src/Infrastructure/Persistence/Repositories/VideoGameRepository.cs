using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.VideoGames;
using LanguageExt;
using static LanguageExt.Prelude;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class VideoGameRepository(ApplicationDbContext context) : IVideoGameRepository, IVideoGameQueries
{
    public async Task<IReadOnlyList<VideoGame>> GetAll(CancellationToken cancellationToken)
    {
        return await context.VideoGames.ToListAsync(cancellationToken);
    }

    public async Task<Option<VideoGame>> GetById(VideoGameId id, CancellationToken cancellationToken)
    {
        return Optional(await context.VideoGames.FirstOrDefaultAsync(x => x.Id == id, cancellationToken));
    }

    public async Task<Option<VideoGame>> GetByTitle(string title, CancellationToken cancellationToken)
    {
        return Optional(await context.VideoGames.FirstOrDefaultAsync(x => x.Title == title, cancellationToken));
    }

    public async Task<VideoGame> Add(VideoGame videoGame, CancellationToken cancellationToken)
    {
        await context.VideoGames.AddAsync(videoGame, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return videoGame;
    }

    public async Task<VideoGame> Update(VideoGame videoGame, CancellationToken cancellationToken)
    {
        context.VideoGames.Update(videoGame);
        await context.SaveChangesAsync(cancellationToken);
        return videoGame;
    }

    public async Task Delete(VideoGame videoGame, CancellationToken cancellationToken)
    {
        context.VideoGames.Remove(videoGame);
        await context.SaveChangesAsync(cancellationToken);
    }
}
