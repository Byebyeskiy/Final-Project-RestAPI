using Application.Common.Interfaces.Repositories;
using Application.Listings.Exceptions;
using Domain.Listings;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Listings.Commands;

public sealed record CreateListingCommand(string Title, decimal Price, DateTime AvailableSince, ListingCategory Category)
    : IRequest<Either<ListingException, Listing>>;

public sealed class CreateListingCommandHandler(IListingRepository repository, ILogger<CreateListingCommandHandler> logger)
    : IRequestHandler<CreateListingCommand, Either<ListingException, Listing>>
{
    public async Task<Either<ListingException, Listing>> Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        var id = ListingId.New();
        try
        {
            var duplicate = await repository.GetByTitle(request.Title, cancellationToken);
            return await duplicate.Match<Task<Either<ListingException, Listing>>>(
                game => Task.FromResult<Either<ListingException, Listing>>(
                    new ListingAlreadyExistsException(game.Id, request.Title)),
                async () =>
                {
                    var game = Listing.New(id, request.Title, request.Price, request.AvailableSince, request.Category);
                    return await repository.Add(game, cancellationToken);
                });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Listing operation failed for {ListingId}", id);
            return new ListingUnhandledException(id, exception);
        }
    }
}
