using Application.Common.Interfaces.Repositories;
using Application.VideoGames.Exceptions;
using Domain.VideoGames;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using Unit = LanguageExt.Unit;

namespace Application.VideoGames.Commands;

public sealed record DeleteVideoGameCommand(VideoGameId Id)
    : IRequest<Either<VideoGameException, Unit>>;

public sealed class DeleteVideoGameCommandHandler(IVideoGameRepository repository, ILogger<DeleteVideoGameCommandHandler> logger)
    : IRequestHandler<DeleteVideoGameCommand, Either<VideoGameException, Unit>>
{
    public async Task<Either<VideoGameException, Unit>> Handle(DeleteVideoGameCommand request, CancellationToken cancellationToken)
    {
        var id = request.Id;
        try
        {
            var existing = await repository.GetById(id, cancellationToken);
            return await existing.Match<Task<Either<VideoGameException, Unit>>>(
                async game =>
                {
                    await repository.Delete(game, cancellationToken);
                    return Prelude.unit;
                },
                () => Task.FromResult<Either<VideoGameException, Unit>>(new VideoGameNotFoundException(id)));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Video game operation failed for {VideoGameId}", id);
            return new VideoGameUnhandledException(id, exception);
        }
    }
}
