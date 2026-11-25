using Application.Common.Interfaces.Queries;
using Application.Common.Interfaces.Repositories;
using Domain.Listings;
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
        context.Listings.Update(listing);
        await context.SaveChangesAsync(cancellationToken);
        return listing;
    }

    public async Task Delete(Listing listing, CancellationToken cancellationToken)
    {
        context.Listings.Remove(listing);
        await context.SaveChangesAsync(cancellationToken);
    }
}
