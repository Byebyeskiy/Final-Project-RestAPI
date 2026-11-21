using Application.Common.Interfaces.Repositories;
using Application.VideoGames.Exceptions;
using Domain.VideoGames;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.VideoGames.Commands;

public sealed record UpdateVideoGameCommand(VideoGameId Id, string Title, decimal Price, DateTime ReleaseDate, Platform Platform)
    : IRequest<Either<VideoGameException, VideoGame>>;

public sealed class UpdateVideoGameCommandHandler(IVideoGameRepository repository, ILogger<UpdateVideoGameCommandHandler> logger)
    : IRequestHandler<UpdateVideoGameCommand, Either<VideoGameException, VideoGame>>
{
    public async Task<Either<VideoGameException, VideoGame>> Handle(UpdateVideoGameCommand request, CancellationToken cancellationToken)
    {
        var id = request.Id;
        try
        {
            var existing = await repository.GetById(id, cancellationToken);
            return await existing.Match<Task<Either<VideoGameException, VideoGame>>>(
                async game =>
                {
                    var duplicate = await repository.GetByTitle(request.Title, cancellationToken);
                    var conflict = duplicate.Filter(other => other.Id != id);
                    return await conflict.Match<Task<Either<VideoGameException, VideoGame>>>(
                        other => Task.FromResult<Either<VideoGameException, VideoGame>>(
                            new VideoGameAlreadyExistsException(other.Id, request.Title)),
                        async () =>
                        {
                            game.UpdateDetails(request.Title, request.Price, request.ReleaseDate, request.Platform);
                            return await repository.Update(game, cancellationToken);
                        });
                },
                () => Task.FromResult<Either<VideoGameException, VideoGame>>(new VideoGameNotFoundException(id)));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Video game operation failed for {VideoGameId}", id);
            return new VideoGameUnhandledException(id, exception);
        }
    }
}
