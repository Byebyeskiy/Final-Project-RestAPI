using Application.Common.Interfaces.Repositories;
using Application.Listings.Exceptions;
using Domain.Listings;
using FluentValidation;
using LanguageExt;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Listings.Commands;

public sealed record ReserveListingCommand(ListingId Id, int Quantity)
    : IRequest<Either<ListingException, ListingReservation>>;

public sealed class ReserveListingCommandValidator : AbstractValidator<ReserveListingCommand>
{
    public ReserveListingCommandValidator()
    {
        RuleFor(command => command.Quantity).GreaterThan(0);
    }
}

public sealed class ReserveListingCommandHandler(
    IListingRepository repository, ILogger<ReserveListingCommandHandler> logger)
    : IRequestHandler<ReserveListingCommand, Either<ListingException, ListingReservation>>
{
    public async Task<Either<ListingException, ListingReservation>> Handle(
        ReserveListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var reservation = await repository.TryReserve(request.Id, request.Quantity, cancellationToken);
            return await reservation.Match<Task<Either<ListingException, ListingReservation>>>(
                saved => Task.FromResult<Either<ListingException, ListingReservation>>(saved),
                async () =>
                {
                    var listing = await repository.GetById(request.Id, cancellationToken);
                    return listing.Match<Either<ListingException, ListingReservation>>(
                        _ => new ListingUnavailableException(request.Id),
                        () => new ListingNotFoundException(request.Id));
                });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Reservation failed for listing {ListingId}", request.Id);
            return new ListingUnhandledException(request.Id, exception);
        }
    }
}
