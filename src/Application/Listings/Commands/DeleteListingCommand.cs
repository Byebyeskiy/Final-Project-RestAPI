using Application.Common.Interfaces.Repositories;
using Application.Listings.Exceptions;
using Domain.Listings;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;
using Unit = LanguageExt.Unit;

namespace Application.Listings.Commands;

public sealed record DeleteListingCommand(ListingId Id)
    : IRequest<Either<ListingException, Unit>>;

public sealed class DeleteListingCommandHandler(IListingRepository repository, ILogger<DeleteListingCommandHandler> logger)
    : IRequestHandler<DeleteListingCommand, Either<ListingException, Unit>>
{
    public async Task<Either<ListingException, Unit>> Handle(DeleteListingCommand request, CancellationToken cancellationToken)
    {
        var id = request.Id;
        try
        {
            var existing = await repository.GetById(id, cancellationToken);
            return await existing.Match<Task<Either<ListingException, Unit>>>(
                async game =>
                {
                    await repository.Delete(game, cancellationToken);
                    return Prelude.unit;
                },
                () => Task.FromResult<Either<ListingException, Unit>>(new ListingNotFoundException(id)));
        }
        catch (ListingHasReservationsException exception)
        {
            return exception;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Listing operation failed for {ListingId}", id);
            return new ListingUnhandledException(id, exception);
        }
    }
}
