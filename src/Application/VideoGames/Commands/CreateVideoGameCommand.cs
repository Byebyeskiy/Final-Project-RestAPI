using Application.Common.Interfaces.Repositories;
using Domain.VideoGames;
using MediatR;

namespace Application.VideoGames.Commands;

public sealed record CreateVideoGameCommand(string Title, decimal Price, DateTime ReleaseDate, Platform Platform)
    : IRequest<VideoGame>;

public sealed class CreateVideoGameCommandHandler(IVideoGameRepository repository)
    : IRequestHandler<CreateVideoGameCommand, VideoGame>
{
    public async Task<VideoGame> Handle(CreateVideoGameCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await repository.GetByTitle(request.Title, cancellationToken);
        if (duplicate is not null)
        {
            throw new ArgumentException($"Video game with title '{request.Title}' already exists");
        }

        var game = VideoGame.New(Guid.NewGuid(), request.Title, request.Price, request.ReleaseDate, request.Platform);
        return await repository.Add(game, cancellationToken);
    }
}
