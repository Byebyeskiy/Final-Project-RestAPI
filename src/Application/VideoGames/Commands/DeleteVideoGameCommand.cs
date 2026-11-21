using Application.Common.Interfaces.Repositories;
using Domain.VideoGames;
using MediatR;

namespace Application.VideoGames.Commands;

public sealed record DeleteVideoGameCommand(Guid Id)
    : IRequest<bool>;

public sealed class DeleteVideoGameCommandHandler(IVideoGameRepository repository)
    : IRequestHandler<DeleteVideoGameCommand, bool>
{
    public async Task<bool> Handle(DeleteVideoGameCommand request, CancellationToken cancellationToken)
    {
        var game = await repository.GetById(request.Id, cancellationToken);
        if (game is null)
        {
            return false;
        }

        await repository.Delete(game, cancellationToken);
        return true;
    }
}
