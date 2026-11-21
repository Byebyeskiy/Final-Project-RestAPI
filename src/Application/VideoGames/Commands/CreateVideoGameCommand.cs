using Application.Common.Interfaces.Repositories;
using Application.VideoGames.Exceptions;
using Domain.VideoGames;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.VideoGames.Commands;

public sealed record CreateVideoGameCommand(string Title, decimal Price, DateTime ReleaseDate, Platform Platform)
    : IRequest<Either<VideoGameException, VideoGame>>;

public sealed class CreateVideoGameCommandHandler(IVideoGameRepository repository, ILogger<CreateVideoGameCommandHandler> logger)
    : IRequestHandler<CreateVideoGameCommand, Either<VideoGameException, VideoGame>>
{
    public async Task<Either<VideoGameException, VideoGame>> Handle(CreateVideoGameCommand request, CancellationToken cancellationToken)
    {
        var id = VideoGameId.New();
        try
        {
            var duplicate = await repository.GetByTitle(request.Title, cancellationToken);
            return await duplicate.Match<Task<Either<VideoGameException, VideoGame>>>(
                game => Task.FromResult<Either<VideoGameException, VideoGame>>(
                    new VideoGameAlreadyExistsException(game.Id, request.Title)),
                async () =>
                {
                    var game = VideoGame.New(id, request.Title, request.Price, request.ReleaseDate, request.Platform);
                    return await repository.Add(game, cancellationToken);
                });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Video game operation failed for {VideoGameId}", id);
            return new VideoGameUnhandledException(id, exception);
        }
    }
}
