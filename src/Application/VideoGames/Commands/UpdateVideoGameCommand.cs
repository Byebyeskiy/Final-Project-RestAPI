using Application.Common.Interfaces.Repositories;
using Domain.VideoGames;
using MediatR;

namespace Application.VideoGames.Commands;

public sealed record UpdateVideoGameCommand(Guid Id, string Title, decimal Price, DateTime ReleaseDate, Platform Platform)
    : IRequest<VideoGame?>;

public sealed class UpdateVideoGameCommandHandler(IVideoGameRepository repository)
    : IRequestHandler<UpdateVideoGameCommand, VideoGame?>
{
    public async Task<VideoGame?> Handle(UpdateVideoGameCommand request, CancellationToken cancellationToken)
    {
        var game = await repository.GetById(request.Id, cancellationToken);
        if (game is null)
        {
            return null;
        }

        var duplicate = await repository.GetByTitle(request.Title, cancellationToken);
        if (duplicate is not null && duplicate.Id != request.Id)
        {
            throw new ArgumentException($"Video game with title '{request.Title}' already exists");
        }

        game.UpdateDetails(request.Title, request.Price, request.ReleaseDate, request.Platform);
        return await repository.Update(game, cancellationToken);
    }
}
