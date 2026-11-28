using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Listings;
using Application.Listings.Exceptions;
using Infrastructure.Persistence.Configurations;
using Npgsql;
using LanguageExt;
using static LanguageExt.Prelude;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ListingRepository(ApplicationDbContext context) : IListingRepository, IListingQueries
{
    public async Task<IReadOnlyList<Listing>> GetAll(CancellationToken cancellationToken)
    {
        return await context.Listings.ToListAsync(cancellationToken);
    }

    public async Task<Option<Listing>> GetById(ListingId id, CancellationToken cancellationToken)
    {
        return Optional(await context.Listings.FirstOrDefaultAsync(x => x.Id == id, cancellationToken));
    }

    public async Task<Option<Listing>> GetByTitle(string title, CancellationToken cancellationToken)
    {
        return Optional(await context.Listings.FirstOrDefaultAsync(x => x.Title == title, cancellationToken));
    }

    public async Task<Listing> Add(Listing listing, CancellationToken cancellationToken)
    {
        await context.Listings.AddAsync(listing, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return listing;
    }

    public async Task<Listing> Update(Listing listing, CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
        return listing;
    }

    public async Task<bool> TryModerate(ListingId id, ListingStatus status, string? rejectionReason,
        CancellationToken cancellationToken)
    {
        var updated = await context.Listings
            .Where(listing => listing.Id == id && listing.Status == ListingStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(listing => listing.Status, status)
                .SetProperty(listing => listing.RejectionReason, rejectionReason), cancellationToken);

        return updated == 1;
    }

    public async Task<Option<ListingReservation>> TryReserve(ListingId id, int quantity,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // PostgreSQL rechecks the stock condition after a competing update releases its row lock.
        var updated = await context.Listings
            .Where(listing => listing.Id == id && listing.Status == ListingStatus.Approved &&
                listing.AvailableQuantity >= quantity)
            .ExecuteUpdateAsync(setters => setters.SetProperty(listing => listing.AvailableQuantity,
                listing => listing.AvailableQuantity - quantity), cancellationToken);

        if (updated == 0)
        {
            return None;
        }

        var reservation = new ListingReservation(Guid.NewGuid(), id, quantity, DateTime.UtcNow);
        context.ListingReservations.Add(reservation);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return reservation;
    }

    public async Task Delete(Listing listing, CancellationToken cancellationToken)
    {
        context.Listings.Remove(listing);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ForeignKeyViolation,
                ConstraintName: ListingReservationsConfiguration.ListingForeignKey
            })
        {
            throw new ListingHasReservationsException(listing.Id);
        }

    }
}
