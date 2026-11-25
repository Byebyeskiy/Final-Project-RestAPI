using Application.Common.Interfaces.Repositories;
using Application.Listings.Exceptions;
using Domain.Listings;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Listings.Commands;

public sealed record ModerateListingCommand(ListingId Id, ListingStatus Status, string? RejectionReason = null)
    : IRequest<Either<ListingException, Listing>>;

public sealed class ModerateListingCommandHandler(
    IListingRepository repository, ILogger<ModerateListingCommandHandler> logger)
    : IRequestHandler<ModerateListingCommand, Either<ListingException, Listing>>
{
    public async Task<Either<ListingException, Listing>> Handle(
        ModerateListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var applied = await repository.TryModerate(
                request.Id, request.Status, request.RejectionReason?.Trim(), cancellationToken);
            var listing = await repository.GetById(request.Id, cancellationToken);
            return listing.Match<Either<ListingException, Listing>>(
                found => applied
                    ? Either<ListingException, Listing>.Right(found)
                    : Either<ListingException, Listing>.Left(new ListingAlreadyModeratedException(request.Id)),
                () => new ListingNotFoundException(request.Id));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Moderation failed for listing {ListingId}", request.Id);
            return new ListingUnhandledException(request.Id, exception);
        }
    }
}
