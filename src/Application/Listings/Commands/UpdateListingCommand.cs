using Application.Common.Interfaces.Repositories;
using Application.Listings.Exceptions;
using Domain.Listings;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Listings.Commands;

public sealed record UpdateListingCommand(ListingId Id, string Title, decimal Price, DateTime AvailableSince, ListingCategory Category)
    : IRequest<Either<ListingException, Listing>>;

public sealed class UpdateListingCommandHandler(IListingRepository repository, ILogger<UpdateListingCommandHandler> logger)
    : IRequestHandler<UpdateListingCommand, Either<ListingException, Listing>>
{
    public async Task<Either<ListingException, Listing>> Handle(UpdateListingCommand request, CancellationToken cancellationToken)
    {
        var id = request.Id;
        try
        {
            var existing = await repository.GetById(id, cancellationToken);
            return await existing.Match<Task<Either<ListingException, Listing>>>(
                async game =>
                {
                    var duplicate = await repository.GetByTitle(request.Title, cancellationToken);
                    var conflict = duplicate.Filter(other => other.Id != id);
                    return await conflict.Match<Task<Either<ListingException, Listing>>>(
                        other => Task.FromResult<Either<ListingException, Listing>>(
                            new ListingAlreadyExistsException(other.Id, request.Title)),
                        async () =>
                        {
                            game.UpdateDetails(request.Title, request.Price, request.AvailableSince, request.Category);
                            return await repository.Update(game, cancellationToken);
                        });
                },
                () => Task.FromResult<Either<ListingException, Listing>>(new ListingNotFoundException(id)));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Listing operation failed for {ListingId}", id);
            return new ListingUnhandledException(id, exception);
        }
    }
}
